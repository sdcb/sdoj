#pragma once
#include "app_config.h"
#include "sdoj_httpclient.h"

struct application
{
	shared_ptr<app_config> config;
	shared_ptr<MicrosoftAspNetSignalRClientCpp::Connection> connection;
	shared_ptr<sdoj_httpclient> httpclient;
	void run();
};