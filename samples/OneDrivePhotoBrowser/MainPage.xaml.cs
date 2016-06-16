// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace OneDrivePhotoBrowser
{
    using Controllers;
    using Microsoft.OneDrive.Sdk;
    using Models;
    using System.Linq;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Input;

    /// <summary>
    /// The main page for photo and folder browsing.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ItemsController itemsController;
        
        private readonly string[] scopes = new string[] { "onedrive.read", "wl.offline_access", "wl.signin" };

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.itemsController == null)
            {
                this.itemsController = new ItemsController(((App)Application.Current).OneDriveClient);
            }

            var last = ((App)Application.Current).NavigationStack.Last();
            ((App)Application.Current).Items = await this.itemsController.GetImagesAndFolders(last.Id);
            this.DataContext = ((App)Application.Current).Items;
            wait.IsActive = false;
        }

        private void StackPanel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // Get the selected item and add it to the navigation stack.
            ItemModel item = ((StackPanel)sender).DataContext as ItemModel;
            ((App)Application.Current).NavigationStack.Add(item);

            // If the item is a folder, navigate to its contents. Otherwise, load the detailed view of the item.
            if (item.Item.Folder != null)
            {
                Frame.Navigate(typeof(MainPage), ((App)Application.Current).Items.IndexOf((ItemModel)((StackPanel)sender).DataContext));
            }
            else
            {
                Frame.Navigate(typeof(ItemDetail), ((App)Application.Current).Items.IndexOf((ItemModel)((StackPanel)sender).DataContext));
            }
        }
    }
 }
