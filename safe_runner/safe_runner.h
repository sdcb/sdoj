#pragma once

#include "judge_process.h"


bool judge(api_judge_info const & aji, api_judge_result & ajr);

void free_judge_result(api_judge_result & result);