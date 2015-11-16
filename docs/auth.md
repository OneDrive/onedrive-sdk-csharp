Authenticate your C# app for OneDrive
=====

To authenticate your app to use OneDrive, you need to get a `OneDriveClient`, which will handle all authentication for you, and call `AuthenticateAsync` on it. Note that if the user changes their password, your app must re-authenticate.  If you see `401` error codes, this is most likely the case. See [Error codes for the OneDrive C# SDK](errors.md) for more info.

**Note** This topic assumes that you are familiar with app authentication. For more info about authentication in OneDrive, see [Authentication for the OneDrive API](https://dev.onedrive.com/auth/readme.htm).

## Standard authentication components

Various helper methods are available for constructing a client. All of them take a set of standard parameters:

| Paramater | Description |
|:----------|:------------|
| _clientId_ | The client ID of the app. Required. |
| _returnUrl_ | A redirect URL. Required. |
| _scopes_ | Permissions that your app requires from the user. Required. |
| _client\_secret_ | The client secret created for your app. Optional. Not available for Windows Store 8.1, Windows Phone 8.1, and Universal Windows Platform (UWP) apps. |

In addition to _clientId_, _returnURL_, _scopes_, and _client\_secret_ the method takes in implementations for a client type, credential cache, HTTP provider, and a service info provider or web authentication UI. If not provided, the default implementations of each item will be used.

### ClientType
A single client can only call OneDrive for Consumer or OneDrive for Business, not both. The service type is specified via passing ClientType `Personal` or `Business` to the client. The default client type is `Personal`.

If the application would like to interact with both OneDrive for Consumer and OneDrive for Business a client should be created for each.

### CredentialCache

The credential cache is an in-memory authentication cache. It supports retrieving a blob of the current cache state and initializing the cache from a blob so apps can do their own offline storage of cache data.

### IHttpProvider

The HTTP provider is responsible for sending an `HttpRequestMessage`. The default implementation uses an `HttpClient` for sending requests.

### IServiceInfoProvider

The service info provider is responsible for providing information for accessing the OneDrive service, such as an authentication provider and the base URL for the service.

### IWebAuthenticationUi

When you use the default `IServiceInfoProvider` and `IAuthenticationProvider` implementations, an `IWebAuthenticationUi` implementation is required to display authentication UI to the user. Default implementations are available for WinForms, Windows 8.1, Windows Phone 8.1, and UWP applications. If no `IWebAuthenticationUi` implementation is present, only the silent authentication flow will be used.

# Microsoft account (MSA) authentication
## Simple authentication
The easiest way to get an authenticated client is to use one of the `OneDriveClient` extensions and call `AuthenticateAsync` on the resulting client:

```csharp
var oneDriveClient = OneDriveClient.GetMicrosoftAccountClient(
                         clientId,
                         returnUrl,
                         scopes);
                         
await oneDriveClient.AuthenticateAsync();
```

## Authentication for WinForms

```csharp
var oneDriveClient = OneDriveClient.GetMicrosoftAccountClient(
                         clientId,
                         returnUrl,
                         scopes,
                         webAuthenticationUi: new FormsWebAuthenticationUi());
                         
await oneDriveClient.AuthenticateAsync();
```

## Windows 8.1, Windows Phone 8.1, and UWP

The OneDriveClient extensions available are based on the build target of the project. For Windows 8.1, Windows Phone 8.1, and UWP projects, there are three available methods depending on which Windows authentication API is used to retrieve a client:

* `GetClientUsingOnlineIdAuthenticator`
* `GetClientUsingWebAuthenticationBroker`
* `GetUniversalClient`

`GetUniversalClient` calls `GetClientUsingOnlineIdAuthenticator` internally, using the [OnlineIdAuthenticator](https://msdn.microsoft.com/en-us/library/windows/apps/windows.security.authentication.onlineid.onlineidauthenticator.aspx) for authentication. `GetClientUsingWebAuthenticationBroker` uses the [WebAuthenticationBroker](https://msdn.microsoft.com/en-us/library/windows/apps/windows.security.authentication.web.webauthenticationbroker.aspx) in SSO (single sign-on) mode for authentication.

Authentication with both the `OnlineIdAuthenticator` and `WebAuthenticationBroker` requires that your app is associated with the Windows Store first.

### Authentication using OnlineIdAuthenticator

```csharp
var oneDriveClient = OneDriveClientExtensions.GetClientUsingOnlineIdAuthenticator(scopes);
                         
await oneDriveClient.AuthenticateAsync();
```

### Authentication using WebAuthenticationBroker

```csharp
var oneDriveClient = OneDriveClientExtensions.GetClientUsingWebAuthenticationBroker(
                         appId,
                         scopes);
                         
await oneDriveClient.AuthenticateAsync();
```

## Signing out

To sign out you can call:

```csharp
await oneDriveClient.SignOutAsync();
```

# Azure Active Directory (AAD) authentication

The SDK uses [ADAL](https://github.com/AzureAD/azure-activedirectory-library-for-dotnet) for authentication against AAD. Implementations are available for WinForms, Windows 8.1, and UWP apps. Due to technical limitations, **Windows Phone 8.1 is not supported**.

## Caching credentials

Since ADAL has its own caching model, AAD authentication has its own CredentialCache implementation for caching, the AdalCredentialCache, that wraps the ADAL caching functionality. The mechanisms for interacting with the cache are the same as with Credential Cache but it can only be used for AAD credential caching. If a CredentialCache is provided that is not an AdalCredentialCache operations will bypass writing to it.

## Authentication using the discovery service

In the case where the OneDrive for Business API endpoint and resource ID aren't known it is possible to authenticate using the [discovery service](https://msdn.microsoft.com/en-us/office/office365/howto/discover-service-endpoints).

```csharp
var oneDriveClient = BusinessClientExtensions.GetActiveDirectoryClient(clientId, returnUrl);
                         
await oneDriveClient.AuthenticateAsync();
```

## Authentication using the OneDrive for Business API endpoint and resource ID

If the OneDrive for Business API endpoint and resource ID are already known they can be provided to the client and authentication will not route through the discovery service.

```csharp
var oneDriveClient = BusinessClientExtensions.GetActiveDirectoryClient(
                        clientId,
                        returnUrl,
                        oneDriveApiEndpoint,
                        serviceResourceId)

await oneDriveClient.AuthenticateAsync();
```

## Signing out

To sign out you can call:

```csharp
await oneDriveClient.SignOutAsync();
```
