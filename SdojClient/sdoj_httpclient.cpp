#include "stdafx.h"
#include "sdoj_httpclient.h"
#include <boost\algorithm\hex.hpp>

using namespace std;
using namespace pplx;
using namespace utility;
using namespace web;
using namespace web::http;
using namespace web::http::client;
using namespace MicrosoftAspNetSignalRClientCpp;
using namespace wincrypt;

sdoj_httpclient::sdoj_httpclient(shared_ptr<app_config> appconfig) :config(appconfig)
{
	// create public key for server recognize.
	auto p = open_provider(BCRYPT_ECDH_P521_ALGORITHM);
	auto fk = create_asymmetric_key(p);
	auto pk_bytes = export_key(fk, BCRYPT_ECCPUBLIC_BLOB);
	public_key = utility::conversions::to_base64(pk_bytes);

	// import agreemented password from client private key + server public key.
	auto pk = import_key(p, BCRYPT_ECCPUBLIC_BLOB, &config->serverpk[0], (unsigned)config->serverpk.size());
	auto pwd = get_agreement(fk, pk);

	// create aes encryptor and import hashed key.
	auto aesp = open_provider(BCRYPT_AES_ALGORITHM);
	aes_key = create_key(aesp, &pwd[0], (unsigned)pwd.size());

	// create a random generator.
	random_provider = open_provider(BCRYPT_RNG_ALGORITHM);
}

sdoj_httpclient::~sdoj_httpclient()
{
}

void sdoj_httpclient::prepare_headers(http_request & request)
{
	request.headers()[L"Public-Key"] = public_key;

	vector<byte> iv(16);
	random(random_provider, &iv[0], (unsigned)iv.size());
	auto ivstr = utility::conversions::to_base64(iv);
	request.headers()[L"IV"] = ivstr;

	json::value v;
	v[L"Email"] = json::value{ config->username };
	v[L"Password"] = json::value{ config->password };
	auto plaintext = v.serialize();
	auto encrypted = encrypt(aes_key, plaintext, iv, BCRYPT_BLOCK_PADDING);
	auto encrypted_str = utility::conversions::to_base64(encrypted);
	request.headers()[L"Security-Token"] = encrypted_str;
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
	prepare_headers(requestMessage);

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