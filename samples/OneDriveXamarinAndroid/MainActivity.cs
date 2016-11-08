using Android.App;
using Android.Widget;
using Android.OS;
using Microsoft.OneDrive.Sdk;
using System;

namespace OneDriveXamarinAndroid
{
    [Activity(Label = "OneDriveXamarinAndroid", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private readonly string appId = "[INSERT APPID]";
        private readonly string clientSecret = "[INSERT CLIENT SECRET]";
        private readonly string[] scopes = new string[] { "onedrive.appfolder", "wl.signin", "wl.offline_access" };
        private readonly string returnUrl = "https://login.live.com/oauth20_desktop.srf";

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            base.SetContentView(Resource.Layout.Main);
            Button loginButton = FindViewById<Button>(Resource.Id.login_button);
            loginButton.Click += LoginButton_Click;
        }

        private async void LoginButton_Click(object sender, System.EventArgs e)
        {
            try
            {
                IOneDriveClient client = XamarinClientExtensions.GetClient(
                    this,
                    appId,
                    returnUrl,
                    scopes,
                    clientSecret);

                await client.AuthenticateAsync();

                System.Diagnostics.Debug.WriteLine("IsAuthenticated: {0}", client.IsAuthenticated);

                Toast.MakeText(this, "Login Completed", ToastLength.Long).Show();
            }
            catch (Exception ex)
            {
                OneDriveException oneDriveError = ex as OneDriveException;
                if (oneDriveError == null)
                    Toast.MakeText(this, "Unknown Error: " + ex.Message, ToastLength.Long).Show();
                else
                    Toast.MakeText(this, "OneDrive Error: " + oneDriveError.Error.Message, ToastLength.Long).Show();
            }
        }
    }
}