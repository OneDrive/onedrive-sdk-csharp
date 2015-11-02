# OneDrive Photo Browser sample

The OneDrive Photo Browser sample is a Windows Universal app that uses the OneDrive SDK for C#/.NET. 
The sample app displays only items that are images from a user's OneDrive. Note that this sample does not work with OneDrive for Business.

## Set up

### Prerequisites

To run the sample, you will need: 

* Visual Studio 2013 or 2015, with Universal Windows App Development Tools **Note:** If you don't have Universal Windows App Development Tools installed, open **Control Panel** | **Uninstall a program**. Then right-click **Microsoft Visual Studio** and click **Change**. Select **Modify** and then choose **Universal Windows App Development Tools**. Click **Update**. For more info about setting up your machine for Universal Windows Platform development, see [Build UWP apps with Visual Studio](https://msdn.microsoft.com/en-us/library/windows/apps/dn609832.aspx).
* A Microsoft account
* Knowledge of Windows Universal app development

### Download the sample

1. Download the sample from [GitHub](https://github.com/OneDrive/onedrive-sdk-csharp) by choosing **Clone in Desktop** or **Download Zip**. 
3. In Visual Studio, open the **OneDriveSdk.sln** file and build it.

### Associate the sample app with the Windows Store

Before you can run the sample, you must associate the app with the Windows Store. To do this, right-click the OneDrivePhotoBrowser project and choose **Store** | **Associate app with store**. Associating the app with the Windows store is reqiured for authentication to succeed.

## Run the sample

1. With the sample open in Visual Studio, at the top, select **Debug** for Solution Configurations and **x86** or **x64** for Solution Platforms, and **OneDrivePhotoBrowser** for Startup project. 
2. Check that you are running the sample on the **Local Machine**.
3. Press **F5** or click **Start** to run the sample.

The OneDrive Photo Browser sample app will open the signed-in user's personal OneDrive, with only folders and images displayed. If the file is not an image, it will not show up in the OneDrive Photo Browser app. Select a folder to see all images in that folder. Select an image to see a larger display of the image, with scroll view.

## API features

### OneDrive sign-in

In this sample app, authentication and sign in occurs when the app starts. If the user is not already signed in, the app will invoke the Microsoft account sign-in window.

In App.xaml.cs, an `IOneDriveClient` object is initialized.

```csharp
public IOneDriveClient OneDriveClient { get; set}
```
In MainPage.xaml.cs, sign-in is verified. If no user is signed in, a Microsoft account sign-in dialog appears. The `GetUniversalClient` is called on the `OneDriveClientExtensions` object to get a `OneDriveClient` object. Once a `OneDriveClient` object is returned, `AuthenticateAsync` completes the client authentication for the Windows Universal sample app.

```csharp
private readonly string[] scopes = new string[] { "onedrive.readwrite", "wl.offline_access", "wl.signin" };
...
private async void MainPage_Loaded(object sender, RoutedEventArgs e)
{
  if (((App)Application.Current).OneDriveClient == null)
  {
      ((App)Application.Current).OneDriveClient = OneDriveClientExtensions.GetUniversalClient(this.scopes);
      await ((App)Application.Current).OneDriveClient.AuthenticateAsync();
  }
  ...
}
```

### Get thumbnails for an image in OneDrive

In this example, thumbnails are returned for an item, if it is an image. `GetAsync()` is used to get the item's properties.

```csharp
var childrenPage = await this.oneDriveClient.Drive.Items[id].Children.Request().Expand("thumbnails").GetAsync();
items = childrenPage == null
  ? new List<Item>()
  : childrenPage.CurrentPage.Where(item => item.Folder != null || item.Image != null);
```

## More resources

* [OneDrive SDK for CSharp](https://github.com/OneDrive/onedrive-sdk-csharp) documentation
* [OneDrive API](https://dev.onedrive.com/) - Official documentation for the OneDrive API
* [OneDriveApiBrowser](https://github.com/OneDrive/onedrive-sdk-csharp/tree/master/samples/OneDriveApiBrowser) - A Windows Forms sample app using OneDrive SDK for CSharp 
* [Windows Universal apps](https://msdn.microsoft.com/en-us/library/windows/apps/dn726767.aspx) - More information about Windows Universal apps

## Copyright

Copyright (c) Microsoft. All rights reserved.
