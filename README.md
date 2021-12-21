# fiks-maskinporten-dotnet
[![MIT license](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/ks-no/fiks-io-client-dotnet/blob/master/LICENSE)
[![Nuget](https://img.shields.io/nuget/v/KS.fiks.maskinporten.client.svg)](https://www.nuget.org/packages/KS.Fiks.Maskinporten.Client)
[![GitHub issues](https://img.shields.io/github/issues-raw/ks-no/kryptering-dotnet.svg)](//github.com/ks-no/fiks-maskinporten-client-dotnet/issues)

.net core library for maskinporten authorization through ID-porten


## Installation
Install [KS.Fiks.Maskinporten.Client](https://www.nuget.org/packages/KS.Fiks.Maskinporten.Client) nuget package in your .net project.

## Example
### Setup configuration
#### Using factory for VER2 and PROD environments
```c#
// For VER2 (test)
var maskinportenConfigVer2 = MaskinportenClientConfigurationFactory.createVer2Configuration("ver2_issuer", testCertificate);
// For PROD
var maskinportenConfigProd = MaskinportenClientConfigurationFactory.createProdConfiguration("prod_issuer", certificate);
```
#### Complete configuration
```c#
var maskinportenConfig = new MaskinportenClientConfiguration(
    audience: @"https://ver2.maskinporten.no/", // ID-porten audience path
    tokenEndpoint: @"https://ver2.maskinporten.no/token", // ID-porten token path
    issuer: @"oidc_ks_test",  // Issuer name, heter nå Integrasjonens identifikator i selvbetjeningsløsningen til DigDir
    numberOfSecondsLeftBeforeExpire: 10, // The token will be refreshed 10 seconds before it expires
    certificate: /* virksomhetssertifikat as a X509Certificate2  */,
    consumerOrg: /* optional value. Sets header consumer_org */);
```
DigDir maintains a list of [well-know endpoints and configuration](https://docs.digdir.no/maskinporten_func_wellknown.html) for the available environments
### Create instance of MaskinportenClient
```c#
var maskinportenClient = new MaskinportenClient(maskinportenConfig);
```

### Get access token
```c#
var scope = "ks:fiks"; // Scope for access token
var accessToken = await maskinportenClient.GetAccessToken(scope);
```
### Get delegated access token 
```c#
var scope = "ks:fiks"; // Scope for access token
var consumerOrgNo = ...; // Official 9 digit organization number for an organization that has delegated access to you in ALTINN
var accessToken = await maskinportenClient.GetDelegatedAccessToken(consumerOrgNo, scope);
```
For more information on this feature, check the [delegation documentation](https://docs.digdir.no/maskinporten_func_delegering.html) at DigDir

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
