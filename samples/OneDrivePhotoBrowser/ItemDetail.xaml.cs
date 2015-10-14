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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace OneDrivePhotoBrowser
{
    using Models;
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.OneDrive.Sdk;
    using Windows.UI.Core;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Media.Imaging;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ItemDetail : Page
    {
        private bool initialized = false;

        public ItemDetail()
        {
            this.InitializeComponent();
            this.Loaded += ItemTile_Loaded;
        }

        private async void ItemTile_Loaded(object sender, RoutedEventArgs e)
        {
            var last = ((App)Application.Current).NavigationStack.Last();
            this.DataContext = ((App)Application.Current).Items;

            await Dispatcher.RunAsync(
                CoreDispatcherPriority.Low,
                () =>
                {
                    imgFlipView.SelectedIndex = ((App)Application.Current).Items.IndexOf(last);
                });

            await this.LoadImage(last);
            progressRing.IsActive = false;
            initialized = true;
        }

        private async void ImageFlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Load the picture for the newly-selected image.
            if (imgFlipView.SelectedIndex != -1 && initialized)
            {
                progressRing.IsActive = true;
                var item = ((App)Application.Current).Items[imgFlipView.SelectedIndex];
                await this.LoadImage(item);
                progressRing.IsActive = false;
            }
        }

        /// <summary>
        /// Loads the detail view for the specified item.
        /// </summary>
        /// <param name="item">The item to load.</param>
        /// <returns>The task to await.</returns>
        private async Task LoadImage(ItemModel item)
        {
            // Only load a detail view image for image items. Initialize the bitmap from the image conent stream.
            if (item.Bitmap == null && (item.Item.Image != null))
            {
                item.Bitmap = new BitmapImage();
                var client = ((App)Application.Current).OneDriveClient;

                using (var responseStream = await client.Drive.Items[item.Id].Content.Request().GetAsync())
                {
                    var memoryStream = responseStream as MemoryStream;

                    if (memoryStream != null)
                    {
                        await item.Bitmap.SetSourceAsync(memoryStream.AsRandomAccessStream());
                    }
                    else
                    {
                        using (memoryStream = new MemoryStream())
                        {
                            await responseStream.CopyToAsync(memoryStream);
                            memoryStream.Position = 0;

                            await item.Bitmap.SetSourceAsync(memoryStream.AsRandomAccessStream());
                        }
                    }
                }
            }
        }
    }
}
