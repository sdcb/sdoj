#pragma once

#include <cpprest\http_client.h>

class sdoj_httpclient : public MicrosoftAspNetSignalRClientCpp::IHttpClient
{
public:
	sdoj_httpclient();
	~sdoj_httpclient();

	void Initialize(utility::string_t uri);
	pplx::task<web::http::http_response> Get(utility::string_t uri, std::function<void(std::shared_ptr<MicrosoftAspNetSignalRClientCpp::HttpRequestWrapper>)> prepareRequest);
	pplx::task<web::http::http_response> Post(utility::string_t uri, std::function<void(std::shared_ptr<MicrosoftAspNetSignalRClientCpp::HttpRequestWrapper>)> prepareRequest);
	pplx::task<web::http::http_response> Post(utility::string_t uri, std::function<void(std::shared_ptr<MicrosoftAspNetSignalRClientCpp::HttpRequestWrapper>)> prepareRequest, utility::string_t postData);

private:
	std::unique_ptr<web::http::client::http_client> pClient;
};