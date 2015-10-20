# OneDrive SDK for CSharp

[![Build status](https://ci.appveyor.com/api/projects/status/fs9ddrmdev37v012/branch/master?svg=true)](https://ci.appveyor.com/project/OneDrive/onedrive-sdk-csharp/branch/master)

Integrate the [OneDrive API](https://dev.onedrive.com/README.htm) into your C#
project!

The OneDrive SDK is built as a Portable Class Library and targets the following
frameworks: 

* .NET 4.5.1 
* .NET for Windows Store apps 
* Windows Phone 8.1 and higher

## Installation via Nuget

Before you install the OneDrive SDK, you must have Visual Studio installed, and an app to use the OneDrive SDK.
To install the OneDrive SDK via NuGet:

* Search for `Microsoft.OneDriveSDK` in the NuGet Library, or
* Type `Install-Package Microsoft.OneDriveSDK` into the **Package Manager Console** in Visual Studio. 

Once the package is installed in your project, reference the OneDrive SDK extensions into your project:

```csharp
using Microsoft.OneDrive.Sdk;
using Microsoft.OneDrive.Sdk.WinStore;
```

## Getting started

### 1. Register your application

Register your application for OneDrive by following [these](https://dev.onedrive.com/app-registration.htm) steps.

### 2. Setting your application Id and scopes

Your app must requests permissions in order to access a user's OneDrive. To do this, specify your app ID and scopes, or permission level.
For example:

```csharp
private string scopes = new string[] {"onedrive.readwrite', "wl.signin" };
```

For more information, see [Authentication scopes](https://dev.onedrive.com/auth/msa_oauth.htm#authentication-scopes).

### 3. Getting an authenticated OneDriveClient object

The **OneDriveClient** object will handle authentication for you. You must get a **OneDriveClient** object in order for your app to make requests to the service. 
For more information, see [Authenticate your C# app for OneDrive](docs/auth.md).

### 4. Making requests to the service

Once you have a OneDriveClient that is authenticated you can begin to make calls against the service. The requests against the service look like OneDrive's [REST API](https://dev.onedrive.com/README.htm).

To retrieve a user's drive:

```csharp
    var drive = await oneDriveClient
                          .Drive
                          .Request()
                          .GetAsync();
```

`GetAsync` will return a `Drive` object on success and throw a `OneDriveException` on error.

To get the current user's root folder of their drive:

```csharp
    var rootItem = await oneDriveClient
                             .Drive
                             .Root
                             .Request()
                             .GetAsync();
```

`GetAsync` will return an `Item` object on success and throw a `OneDriveException` on error.

For a general overview of how the SDK is designed, see [overview](docs/overview.md).

The following sample applications are also available:
* [OneDrive API Browser](samples/OneDriveApiBrowser) - Windows Forms app
* [OneDrive Photo Browser](samples/OneDrivePhotoBrowser) - Windows Universal app

To run the OneDrivePhotoBrowser sample app your machine will need to be configured for [UWP app development](https://msdn.microsoft.com/en-us/library/windows/apps/dn609832.aspx) and the project must be associated with the Windows Store.

## Documentation and resources

* [Overview](docs/overview.md)
* [Auth](docs/auth.md)
* [Items](docs/items.md)
* [Collections](docs/collections.md)
* [Errors](docs/errors.md)
* [OneDrive API](http://dev.onedrive.com)

## Issues

To view or log issues, see [issues](https://github.com/OneDrive/onedrive-sdk-csharp/issues).

## Other resources

* NuGet Package: [https://www.nuget.org/packages/Microsoft.OneDriveSDK](https://www.nuget.org/packages/Microsoft.OneDriveSDK)


## License

[License](LICENSE.txt)
