#pragma once

#include "app_config.h"
#include "winencrypt.h"

class sdoj_httpclient : public MicrosoftAspNetSignalRClientCpp::IHttpClient
{
public:
	sdoj_httpclient(shared_ptr<app_config> appconfig);
	~sdoj_httpclient();

	void Initialize(utility::string_t uri);
	void Login();
	pplx::task<web::http::http_response> Get(utility::string_t uri, std::function<void(std::shared_ptr<MicrosoftAspNetSignalRClientCpp::HttpRequestWrapper>)> prepareRequest);
	pplx::task<web::http::http_response> Post(utility::string_t uri, std::function<void(std::shared_ptr<MicrosoftAspNetSignalRClientCpp::HttpRequestWrapper>)> prepareRequest);
	pplx::task<web::http::http_response> Post(utility::string_t uri, std::function<void(std::shared_ptr<MicrosoftAspNetSignalRClientCpp::HttpRequestWrapper>)> prepareRequest, utility::string_t postData);

private:

	void prepare_headers(web::http::http_request & request);

	std::unique_ptr<web::http::client::http_client> client;
	shared_ptr<app_config> config;
	wstring public_key;
	wincrypt::key aes_key;
	wincrypt::provider random_provider;
};