#pragma once

struct api_judge_info
{
	wchar_t * path;
	int path_len;

	wchar_t * input;
	int input_len;

	wchar_t * output;
	int output_len;

	int32_t time_limit_ms;

	float memory_limit_mb;
};

struct judge_info
{
	judge_info(){}

	judge_info(api_judge_info const &);

	std::wstring path;
	std::wstring input;
	std::wstring output;
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
	wchar_t * error;
	wchar_t * exception;
};

class judge_process
{
public:

	judge_process(judge_info const & ji) :
		m_judge_info(ji)
	{
	}

	void execute();

	api_judge_result get_result();

private:

	judge_info const m_judge_info;

	api_judge_result m_result;
};