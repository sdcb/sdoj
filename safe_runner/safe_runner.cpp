#include "pch.h"
#include "safe_runner.h"
#include "debug.h"

using namespace std;



bool judge(api_judge_info const & aji, api_judge_result & ajr)
{
	try
	{
		judge_process ps{ aji };
		ps.execute();
		ps.get_result(ajr);
		return true;
	}
	catch (win32_exception const & e)
	{
		ajr.error_code = e.error_code;
		return false;
	}
	catch (...)
	{
		return false;
	}
}

void free_judge_result(api_judge_result & result)
{
	if (result.output != nullptr)
	{
		delete[] result.output;
		result.output = nullptr;
	}

	if (result.exception != nullptr)
	{
		delete[] result.exception;
		result.exception = nullptr;
	}
}