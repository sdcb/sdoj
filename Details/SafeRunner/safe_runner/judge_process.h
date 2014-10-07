#pragma once

struct api_judge_info
{
	wchar_t * path;

	int path_len;

	wchar_t * input;

	int input_len;

	int32_t time_limit_ms;

	float memory_limit_mb;
};

struct judge_info
{
	judge_info(){}

	judge_info(api_judge_info const &);

	std::wstring path;
	std::wstring input;
	int64_t time_limit;
	size_t memory_limit;
};

struct api_judge_result
{
	uint32_t error_code;
	uint32_t except_code;
	int32_t time;
	float memory;
	wchar_t * output;
	wchar_t * exception;
};

class judge_process
{
public:

	judge_process(judge_info const & ji) :
		m_judge_info(ji), 
		error_code(0), 
		except_code(0), 
		time(0), 
		memory(0)
	{
	}

	void execute();

	void get_result(api_judge_result &);

private:

	judge_info m_judge_info;

	std::wstring error;

	std::wstring exception;

	std::wstring output;

	uint32_t error_code;

	uint32_t except_code;

	int64_t time;

	int64_t memory;
};