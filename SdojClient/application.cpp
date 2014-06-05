#include "stdafx.h"
#include "application.h"

namespace signalr = MicrosoftAspNetSignalRClientCpp;

void application::run()
{
	config = app_config::from_file("config.xml");
	connection = make_shared<signalr::Connection>(config->server);
	connection->SetReceivedCallback([](wstring s){
		wcout << s << endl;
	});

	connection->SetReconnectingCallback([]{
		wcout << "Reconnecting..." << endl;
	});
	connection->SetReconnectedCallback([]{
		wcout << "Reconnected!" << endl;
	});
	connection->SetClosedCallback([]{
		wcout << "Closed" << endl;
	});

	httpclient.swap(make_shared<sdoj_httpclient>(config));
	httpclient->Login();
	connection->Start(httpclient).get();
	
	

	getchar();
}
