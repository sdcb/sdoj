#pragma once

struct app_config
{
	wstring server;
	short port;
	wstring username;
	wstring password;
	vector<byte> serverpk;

	void load_from_file(string filename);
};