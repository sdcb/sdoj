#include "pch.h"
#include "judge_process.h"
#include "os_wrapper.h"



judge_info::judge_info(api_judge_info const & aji) :
	path(aji.path, aji.path_len), 
	input(aji.input, aji.input_len), 
	time_limit(ms_to_ns100(aji.time_limit_ms)),
	memory_limit(static_cast<size_t>(aji.memory_limit_mb * 1024.0 * 1024))
{
}



void judge_process::execute()
{
	null_handle job{ create_job_object(m_judge_info.memory_limit, m_judge_info.time_limit) };
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