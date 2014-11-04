#pragma once
#include "os_wrapper.h"

class run_process;

struct api_run_info
{
	wchar_t * path;

	int path_len;

	int32_t time_limit_ms;

	float memory_limit_mb;

	int limit_process_count;
};

struct api_run_io_result
{
	bool succeed;

	run_process * notuse;

	uint32_t error_code;

	HANDLE input_write_handle;

	HANDLE output_read_handle;

	HANDLE error_read_handle;
};

struct api_run_result
{
	bool succeed;

	uint32_t error_code;

	int32_t exit_code;

	int32_t time_ms;

	float memory_mb;
};

class run_process
{
public:
	run_process(api_run_info const & info);

	void begin_runs();

	void end_runs();

	void get_io_result(api_run_io_result & io_result);

	void get_result(api_run_result & result);

private:
	// input: 
	std::wstring m_path;

	int64_t m_time_limit;

	int64_t m_memory_limit;

	int m_limit_process_count;

	// io result:
	HANDLE m_input_write;

	HANDLE m_output_read;

	HANDLE m_error_read;

	// temp: 
	std::unique_ptr<process_information> m_pi;

	null_handle m_job;

	// result: 
	uint32_t m_error_code;

	DWORD m_exit_code;

	int32_t m_time_ms;

	int64_t m_memory;
};