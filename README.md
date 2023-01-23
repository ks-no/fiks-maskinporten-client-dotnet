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
var maskinportenConfigVer2 = MaskinportenClientConfigurationFactory.CreateVer2Configuration("ver2_issuer", testCertificate);
// For PROD
var maskinportenConfigProd = MaskinportenClientConfigurationFactory.CreateProdConfiguration("prod_issuer", certificate);
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

### Get on behalf of access token
*This is a feature with limited usecase*
```c#
var scope = "ks:fiks"; // Scope for access token
var consumerOrgNo = ...; // Official 9 digit organization number for an organization that has delegated access to you in ALTINN
var accessToken = await maskinportenClient.GetOnBehalfOfAccessToken(consumerOrgNo, scope);
```
For more information on this feature, check the [onbehalfof documentation](https://docs.digdir.no/docs/idporten/oidc/oidc_api_admin_leverand%C3%B8r.html#1-onbehalfof-i-id-porten) at DigDir

Please note that as stated in the documentation at DigDir, *"Det gir ingen mening å bruke onbehalfof for Maskinporten-integrasjoner"*, means that for most cases it is not usable and is planned for removal. When it is removed this feature will be removed from this client too. 



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
