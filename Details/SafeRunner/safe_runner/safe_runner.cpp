#include "pch.h"
#include "safe_runner.h"

using namespace std;



api_judge_result judge(api_judge_info const & aji)
{
	judge_process ps{ aji };
	ps.execute();
	api_judge_result result = ps.get_result();
	return result;
}

void free_judge_result(api_judge_result & result)
{
	if (result.output != nullptr)
	{
		delete[] result.output;
		result.output = nullptr;
	}
}








void cluck()
{
}

int get3()
{
	return 3;
}

int string_length(wchar_t * str)
{
	if (str == nullptr) return -1;
	return wcslen(str);
}

bool concat_string_table(string_table const & table, wchar_t * result, int length)
{
	if (length < table.length1 + table.length2 + table.length3 + 1)
	{
		return false;
	}
	else
	{
		wstring str;
		str.append(table.str1, table.length1);
		str.append(table.str2, table.length2);
		str.append(table.str3, table.length3);
		wcsncpy_s(result, length, str.c_str(), str.size());

		return true;
	}
}

bool concat_string_args(
	wchar_t * const str1, int length1, 
	wchar_t * const str2, int length2, 
	wchar_t * const str3, int length3, 
	wchar_t * result, int length)
{
	if (length < length1 + length2 + length3 + 1)
	{
		return false;
	}
	else
	{
		wstring str;
		str.append(str1, length1);
		str.append(str2, length2);
		str.append(str3, length3);
		wcsncpy_s(result, length, str.c_str(), str.size());

		return true;
	}
}