#include "pch.h"
#include "run_process.h"
#include "os_wrapper.h"

using namespace std;



void run_process::begin_run()
{

}



void run_process::end_run()
{

}



void run_process::get_io_result(api_run_io_result & io_result)
{
	io_result.error_code = m_error_code;
	io_result.error_read_handle = m_error_read;
	io_result.output_read_handle = m_output_read;
	io_result.input_write_handle = m_input_write;
	io_result.notuse = this;
}



void run_process::get_result(api_run_result & result)
{
	result.error_code = m_error_code;
	result.except_code = m_except_code;
	result.exit_code = m_exit_code;
	result.memory_mb = m_memory_mb;
	result.time_ms = m_time_ms;
}