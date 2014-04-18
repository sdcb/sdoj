#include "stdafx.h"
#include "app_config.h"
#include <boost\property_tree\xml_parser.hpp>

using namespace boost::property_tree;

void app_config::load_from_file(string filename)
{
	wptree pt;
	read_xml(filename, pt);

	auto config = pt.get_child(L"A-Pa5sword-That:Never8eenUsed");

	server = config.get<wstring>(L"server");
	port = config.get<short>(L"port");
	username = config.get<wstring>(L"username");
	password = config.get<wstring>(L"password");
}
