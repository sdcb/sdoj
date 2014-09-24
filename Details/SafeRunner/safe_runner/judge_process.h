#pragma once

struct api_judge_info
{
	wchar_t * path;
	int path_len;

	wchar_t * input;
	int input_len;

	wchar_t * output;
	int output_len;

	int32_t time_limit;

	float memory_limit;
};

struct judge_info
{
	judge_info(){}

	judge_info(api_judge_info const &);

	std::wstring path;
	std::wstring input;
	std::wstring output;
	int32_t time_limit;
	int64_t memory_limit;
};

struct api_judge_result
{
	uint32_t error_code;
	uint32_t except_code;
	int32_t time;
	float memory;
	wchar_t * output;
};

class judge_process
{
public:

	judge_process(judge_info const & ji) :
		judge_info(ji)
	{
	}

	void execute();

	api_judge_result get_result();

private:

	judge_info const judge_info;

	api_judge_result m_result;
};