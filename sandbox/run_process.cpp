#include "pch.h"
#include "run_process.h"

using namespace std;


run_process::run_process(api_run_info const & info) :
	m_path(info.path, info.path_len),
	m_time_limit(ms_to_ns100(info.time_limit_ms)),
	m_memory_limit(static_cast<int64_t>((double)info.memory_limit_mb * 1024 * 1024)), 
	m_limit_process_count(info.limit_process_count)
{
}



void run_process::begin_run()
{
	null_handle m_job{ create_job_object(m_memory_limit, m_time_limit, m_limit_process_count) };
	pipe_handles pipe_handles;
	m_pi = move(create_security_process(&m_path[0], 
										pipe_handles.in_read.get(), 
										pipe_handles.out_write.get(), 
										pipe_handles.err_write.get(), 
										m_job.get()));

	// write task & read task in upper level application.
	m_input_write.reset( pipe_handles.in_write.release() );
	m_output_read.reset( pipe_handles.out_read.release() );
	m_error_read.reset( pipe_handles.err_read.release() );
}



void run_process::end_run()
{
	ThrowIfFailed(ResumeThread(m_pi.thread_handle.get()));

	LARGE_INTEGER freq, wait_start, wait_end;
	QueryPerformanceCounter(&freq);
	QueryPerformanceCounter(&wait_start);

	DWORD wait_result = WaitForSingleObject(m_pi.process_handle.get(), 
											static_cast<DWORD>(ns100_to_ms( m_time_limit )));
	QueryPerformanceCounter(&wait_end);
	int64_t wait_ms = 1000LL * (wait_end.QuadPart - wait_start.QuadPart) / freq.QuadPart;

	BOOL ok = GetExitCodeProcess(m_pi.process_handle.get(), &m_exit_code);

	JOBOBJECT_EXTENDED_LIMIT_INFORMATION exinfo;
	ThrowIfFailed(QueryInformationJobObject(m_job.get(), 
											JobObjectExtendedLimitInformation, 
											&exinfo, 
											sizeof(exinfo), 
											nullptr));

	if (wait_result == WAIT_TIMEOUT && wait_ms < ns100_to_ms(m_time_limit))
	{
		m_time_ms = static_cast<int32_t>(ns100_to_ms(m_time_limit));
	}
	else
	{
		m_time_ms = static_cast<int32_t>(wait_ms);
	}
}



void run_process::get_io_result(api_run_io_result & io_result)
{
	io_result.error_code = m_error_code;
	io_result.error_read_handle = m_error_read.get();
	io_result.output_read_handle = m_output_read.get();
	io_result.input_write_handle = m_input_write.get();
	io_result.notuse = this;
}



void run_process::get_result(api_run_result & result)
{
	result.error_code = m_error_code;
	result.exit_code = m_exit_code;
	result.memory_mb = m_memory_mb/1024/1024.0f;
	result.time_ms = m_time_ms;
}