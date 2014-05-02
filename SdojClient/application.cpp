#include "stdafx.h"
#include "application.h"

namespace signalr = MicrosoftAspNetSignalRClientCpp;

void application::run()
{
	config.load_from_file("config.xml");
	connection = make_shared<signalr::Connection>(config.server, L"", signalr::All);
	connection->Start().get();
}
