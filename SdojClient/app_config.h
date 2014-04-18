#pragma once

struct app_config
{
	wstring server;
	short port;
	wstring username;
	wstring password;

	void load_from_file(string filename);
};