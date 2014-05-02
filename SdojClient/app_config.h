#pragma once

struct app_config : public std::enable_shared_from_this<app_config>
{
	wstring server;
	wstring username;
	wstring password;
	vector<byte> serverpk;

	static shared_ptr<app_config> from_file(string filename);
};