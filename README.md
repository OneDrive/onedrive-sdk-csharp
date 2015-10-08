# OneDrive SDK for CSharp

[![Build status](https://ci.appveyor.com/api/projects/status/fs9ddrmdev37v012/branch/master?svg=true)](https://ci.appveyor.com/project/OneDrive/onedrive-sdk-csharp/branch/master)

Integrate the [OneDrive API](https://dev.onedrive.com/README.htm) into your C#
project!

The OneDrive SDK is built as a Portable Class Library and targets the following
frameworks: .NET 4.5.1, .NET for Windows Store apps, Windows Phone 8.0 and higher.

## Installation

### Install via NuGet

To install the OneDrive SDK via NuGet

* Search for `Microsoft.OneDriveSDK` in the NuGet Library, or
* Type `Install-Package Microsoft.OneDriveSDK` into the Package Manager Console.

## Getting started

### 1. Register your application

Register your application by following [these](https://dev.onedrive.com/app-registration.htm) steps.

### 2. Setting your application Id and scopes

For more info about scopes, see [Authentication scopes](https://dev.onedrive.com/auth/msa_oauth.htm#authentication-scopes).

### 3. Getting an authenticated OneDriveClient object

For more info about authentication, see [auth](docs/auth.md).

### 4. Making requests to the service

Once you have an OneDriveClient that is authenticated you can begin to make calls against the service. The requests against the service look like our [REST API](https://dev.onedrive.com/README.htm).

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

## Documentation

For more detailed documentation see:

* [Overview](docs/overview.md)
* [Auth](docs/auth.md)
* [Items](docs/items.md)
* [Collections](docs/collections.md)
* [Errors](docs/errors.md)

## Known Issues

For known issues, see [issues](https://github.com/OneDrive/onedrive-sdk-csharp/issues).

## Other Resources

* NuGet Package: [https://www.nuget.org/packages/Microsoft.OneDriveSDK](https://www.nuget.org/packages/Microsoft.OneDriveSDK)


## License

[License](LICENSE.txt)
