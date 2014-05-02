#pragma once

#include "app_config.h"
#include "winencrypt.h"

class sdoj_httpclient : public MicrosoftAspNetSignalRClientCpp::IHttpClient
{
public:
	sdoj_httpclient(shared_ptr<app_config> appconfig);
	~sdoj_httpclient();

	void Initialize(utility::string_t uri);
	pplx::task<web::http::http_response> Get(utility::string_t uri, std::function<void(std::shared_ptr<MicrosoftAspNetSignalRClientCpp::HttpRequestWrapper>)> prepareRequest);
	pplx::task<web::http::http_response> Post(utility::string_t uri, std::function<void(std::shared_ptr<MicrosoftAspNetSignalRClientCpp::HttpRequestWrapper>)> prepareRequest);
	pplx::task<web::http::http_response> Post(utility::string_t uri, std::function<void(std::shared_ptr<MicrosoftAspNetSignalRClientCpp::HttpRequestWrapper>)> prepareRequest, utility::string_t postData);

private:

	std::unique_ptr<web::http::client::http_client> pClient;
	shared_ptr<app_config> config;
	vector<byte> public_key;
	wincrypt::key aes_key;
};