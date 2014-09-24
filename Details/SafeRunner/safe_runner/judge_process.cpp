#include "pch.h"
#include "judge_process.h"

judge_info::judge_info(api_judge_info const & aji) :
	path(aji.path, aji.path_len), 
	input(aji.input, aji.input_len), 
	output(aji.output, aji.output_len)
{
}