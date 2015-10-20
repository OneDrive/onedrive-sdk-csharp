// ------------------------------------------------------------------------------
//  Copyright (c) 2015 Microsoft Corporation
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
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
