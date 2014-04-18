#include "stdafx.h"
#include "app_config.h"
#include <boost\property_tree\xml_parser.hpp>

using namespace boost::property_tree;

void app_config::load_from_file(string filename)
{
	wptree pt;
	read_xml(filename, pt);
	server = pt.get<wstring>(L"config.server");
	port = pt.get<short>(L"config.port");
}
