#pragma once

#include "judge_process.h"


bool judge(api_judge_info const & aji, api_judge_result & ajr);

void free_judge_result(api_judge_result & result);
















void cluck();
int get3();
int string_length(wchar_t *);

struct string_table
{
	wchar_t * str1;
	int length1;
	wchar_t * str2;
	int length2;
	wchar_t * str3;
	int length3;
};
bool concat_string_table(string_table const & table, wchar_t * result, int length);
bool concat_string_args(
	wchar_t const * str1, int length1, wchar_t const * str2, int length2, wchar_t const * str3, int length3, wchar_t * result, int length);