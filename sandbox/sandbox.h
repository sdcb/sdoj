#pragma once

#include "judge_process.h"
#include "run_process.h"

bool judge(api_judge_info const & aji, api_judge_result & ajr);

void free_judge_result(api_judge_result & result);

void __stdcall begin_run(api_run_info const & info, api_run_io_result & io_result);

void __stdcall end_run(run_process * run_process, api_run_result result);