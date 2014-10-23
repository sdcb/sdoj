#pragma once

class run_process;

struct api_run_info
{
	wchar_t * path;

	int path_len;

	int32_t time_limit_ms;

	float memory_limit_mb;
};

struct api_run_io_result
{
	bool succeed;

	run_process * notuse;

	uint32_t error_code;

	intptr_t input_write_handle;

	intptr_t output_read_handle;

	intptr_t error_read_handle;
};

struct api_run_result
{
	bool succeed;

	uint32_t error_code;

	uint32_t except_code;

	int32_t exit_code;

	int32_t timeMs;

	float memoryMb;
};

class run_process
{
public:
	run_process(api_run_info const & info) 
	{
	}

	void run();

	void wait();

	void get_io_result(api_run_io_result & io_result);

	void get_result(api_run_result & result);

private:
	// input: 
	std::wstring m_path;

	int32_t m_time_limit_ms;

	float m_memory_limit_mb;

	// io result:
	HANDLE m_input_write;

	HANDLE m_output_read;

	HANDLE m_error_read;

	// result: 
	uint32_t m_error_code;

	uint32_t m_except_code;

	DWORD m_exit_code;

	int32_t m_timeMs;

	int64_t m_memory;
};