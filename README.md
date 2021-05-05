# Token Proxy
Token Proxy allows the use of `client_credentials` grant in Logic Apps/Power Automate connectors via Basic Authentication headers and acting as a proxy to the target API.

The primary point is to provide the ability to use `client_credentials` flow in [Custom Connectors](https://docs.microsoft.com/en-us/connectors/custom-connectors/create-logic-apps-connector), which is not officially supported by Microsoft (if you need to authenticate with `client_credentials` against your own API protected with Azure AD, there is a native way to do this, blog post will eventually be made, however this tool can also be used to authenticate against Azure AD via `client_credentials` flow).

As previously mentioned, this solution acts as a proxy which accepts the `client_credentials` as [Basic authentication](https://docs.microsoft.com/en-us/connectors/custom-connectors/#2-secure-your-api) (username = `clientId`, password = `clientSecret`). While we know it is not an optimal solution, it appears to be the only solution to do this against custom OAuth2 / OpenID Connect provider. Also the extra benefit is, that the acquired token can be cached for the specified amount of time, so you won't hit your STS with every single request.

Alternatively, you can provide both `clientId` and `clientSecret` as query parameters = `$clientId` and `$clientSecret`. This has been added due to the need to access multiple workspaces in a single Flow dynamically.

There is [an open issue](https://github.com/microsoft/PowerPlatformConnectors/issues/708) in Microsoft's custom connectors repository to provide the documentation on how-to achieve this natively with connectors (yes, it is possible, at least with Azure AD).

## Getting started
1. Deploy to Azure as an [Azure Function app](https://azure.microsoft.com/en-us/services/functions/)
2. [Configure settings](https://docs.microsoft.com/en-us/azure/azure-functions/functions-how-to-use-azure-function-app-settings?tabs=portal) (example below)
```js
"BaseUrl": "https://petstore.swagger.io",
"TokenEndpoint": "https://petstore.swagger.io/oauth/token",
"Scope": "api.read api.write"
```
Optionally you can configure token cache TTL (default value is 30 minutes)
```js
"TokenCacheTime": "0:15:00" // set to 15 minutes
```
3. Change the base url in your custom connector definition to `https://{your-function-app-name}.azurewebsites.net/Proxy`

## Example usage
Every Custom connector action should now work as expected. For example the action `GET /pet/{petId}` from the original connector will now be:
1. Logic Apps/Power Automate sends request to `https://{your-function-app-name}.azurewebsites.net/Proxy/pet/{petId}`
2. Basic authentication will be used as client credentials to acquire the access token
    * You can then configure Basic authentication as a method for your Flow/Logic Apps custom connector.
3. The token is cached for future use with identical client credentials
4. The token is attached to the incoming request and sent along to `/pet/{petId}`
5. The response is then forwarded back to your Logic Apps/Power Automate
