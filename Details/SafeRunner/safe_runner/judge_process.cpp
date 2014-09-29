#include "pch.h"
#include "handle.h"
#include "judge_process.h"

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
	redirect_process_attr_list(HANDLE in_read, HANDLE out_write, HANDLE err_write);

	~redirect_process_attr_list();

	redirect_process_attr_list(redirect_process_attr_list const &) = delete;

	redirect_process_attr_list operator=(redirect_process_attr_list const &) = delete;

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

inline int64_t ms_to_ns100(int ms);

inline int ns100_to_ms(int64_t ns100);

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

	ThrowIfFailed(WriteFile(pipe_handles.in_write.get(),
							m_judge_info.input.c_str(), 
							m_judge_info.input.size() * sizeof(wchar_t), 
							nullptr, 
							nullptr));
	pipe_handles.in_write.reset();

	ThrowIfFailed(ResumeThread(process_info.thread_handle.get()));

	char text[4096];
	DWORD read{};
	/*VERIFY(ReadFile(pipe_handles.out_read.get(), 
					(void*)text, 
					_countof(text)*sizeof(char), 
					&read, 
					nullptr));*/
	text[read] = '\0';

	int c;
	c = MultiByteToWideChar(CP_ACP, 0, text, read, nullptr, 0);
	output.resize(read + 1);
	MultiByteToWideChar(CP_ACP, 0, text, read, &output[0], output.size());

	WaitForSingleObject(process_info.process_handle.get(), static_cast<DWORD>(ns100_to_ms(m_judge_info.time_limit)));
	TerminateProcess(process_info.process_handle.get(), 0);

	JOBOBJECT_BASIC_ACCOUNTING_INFORMATION basic_info;
	ThrowIfFailed(QueryInformationJobObject(job.get(), JobObjectBasicAccountingInformation, &basic_info, sizeof(basic_info), nullptr));

	JOBOBJECT_EXTENDED_LIMIT_INFORMATION extend_info;
	ThrowIfFailed(QueryInformationJobObject(job.get(), JobObjectExtendedLimitInformation, &extend_info, sizeof(extend_info), nullptr));

	time = basic_info.TotalUserTime.QuadPart;
	memory = extend_info.PeakJobMemoryUsed;
}



void judge_process::get_result(api_judge_result & result)
{
	
	result.error_code  = error_code;
	result.except_code = except_code;
	result.memory      = memory/1024/1024.0f;
	result.time        = ns100_to_ms(time);

	if (output.size() > 0)
	{
		result.output = new wchar_t[output.size() + 1];
		wcscpy_s(result.output, output.size(), output.c_str());
	}

	if (error_code != 0 && error.size() > 0)
	{
		result.error = new wchar_t[error.size() + 1];
		wcscpy_s(result.error, error.size(), error.c_str());
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

	
	JOBOBJECT_EXTENDED_LIMIT_INFORMATION extend_limit{};
	extend_limit.ProcessMemoryLimit										= info.memory_limit;
	extend_limit.BasicLimitInformation.ActiveProcessLimit				= 1;
	extend_limit.BasicLimitInformation.PerProcessUserTimeLimit.QuadPart = info.time_limit;
	extend_limit.BasicLimitInformation.LimitFlags						= JOB_OBJECT_LIMIT_PROCESS_MEMORY             |
																		  JOB_OBJECT_LIMIT_ACTIVE_PROCESS             |
																		  JOB_OBJECT_LIMIT_PROCESS_TIME               |
																		  JOB_OBJECT_LIMIT_DIE_ON_UNHANDLED_EXCEPTION |
																		  JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE;
	ThrowIfFailed(SetInformationJobObject(job.get(), JobObjectExtendedLimitInformation, &extend_limit, sizeof(extend_limit)));
	

	JOBOBJECT_BASIC_UI_RESTRICTIONS ui_limit;
	ui_limit.UIRestrictionsClass = JOB_OBJECT_UILIMIT_HANDLES			|
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



inline int64_t ms_to_ns100(int ms)
{
	return ms * 10000L;
}



inline int ns100_to_ms(int64_t ns100)
{
	return static_cast<int>(ns100 / 10000);
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
	redirect_process_attr_list attr_list{ in_read, out_write, err_write };
	
	PROCESS_INFORMATION pi;
	STARTUPINFOEX si{};
	si.StartupInfo.cb = sizeof(si);
	si.StartupInfo.hStdInput = in_read;
	si.StartupInfo.hStdOutput = out_write;
	si.StartupInfo.hStdError = err_write;

	unsigned long flags = CREATE_SUSPENDED        | 
						  /*DEBUG_ONLY_THIS_PROCESS | */
						  /*CREATE_NO_WINDOW        | */
						  EXTENDED_STARTUPINFO_PRESENT;
	ThrowIfFailed(CreateProcessAsUser(token.get(), 
									  nullptr, 
									  &cmd[0], 
									  nullptr, 
									  nullptr, 
									  true, 
									  flags, 
									  nullptr, 
									  nullptr, 
									  &si.StartupInfo, 
									  &pi));

	ThrowIfFailed(AssignProcessToJobObject(job, pi.hProcess));

	return process_information{ pi };
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



redirect_process_attr_list::redirect_process_attr_list(HANDLE in_read, HANDLE out_write, HANDLE err_write)
{
	unsigned long size;
	ThrowIfFailed(InitializeProcThreadAttributeList(nullptr, 1, 0, &size) || GetLastError() == ERROR_INSUFFICIENT_BUFFER);

	LPPROC_THREAD_ATTRIBUTE_LIST attr_list;
	attr_list = reinterpret_cast<LPPROC_THREAD_ATTRIBUTE_LIST>(HeapAlloc(GetProcessHeap(), 0, size));
	ThrowIfFailed(attr_list);

	ThrowIfFailed(InitializeProcThreadAttributeList(attr_list, 1, 0, &size));

	HANDLE handles[] = { in_read, out_write, err_write };
	ThrowIfFailed(UpdateProcThreadAttribute(attr_list,
											0, 
											PROC_THREAD_ATTRIBUTE_HANDLE_LIST,
											handles,
											sizeof(handles), 
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