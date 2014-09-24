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

enum class judge_result_types
{
	queuing = 0,
	juding = 1,
	completed = 100,
	compile_error = 101,
	accepted = 102,
	wrong_answer = 103,
	runtime_error = 104,
	time_limit_exceed = 105,
	memory_limit_exceed = 106,
};

struct api_judge_result
{
	judge_result_types result_type;
	int32_t time;
	float memory;
};

class judge_process
{
public:

	void execute();

	api_judge_result get_result();

private:

	judge_info m_judge_info;

	api_judge_result m_result;
};