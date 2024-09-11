# Umbraco OpenID Connect example package

This example shows how OpenID Connect can be used for members in Umbraco. It's a complete Umbraco solution with a SQLite database. Everything is already configured correct so you can just download the project and run it. 

It's based on the external login providers documentation:<br />
https://docs.umbraco.com/umbraco-cms/reference/security/external-login-providers

There are two versions of this example. The **v13 version** can be found in [`Umbraco-OpenIdConnect-Example.Web`](./Umbraco-OpenIdConnect-Example.Web), which references [`Umbraco-OpenIdConnect-Example.Core`](./Umbraco-OpenIdConnect-Example.Core). The **v14+ version** has been completely rebuilt and can be found in [`Umbraco-OpenIdConnect-Example-v14+`](./Umbraco-OpenIdConnect-Example-v14+), offering full compatibility with Umbraco v14 and above.


**Backoffice credentials:**

|          	|                                      	|
|----------	|--------------------------------------	|
| Username 	| umbraco-openidconnect@mailinator.com 	|
| Password 	| AKXT9fJGqBvKCVK5TqNZ                 	|

**External member credentials:**

|          	|                                      	|
|----------	|--------------------------------------	|
| Username 	| openidconnect-example@mailinator.com 	|
| Password 	| juSp#&uf4a+omLkigIto                 	|

> [!TIP]
> I also created a [fork of the Umbraco Delivery API - member auth demo](https://github.com/jbreuer/UmbracoDeliveryApiAuthDemo). It adds support for external login providers. See this [pull request](https://github.com/jbreuer/UmbracoDeliveryApiAuthDemo/pull/1) for all the changes. You can also find more info in [this blog](https://www.jeroenbreuer.nl/blog/umbraco-headless-member-auth-with-external-login-providers/).

## Getting started
You can watch a getting started video here: <a href="https://youtu.be/cklH7DtRDIQ" target="_blank">https://youtu.be/cklH7DtRDIQ</a>

<a href="https://youtu.be/cklH7DtRDIQ" target="_blank"><img src="./Getting-started.png" height="250"></a>

## Online presentation
You can watch my online presentation here: <a href="https://youtu.be/I4ysh-czrYk" target="_blank">https://youtu.be/I4ysh-czrYk</a>

<a href="https://youtu.be/I4ysh-czrYk" target="_blank"><img src="./Umbraco-Community-Day-External-login-providers.jpg" height="250"></a>

## Important files

All important files that are used for this setup are in the ```Umbraco-OpenIdConnect-Example.Core``` project.

1. <a href="./Umbraco-OpenIdConnect-Example.Core/Provider/OpenIdConnectMemberExternalLoginProviderOptions.cs" target="_blank">OpenIdConnectMemberExternalLoginProviderOptions.cs</a><br />
This file is used to setup the auto link options.
2. <a href="./Umbraco-OpenIdConnect-Example.Core/Extensions/UmbracoBuilderExtensions.cs" target="_blank">UmbracoBuilderExtensions.cs</a><br />
Extensions used to setup OpenID Connect and the related events.
3. <a href="./Umbraco-OpenIdConnect-Example.Core/Controllers/ExternalLogoutController.cs" target="_blank">ExternalLogoutController.cs</a><br />
A new controller used for logout on the external login provider.

## Short demo
![Umbraco OpenID Connect demo](./Umbraco-OpenID-Connect-demo.gif)

## Auth0
There is a free Auth0 account that this project connects with. The Client ID and Client Secret are already <a href="./Umbraco-OpenIdConnect-Example.Web/appsettings.json#L30" target="_blank">configured</a> for that. Normally the Client Secret should not be in Github, but these settings are only used in this example so it's ok they are public. This is the project:

<img src="./auth0.png">