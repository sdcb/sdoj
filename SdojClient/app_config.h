#pragma once

struct app_config
{
	wstring server;
	wstring username;
	wstring password;
	vector<byte> serverpk;

	void load_from_file(string filename);
};