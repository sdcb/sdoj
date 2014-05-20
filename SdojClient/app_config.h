#pragma once

struct app_config : public std::enable_shared_from_this<app_config>
{
	wstring server;
	wstring username;
	wstring password;
	wstring login_url;
	vector<byte> serverpk;

	static shared_ptr<app_config> from_file(string filename);
};