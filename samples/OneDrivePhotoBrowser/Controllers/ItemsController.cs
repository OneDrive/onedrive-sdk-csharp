// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace OneDrivePhotoBrowser.Controllers
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.OneDrive.Sdk;

    using Models;

    public class ItemsController
    {
        private IOneDriveClient oneDriveClient;

        public ItemsController(IOneDriveClient oneDriveClient)
        {
            this.oneDriveClient = oneDriveClient;
        }

        /// <summary>
        /// Gets the child folders and photos of the specified item ID.
        /// </summary>
        /// <param name="id">The ID of the parent item.</param>
        /// <returns>The child folders and photos of the specified item ID.</returns>
        public async Task<ObservableCollection<ItemModel>> GetImagesAndFolders(string id)
        {
            ObservableCollection<ItemModel> results = new ObservableCollection<ItemModel>();
            
            IEnumerable<Item> items;

            var expandString = "thumbnails,children(expand=thumbnails)";

            // If id isn't set, get the OneDrive root's photos and folders. Otherwise, get those for the specified item ID.
            // Also retrieve the thumbnails for each item if using a consumer client.
            var itemRequest = string.IsNullOrEmpty(id)
                ? this.oneDriveClient.Drive.Root.Request().Expand(expandString)
                : this.oneDriveClient.Drive.Items[id].Request().Expand(expandString);

            var item = await itemRequest.GetAsync();
            items = item.Children == null
                ? new List<Item>()
                : item.Children.CurrentPage.Where(child => child.Folder != null || child.Image != null);

            foreach (var child in items)
            {
                results.Add(new ItemModel(child));
            }

            return results;
        }
    }
}
