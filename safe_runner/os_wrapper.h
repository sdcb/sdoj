#pragma once



struct pipe_handles
{
	pipe_handles();

	invalid_handle in_read, in_write;
	invalid_handle out_read, out_write;
	invalid_handle err_read, err_write;
};



struct redirect_process_attr_list
{
public:
	redirect_process_attr_list(HANDLE * handles, int handle_count);

	~redirect_process_attr_list();

	redirect_process_attr_list(redirect_process_attr_list const &) = delete;
	redirect_process_attr_list(redirect_process_attr_list &&) = delete;
	redirect_process_attr_list operator=(redirect_process_attr_list const &) = delete;
	redirect_process_attr_list operator=(redirect_process_attr_list &&) = delete;

	LPPROC_THREAD_ATTRIBUTE_LIST get();

private:
	LPPROC_THREAD_ATTRIBUTE_LIST m_attr_list;
};



struct process_information
{
	null_handle process_handle;
	null_handle thread_handle;
	uint32_t process_id;
	uint32_t thread_id;

	process_information(){}

	process_information(process_information const &) = delete;
	process_information operator=(process_information const &) = delete;

	process_information(process_information &&);

	process_information operator=(process_information &&);

	process_information(PROCESS_INFORMATION const &);
};



null_handle create_job_object(size_t memory_limit, int64_t time_limit);



#define ms_to_ns100(ms) ((ms)*10000LL)



#define ns100_to_ms(ns100) ((ns100)/10000)



std::wstring s2ws(const std::string& s);



std::string ws2s(const std::wstring& s);



process_information create_security_process(wchar_t * cmd,
	HANDLE in_read,
	HANDLE out_write,
	HANDLE err_write,
	HANDLE job);



null_handle create_security_process_token();