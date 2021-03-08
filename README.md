# Token Proxy
Token Proxy allows the use of client_credentials grant in Logic Apps/Power Automate connectors via Basic Authentication headers and acting as a proxy to the target API.

## Getting started
1. Deploy to Azure
2. Configure settings (example below)
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
3. The token is cached for future use with identical client credentials
4. The token is attached to the incoming request and sent along to `/pet/{petId}`
5. The response is then forwarded back to your Logic Apps/Power Automate
