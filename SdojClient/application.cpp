#include "stdafx.h"
#include "application.h"
#include "sdoj_httpclient.h"

namespace signalr = MicrosoftAspNetSignalRClientCpp;

void application::run()
{
	config = app_config::from_file("config.xml");
	connection = make_shared<signalr::Connection>(config->server);
	connection->SetReceivedCallback([](wstring s){
		wcout << s << endl;
	});
	connection->Start(make_shared<sdoj_httpclient>(config)).get();
	
	getchar();
}
