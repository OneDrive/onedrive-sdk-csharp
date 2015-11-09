Authenticate your C# app for OneDrive
=====

To authenticate your app to use OneDrive, you need to get a `OneDriveClient`, which will handle all authentication for you, and call `AuthenticateAsync` on it. Note that if the user changes their password, you must re-authenticate.  If you see `401` error codes, this is most likely the case. See [Error codes for the OneDrive C# SDK](errors.md) for more info.

## Standard authentication components

### CredentialCache

The credential cache is an in-memory authentication cache. It supports retrieving a blob of the current cache state and initializing the cache from a blob so apps can do their own offline storage of cache data.

### IHttpProvider

The HTTP provider is responsible for sending an `HttpRequestMessage`. The default implementation uses an `HttpClient` for sending requests.

### IServiceInfoProvider

The service info provider is responsible for providing information for accessing the OneDrive service, such as an authentication provider and the base URL for the service.

### IWebAuthenticationUi

When using the default `IServiceInfoProvider` and `IAuthenticationProvider` implementations, an `IWebAuthenticationUi` implementation is required to display authentication UI to the user. Default implementations are available for WinForms, Windows 8.1, Windows Phone 8.1, and UWP applications. If no `IWebAuthenticationUi` implementation is present, only the silent authentication flow will be used.

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

In addition to client ID, return URL, scopes, and client secret the method takes in implementations for a credential cache, HTTP provider, and a service info provider or web authentication UI. If not provided, the default implementations of each item will be used.

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

The OneDriveClient extensions available vary based on the build flavor of the project. For Windows 8.1, Windows Phone 8.1, and UWP projects, there are 3 available methods depending on the Windows authentication API used to retrieve a client:

* `GetClientUsingOnlineIdAuthenticator`
* `GetClientUsingWebAuthenticationBroker`
* `GetUniversalClient`

`GetUniversalClient` calls `GetClientUsingOnlineIdAuthenticator` internally, using the [OnlineIdAuthenticator](https://msdn.microsoft.com/en-us/library/windows/apps/windows.security.authentication.onlineid.onlineidauthenticator.aspx) for authentication. `GetClientUsingWebAuthenticationBroker` uses the [WebAuthenticationBroker](https://msdn.microsoft.com/en-us/library/windows/apps/windows.security.authentication.web.webauthenticationbroker.aspx) in SSO mode for authentication.

Authentication with both the OnlineIdAuthenticator and WebAuthenticationBroker requires your app be associated with the Windows Store first.

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

The SDK uses [ADAL](https://github.com/AzureAD/azure-activedirectory-library-for-dotnet) for authentication against AAD. Implementations are available for WinForms, Windows 8.1, and UWP apps. Due to technical limitations, Windows Phone 8.1 is **not** supported.

## Caching credentials

Since ADAL has its own caching model, AAD authentication has its own CredentialCache implementation for caching, the AdalCredentialCache, that wraps the ADAL caching functionality. The mechanisms for interacting with the cache are the same as with Credential Cache but it can only be used for AAD credential caching.

## Authentication for WinForms

```csharp
var oneDriveClient = BusinessClientExtensions.GetActiveDirectoryClient(clientId, returnUrl);
                         
await oneDriveClient.AuthenticateAsync();
```

## Windows 8.1 and UWP

```csharp
var oneDriveClient = BusinessClientExtensions.GetActiveDirectoryClient(
                        oneDriveForBusinessAppId,
                        oneDriveForBusinessReturnUrl)

await oneDriveClient.AuthenticateAsync();
```

## Signing out

To sign out you can call:

```csharp
await oneDriveClient.SignOutAsync();
```