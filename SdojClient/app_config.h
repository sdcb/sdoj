#pragma once

struct app_config
{
	wstring server;
	short port;
	void load_from_file(string filename);
};