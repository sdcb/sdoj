#include "stdafx.h"
#include "sdoj_httpclient.h"
#include "app_config.h"
#include "winencrypt.h"
#include <boost\algorithm\hex.hpp>

using namespace std;
using namespace pplx;
using namespace utility;
using namespace web::http;
using namespace web::http::client;
using namespace MicrosoftAspNetSignalRClientCpp;
using namespace wincrypt;

sdoj_httpclient::sdoj_httpclient(shared_ptr<app_config> appconfig) :config(appconfig)
{
	/*auto p = open_provider(BCRYPT_ECDH_P521_ALGORITHM);
	auto pk = import_key(p, BCRYPT_ECCPUBLIC_BLOB, &config->serverpk[0], (unsigned)config->serverpk.size());
	wstring fkstr = L"RUNLNkIAAAAAJtrV6IAj27tbSGOpuNCd348bPWjyPBghTk4xeOcwWYl0hH7Y0xCHQm9VXyY9AgnwUpS1XPJnHNHycu7BwWoC06MAPai+3dM8iEm/5d1oFvyY1clRU8KusSNAHT15dogP5HX0ACTd2MO+xxygvmGfMIK+7kXts6WtwFP0H11Hn4jRWw4BTc/QOqAh9j9mDt/yQEaR+uKk0f0lJSerklhLorXLqTg5A2Uk+qxYDYrJTwFAoYQsLjhi7485mLrO5Ur7n5nScBY=";
	auto fkbytes = utility::conversions::from_base64(fkstr);
	auto fk = import_key(p, BCRYPT_ECCPRIVATE_BLOB, &fkbytes[0], fkbytes.size());
	auto pwd = get_agreement(fk, pk);
	auto pwdstr = utility::conversions::to_base64(pwd);*/

	auto p = open_provider(BCRYPT_SHA256_ALGORITHM);
	auto h = create_hash(p);
	string tobehashed = "y0usCAY+GMGXgOjjOuLWfCi/2yQ=";
	combine(h, &tobehashed[0], tobehashed.size());
	int size;
	get_property(h.get(), BCRYPT_HASH_LENGTH, size);
	vector<byte> hashed(size, '\0');
	get_value(h, &hashed[0], size);
	
	auto something = boost::algorithm::hex(hashed);
	auto hexstring = string(something.begin(), something.end());
	exit(0);
}

sdoj_httpclient::~sdoj_httpclient()
{
}

void sdoj_httpclient::Initialize(string_t uri)
{
	// Disabling the Http Client timeout by setting timeout to 0?
	http_client_config configuration = http_client_config();
	configuration.set_timeout(seconds(0));

	pClient = unique_ptr<http_client>(new http_client(uri, configuration));
}

task<http_response> sdoj_httpclient::Get(string_t uri, function<void(shared_ptr<HttpRequestWrapper>)> prepareRequest)
{
	if (prepareRequest == nullptr)
	{
		throw exception("ArgumentNullException: prepareRequest");
	}

	auto cts = make_shared<cancellation_token_source>();

	auto requestMessage = http_request(methods::GET);
	requestMessage.set_request_uri(uri);

	auto request = make_shared<HttpRequestWrapper>(requestMessage, [cts]()
	{
		cts->cancel();
	});

	prepareRequest(request);
	pplx::task_options readTaskOptions(cts->get_token());
	return pClient->request(requestMessage).then([](http_response response)
	{
		if (is_task_cancellation_requested())
		{
			cancel_current_task();
		}
		// check if the request was successful, temporary
		if (response.status_code() / 100 != 2)
		{
			throw exception("HttpClientException: Get Failed");
		}

		return response;
	}, readTaskOptions);
}

task<http_response> sdoj_httpclient::Post(string_t uri, function<void(shared_ptr<HttpRequestWrapper>)> prepareRequest)
{
	return Post(uri, prepareRequest, U(""));
}

task<http_response> sdoj_httpclient::Post(string_t uri, function<void(shared_ptr<HttpRequestWrapper>)> prepareRequest, string_t postData)
{
	if (prepareRequest == nullptr)
	{
		throw exception("ArgumentNullException: prepareRequest");
	}
	auto cts = make_shared<cancellation_token_source>();

	auto requestMessage = http_request(methods::POST);
	requestMessage.set_request_uri(uri);
	requestMessage.set_body(postData);

	auto request = make_shared<HttpRequestWrapper>(requestMessage, [cts]()
	{
		cts->cancel();
	});

	prepareRequest(request);
	pplx::task_options readTaskOptions(cts->get_token());
	return pClient->request(requestMessage).then([](http_response response)
	{
		if (is_task_cancellation_requested())
		{
			cancel_current_task();
		}
		// check if the request was successful, temporary
		if (response.status_code() / 100 != 2)
		{
			throw exception("HttpClientException: Post Failed");
		}

		return response;
	}, readTaskOptions);
}