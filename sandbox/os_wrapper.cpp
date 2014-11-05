#include "pch.h"
#include "os_wrapper.h"

null_handle create_job_object(size_t memory_limit, int64_t time_limit, int limit_process_count)
{
	null_handle job{ CreateJobObject(nullptr, nullptr) };
	ThrowIfFailed(job);

	// 注意：
	// 此处不能对JOB_OBJECT_LIMIT_PROCESS_MEMORY进行限制
	// 因为如果限制，则内存使用峰值的数值时将不正确。

	JOBOBJECT_EXTENDED_LIMIT_INFORMATION extend_limit{};

	extend_limit.JobMemoryLimit											= memory_limit;
	extend_limit.BasicLimitInformation.ActiveProcessLimit				= limit_process_count;
	extend_limit.BasicLimitInformation.PerProcessUserTimeLimit.QuadPart = time_limit;

	extend_limit.BasicLimitInformation.LimitFlags = JOB_OBJECT_LIMIT_JOB_MEMORY					|
													JOB_OBJECT_LIMIT_ACTIVE_PROCESS				|
													JOB_OBJECT_LIMIT_PROCESS_TIME				|
													JOB_OBJECT_LIMIT_DIE_ON_UNHANDLED_EXCEPTION |
													JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE			|
													0;

	ThrowIfFailed(SetInformationJobObject(job.get(), JobObjectExtendedLimitInformation, &extend_limit, sizeof(extend_limit)));


	JOBOBJECT_BASIC_UI_RESTRICTIONS ui_limit;
	ui_limit.UIRestrictionsClass = 0									|
								   JOB_OBJECT_UILIMIT_HANDLES			|
								   JOB_OBJECT_UILIMIT_READCLIPBOARD		|
								   JOB_OBJECT_UILIMIT_WRITECLIPBOARD	|
								   JOB_OBJECT_UILIMIT_SYSTEMPARAMETERS	|
								   JOB_OBJECT_UILIMIT_DISPLAYSETTINGS	|
								   JOB_OBJECT_UILIMIT_GLOBALATOMS		|
								   JOB_OBJECT_UILIMIT_DESKTOP			|
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



std::unique_ptr<process_information> create_security_process(wchar_t * cmd,
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

	unsigned long flags = CREATE_SUSPENDED		|
						  CREATE_NO_WINDOW		|
						  HIGH_PRIORITY_CLASS	|
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

	return std::make_unique<process_information>(pi);
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

	ThrowIfFailed( FreeSid(sid) == nullptr );

	return newtoken;
}



process_information::process_information(process_information && that)
{
	ThrowIfFailed(process_handle.reset(that.process_handle.release()));
	ThrowIfFailed(thread_handle.reset(that.thread_handle.release()));
	process_id = that.process_id;
	thread_id = that.thread_id;
}



process_information process_information::operator=(process_information && that)
{
	process_information pi;
	pi.process_handle.reset(that.process_handle.release());
	pi.thread_handle.reset(that.process_handle.release());
	pi.process_id = that.process_id;
	pi.thread_id = that.thread_id;
	return pi;
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