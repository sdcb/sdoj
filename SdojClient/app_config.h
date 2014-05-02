#pragma once

struct app_config : public std::enable_shared_from_this<app_config>
{
	wstring server;
	wstring username;
	wstring password;
	vector<byte> serverpk;

	void load_from_file(string filename);
};