#include "pch.h"
#include "handle.h"
#include "judge_process.h"

#include <Psapi.h>

struct pipe_handles
{
	pipe_handles();

	invalid_handle in_read, in_write;
	invalid_handle out_read, out_write;
	invalid_handle err_read, err_write;
};


struct redirect_process_attr_list
{
public:
	redirect_process_attr_list(HANDLE * handles, int handle_count);

	~redirect_process_attr_list();

	redirect_process_attr_list(redirect_process_attr_list const &) = delete;
	redirect_process_attr_list(redirect_process_attr_list &&) = delete;
	redirect_process_attr_list operator=(redirect_process_attr_list const &) = delete;
	redirect_process_attr_list operator=(redirect_process_attr_list &&) = delete;

	LPPROC_THREAD_ATTRIBUTE_LIST get();

private:
	LPPROC_THREAD_ATTRIBUTE_LIST m_attr_list;
};


struct process_information
{
	null_handle process_handle;
	null_handle thread_handle;
	uint32_t process_id;
	uint32_t thread_id;

	process_information(process_information const &) = delete;
	process_information operator=(process_information const &) = delete;

	process_information(process_information &&);

	process_information(PROCESS_INFORMATION const &);
};


null_handle create_job_object(judge_info const &);

#define ms_to_ns100(ms) ((ms)*10000LL)

#define ns100_to_ms(ns100) ((ns100)/10000)

std::wstring s2ws(const std::string& s);

std::string ws2s(const std::wstring& s);

process_information create_security_process(wchar_t * cmd, 
											HANDLE in_read, 
											HANDLE out_write, 
											HANDLE err_write, 
											HANDLE job);

null_handle create_security_process_token();


judge_info::judge_info(api_judge_info const & aji) :
	path(aji.path, aji.path_len), 
	input(aji.input, aji.input_len), 
	time_limit(ms_to_ns100(aji.time_limit_ms)),
	memory_limit(static_cast<size_t>(aji.memory_limit_mb * 1024.0 * 1024))
{
}



void judge_process::execute()
{
	null_handle job{ create_job_object(m_judge_info) };
	pipe_handles pipe_handles;
	process_information process_info{ create_security_process(&m_judge_info.path[0], 
															  pipe_handles.in_read.get(), 
															  pipe_handles.out_write.get(), 
															  pipe_handles.err_write.get(), 
															  job.get()) };

	// write task
	auto write_task = std::thread([&](){
		DWORD writed;
		std::string ansi_string = ws2s(m_judge_info.input);
		size_t length = ansi_string.length();
		if (*ansi_string.rbegin() == '\0') --length;

		BOOL result = WriteFile(pipe_handles.in_write.get(),
								ansi_string.c_str(),
								static_cast<DWORD>(length),
								&writed,
								nullptr);

		pipe_handles.in_write.reset();
	});
	
	ThrowIfFailed(ResumeThread(process_info.thread_handle.get()));

	// read task
	auto read_task = std::thread([&](){
		char buffer[4096];
		DWORD read;
		std::string ansi_buffer;

		while (true)
		{
			BOOL result = ReadFile(pipe_handles.out_read.get(),
								   buffer,
								   sizeof(buffer)-1,
								   &read,
								   nullptr);

			if (result)
			{
				buffer[read] = '\0';

				ansi_buffer.append(buffer, read);
			}
			else if (GetLastError() == ERROR_OPERATION_ABORTED)
			{
				break;
			}
		}

		output.assign(s2ws(ansi_buffer));
	});
	
	LARGE_INTEGER freq, wait_start, wait_end;
	QueryPerformanceFrequency(&freq);
	QueryPerformanceCounter(&wait_start);

	// wait the process ( terminate in job close )
	DWORD wait_result = WaitForSingleObject(process_info.process_handle.get(), 
		static_cast<DWORD>(ns100_to_ms(m_judge_info.time_limit)));

	QueryPerformanceCounter(&wait_end);
	auto microsecond = 1000LL * (wait_end.QuadPart - wait_start.QuadPart) / freq.QuadPart;

	BOOL ok = GetExitCodeProcess(process_info.process_handle.get(), &exit_code);

	// terminate io.
	{
		BOOL cancel_result1, cancel_result2;
		cancel_result2 = CancelSynchronousIo(write_task.native_handle());
		cancel_result1 = CancelSynchronousIo(read_task.native_handle());
		write_task.join();
		read_task.join();
	}

	//JOBOBJECT_BASIC_ACCOUNTING_INFORMATION basic_info;
	//ThrowIfFailed(QueryInformationJobObject(job.get(), JobObjectBasicAccountingInformation, &basic_info, sizeof(basic_info), nullptr));

	JOBOBJECT_EXTENDED_LIMIT_INFORMATION extend_info;
	ThrowIfFailed(QueryInformationJobObject(job.get(), JobObjectExtendedLimitInformation, &extend_info, sizeof(extend_info), nullptr));

	if (wait_result == WAIT_TIMEOUT)
	{
		timeMs = static_cast<int32_t>( ns100_to_ms(m_judge_info.time_limit) );
	}
	else if (wait_result == WAIT_OBJECT_0)
	{
		timeMs = static_cast<int32_t>( microsecond );
	}
	memory = extend_info.PeakJobMemoryUsed;
}



void judge_process::get_result(api_judge_result & result)
{
	
	result.error_code  = error_code;
	result.exit_code   = exit_code;
	result.except_code = except_code;
	result.memory      = memory/1024/1024.0f;
	result.time        = timeMs;

	if (output.size() > 0)
	{
		result.output = new wchar_t[output.size() + 1];
		wcscpy_s(result.output, output.size(), output.c_str());
	}
	
	if (except_code != 0 && exception.size() > 0)
	{
		result.exception = new wchar_t[exception.size() + 1];
		wcscpy_s(result.exception, exception.size(), exception.c_str());
	}
}



null_handle create_job_object(judge_info const & info)
{
	null_handle job{ CreateJobObject(nullptr, nullptr) };
	ThrowIfFailed(job);

	// 注意：
	// 此处不能对JOB_OBJECT_LIMIT_PROCESS_MEMORY进行限制
	// 因为如果限制，则内存使用峰值的数值时将不正确。

	JOBOBJECT_EXTENDED_LIMIT_INFORMATION extend_limit{};
	extend_limit.JobMemoryLimit											= info.memory_limit;
	/*extend_limit.ProcessMemoryLimit                                     = info.memory_limit;*/
	extend_limit.BasicLimitInformation.ActiveProcessLimit				= 1;
	extend_limit.BasicLimitInformation.PerProcessUserTimeLimit.QuadPart = info.time_limit;
	extend_limit.BasicLimitInformation.LimitFlags						= JOB_OBJECT_LIMIT_JOB_MEMORY                 |
																		  /*JOB_OBJECT_LIMIT_PROCESS_MEMORY             |*/
																		  JOB_OBJECT_LIMIT_ACTIVE_PROCESS             |
																		  JOB_OBJECT_LIMIT_PROCESS_TIME               |
																		  JOB_OBJECT_LIMIT_DIE_ON_UNHANDLED_EXCEPTION |
																		  JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE          |
																		  0;
	ThrowIfFailed(SetInformationJobObject(job.get(), JobObjectExtendedLimitInformation, &extend_limit, sizeof(extend_limit)));
	

	JOBOBJECT_BASIC_UI_RESTRICTIONS ui_limit;
	ui_limit.UIRestrictionsClass = 0                                    |
								   JOB_OBJECT_UILIMIT_HANDLES			|
								   JOB_OBJECT_UILIMIT_READCLIPBOARD   	|
								   JOB_OBJECT_UILIMIT_WRITECLIPBOARD  	|
								   JOB_OBJECT_UILIMIT_SYSTEMPARAMETERS	|
								   JOB_OBJECT_UILIMIT_DISPLAYSETTINGS 	|
								   JOB_OBJECT_UILIMIT_GLOBALATOMS     	|
								   JOB_OBJECT_UILIMIT_DESKTOP         	|
								   JOB_OBJECT_UILIMIT_EXITWINDOWS;
	ThrowIfFailed(SetInformationJobObject(job.get(), JobObjectBasicUIRestrictions, &ui_limit, sizeof(ui_limit)));
	

	JOBOBJECT_END_OF_JOB_TIME_INFORMATION end_info;
	end_info.EndOfJobTimeAction = JOB_OBJECT_TERMINATE_AT_END_OF_JOB;
	ThrowIfFailed(SetInformationJobObject(job.get(), JobObjectEndOfJobTimeInformation, &end_info, sizeof(end_info)));


	return job;
}



pipe_handles::pipe_handles()
{
	ThrowIfFailed(CreatePipe(in_read.get_address_of(), in_write.get_address_of(), nullptr, 0));
	ThrowIfFailed(CreatePipe(out_read.get_address_of(), out_write.get_address_of(), nullptr, 0));
	ThrowIfFailed(CreatePipe(err_read.get_address_of(), err_write.get_address_of(), nullptr, 0));

	ThrowIfFailed(SetHandleInformation(in_read.get(), HANDLE_FLAG_INHERIT, HANDLE_FLAG_INHERIT));
	ThrowIfFailed(SetHandleInformation(out_write.get(), HANDLE_FLAG_INHERIT, HANDLE_FLAG_INHERIT));
	ThrowIfFailed(SetHandleInformation(err_write.get(), HANDLE_FLAG_INHERIT, HANDLE_FLAG_INHERIT));
}



process_information create_security_process(wchar_t * cmd, 
											HANDLE in_read, 
											HANDLE out_write, 
											HANDLE err_write, 
											HANDLE job)
{
	ASSERT(cmd != nullptr);
	ASSERT(in_read != INVALID_HANDLE_VALUE);
	ASSERT(out_write != INVALID_HANDLE_VALUE);
	ASSERT(err_write != INVALID_HANDLE_VALUE);
	ASSERT(job != nullptr);

	null_handle token{ create_security_process_token() };
	HANDLE handles[] { in_read, out_write, err_write };
	redirect_process_attr_list attr_list{ handles, _countof(handles) };
	
	PROCESS_INFORMATION pi;
	STARTUPINFOEX si{};
	si.StartupInfo.cb = sizeof(si);
	si.StartupInfo.hStdInput = in_read;
	si.StartupInfo.hStdOutput = out_write;
	si.StartupInfo.hStdError = err_write;
	si.StartupInfo.dwFlags |= STARTF_USESTDHANDLES;
	si.lpAttributeList = attr_list.get();

	unsigned long flags = CREATE_SUSPENDED        | 
						  /*DEBUG_ONLY_THIS_PROCESS | */
						  CREATE_NO_WINDOW        | 
						  EXTENDED_STARTUPINFO_PRESENT;
	ThrowIfFailed(CreateProcessAsUser(token.get(), 
									  nullptr, 
									  &cmd[0], 
									  nullptr, 
									  nullptr, 
									  TRUE, 
									  flags, 
									  nullptr, 
									  nullptr, 
									  reinterpret_cast<LPSTARTUPINFO>(&si), 
									  &pi));

	ThrowIfFailed(AssignProcessToJobObject(job, pi.hProcess));

	return process_information{ pi };
}



redirect_process_attr_list::redirect_process_attr_list(HANDLE * handles, int handle_count)
{
	SIZE_T size = 0;
	ThrowIfFailed(InitializeProcThreadAttributeList(nullptr, 1, 0, &size) || GetLastError() == ERROR_INSUFFICIENT_BUFFER);

	LPPROC_THREAD_ATTRIBUTE_LIST attr_list;
	attr_list = reinterpret_cast<LPPROC_THREAD_ATTRIBUTE_LIST>(HeapAlloc(GetProcessHeap(), 0, size));
	ThrowIfFailed(attr_list);

	ThrowIfFailed(InitializeProcThreadAttributeList(attr_list, 1, 0, &size));

	ThrowIfFailed(UpdateProcThreadAttribute(attr_list,
											0, 
											PROC_THREAD_ATTRIBUTE_HANDLE_LIST,
											handles,
											sizeof(HANDLE)*handle_count, 
											nullptr, 
											nullptr));
	m_attr_list = attr_list;
}



redirect_process_attr_list::~redirect_process_attr_list()
{
	DeleteProcThreadAttributeList(m_attr_list);
	ThrowIfFailed(HeapFree(GetProcessHeap(), 0, m_attr_list));
}



LPPROC_THREAD_ATTRIBUTE_LIST redirect_process_attr_list::get()
{
	return m_attr_list;
}



null_handle create_security_process_token()
{
	null_handle pstoken, newtoken;
	ThrowIfFailed(OpenProcessToken(GetCurrentProcess(),
		TOKEN_DUPLICATE | TOKEN_QUERY | TOKEN_ADJUST_DEFAULT | TOKEN_ASSIGN_PRIMARY,
		pstoken.get_address_of()));

	ThrowIfFailed(DuplicateTokenEx(pstoken.get(),
		0,
		nullptr,
		SecurityImpersonation,
		TokenPrimary,
		newtoken.get_address_of()));

	SID_IDENTIFIER_AUTHORITY sia = SECURITY_MANDATORY_LABEL_AUTHORITY;
	PSID sid;
	ThrowIfFailed(AllocateAndInitializeSid(&sia,
		1,
		SECURITY_MANDATORY_LOW_RID,
		0, 0, 0, 0, 0, 0, 0,
		&sid));

	TOKEN_MANDATORY_LABEL tml{};
	tml.Label.Attributes = SE_GROUP_INTEGRITY;
	tml.Label.Sid = sid;
	ThrowIfFailed(SetTokenInformation(newtoken.get(),
		TokenIntegrityLevel,
		&tml,
		sizeof(tml) + GetLengthSid(sid)));

	return newtoken;
}



process_information::process_information(process_information && that)
{
	ThrowIfFailed(process_handle.reset(that.process_handle.release()));
	ThrowIfFailed(thread_handle.reset(that.thread_handle.release()));
}



process_information::process_information(PROCESS_INFORMATION const & that)
{
	ThrowIfFailed(process_handle.reset(that.hProcess));
	ThrowIfFailed(thread_handle.reset(that.hThread));
	process_id = that.dwProcessId;
	thread_id = that.dwThreadId;
}



std::wstring s2ws(const std::string& s)
{
	int len;
	int slength = (int)s.length() + 1;
	len = MultiByteToWideChar(CP_ACP, 0, s.c_str(), slength, 0, 0);
	std::wstring r(len, L'\0');
	MultiByteToWideChar(CP_ACP, 0, s.c_str(), slength, &r[0], len);
	return r;
}

std::string ws2s(const std::wstring& s)
{
	int len;
	int slength = (int)s.length() + 1;
	len = WideCharToMultiByte(CP_ACP, 0, s.c_str(), slength, 0, 0, 0, 0);
	std::string r(len, '\0');
	WideCharToMultiByte(CP_ACP, 0, s.c_str(), slength, &r[0], len, 0, 0);
	return r;
}