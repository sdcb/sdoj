#include "pch.h"
#include "handle.h"
#include "judge_process.h"

null_handle create_job_object(judge_info const &);

inline int64_t ms_to_ns100(int64_t);

inline int64_t ns100_to_ms(int64_t);



judge_info::judge_info(api_judge_info const & aji) :
	path(aji.path, aji.path_len), 
	input(aji.input, aji.input_len), 
	output(aji.output, aji.output_len), 
	time_limit(ms_to_ns100(aji.time_limit_ms)),
	memory_limit(static_cast<size_t>(aji.memory_limit_mb * 1024.0 * 1024))
{
}



void judge_process::execute()
{
	null_handle job{ create_job_object(m_judge_info) };
}



api_judge_result judge_process::get_result()
{
	return m_result;
}



null_handle create_job_object(judge_info const & info)
{
	null_handle job{ CreateJobObject(nullptr, nullptr) };
	VERIFY(job);


	JOBOBJECT_EXTENDED_LIMIT_INFORMATION extend_limit{};
	extend_limit.ProcessMemoryLimit										= info.memory_limit;
	extend_limit.BasicLimitInformation.ActiveProcessLimit				= 1;
	extend_limit.BasicLimitInformation.PerProcessUserTimeLimit.QuadPart = info.time_limit;
	extend_limit.BasicLimitInformation.LimitFlags						= JOB_OBJECT_LIMIT_PROCESS_MEMORY             |
																		  JOB_OBJECT_LIMIT_ACTIVE_PROCESS             |
																		  JOB_OBJECT_LIMIT_PROCESS_TIME               |
																		  JOB_OBJECT_LIMIT_DIE_ON_UNHANDLED_EXCEPTION |
																		  JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE;
	VERIFY(SetInformationJobObject(job.get(), JobObjectExtendedLimitInformation, &extend_limit, sizeof(extend_limit)));
	

	JOBOBJECT_BASIC_UI_RESTRICTIONS ui_limit;
	ui_limit.UIRestrictionsClass = JOB_OBJECT_UILIMIT_HANDLES			|
								   JOB_OBJECT_UILIMIT_READCLIPBOARD   	|
								   JOB_OBJECT_UILIMIT_WRITECLIPBOARD  	|
								   JOB_OBJECT_UILIMIT_SYSTEMPARAMETERS	|
								   JOB_OBJECT_UILIMIT_DISPLAYSETTINGS 	|
								   JOB_OBJECT_UILIMIT_GLOBALATOMS     	|
								   JOB_OBJECT_UILIMIT_DESKTOP         	|
								   JOB_OBJECT_UILIMIT_EXITWINDOWS;
	VERIFY(SetInformationJobObject(job.get(), JobObjectBasicUIRestrictions, &ui_limit, sizeof(ui_limit)));
	

	JOBOBJECT_END_OF_JOB_TIME_INFORMATION end_info;
	end_info.EndOfJobTimeAction = JOB_OBJECT_TERMINATE_AT_END_OF_JOB;
	VERIFY(SetInformationJobObject(job.get(), JobObjectEndOfJobTimeInformation, &end_info, sizeof(end_info)));


	return job;
}



inline int64_t ms_to_ns100(int64_t ms)
{
	return ms * 10000;
}



inline int64_t ns100_to_ms(int64_t ns100)
{
	return ns100 / 10000;
}