#include "stdafx.h"
#include "app_config.h"
#include <boost\property_tree\xml_parser.hpp>

using namespace boost::property_tree;

shared_ptr<app_config> app_config::from_file(string filename)
{
	wptree pt;
	read_xml(filename, pt);

	auto config = pt.get_child(L"A-Pa5sword-That:Never8eenUsed");
	auto obj = make_shared<app_config>();

	obj->server = config.get<wstring>(L"server");
	obj->username = config.get<wstring>(L"username");
	obj->password = config.get<wstring>(L"password");
	obj->login_url = config.get<wstring>(L"loginUrl");
	auto pk_string = config.get<wstring>(L"serverPublicKey");
	obj->serverpk = utility::conversions::from_base64(pk_string);

	return obj;
}
