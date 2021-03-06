# fiks-maskinporten-dotnet
[![MIT license](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/ks-no/fiks-io-client-dotnet/blob/master/LICENSE)
[![Nuget](https://img.shields.io/nuget/vpre/KS.fiks.maskinporten.client.svg)](https://www.nuget.org/packages/KS.Fiks.Maskinporten.Client)
[![GitHub issues](https://img.shields.io/github/issues-raw/ks-no/kryptering-dotnet.svg)](//github.com/ks-no/fiks-maskinporten-client-dotnet/issues)

.net core library for maskinporten authorization through ID-porten


## Installation
Install [KS.Fiks.Maskinporten.Client](https://www.nuget.org/packages/KS.Fiks.Maskinporten.Client) nuget package in your .net project.

## Example
### Setup configuration
```c#
var maskinportenConfig = new MaskinportenClientConfiguration(
    audience: @"https://oidc-ver2.difi.no/idporten-oidc-provider/", // ID-porten audience path
    tokenEndpoint: @"https://oidc-ver2.difi.no/idporten-oidc-provider/token", // ID-porten token path
    issuer: @"oidc_ks_test",  // Issuer name, heter nå Integrasjonens identifikator i selvbetjeningsløsningen til difi
    numberOfSecondsLeftBeforeExpire: 10, // The token will be refreshed 10 seconds before it expires
    certificate: /* virksomhetssertifikat as a X509Certificate2  */,
    consumerOrg: /* optional value. Sets header consumer_org */);
```
### Create instance of MaskinportenClient
```c#
var maskinportenClient = new MaskinportenClient(maskinportenConfig);
```

### Get access token
```c#
var scope = "ks:fiks" // Scope for access token
var accessToken = await maskinportenClient.GetAccessToken(scope);
```

### Send request using access token
```c#
var httpClient = new HttpClient();
using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, /* api uri */))
{
  // Set authorization header with maskinporten access token
  requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);
    
  /* Set other headers. Integration id and password etc.*/ 
  
  // Send message
  var response = await httpClient.SendAsync(requestMessage);

  /* Handle response */
}
```
