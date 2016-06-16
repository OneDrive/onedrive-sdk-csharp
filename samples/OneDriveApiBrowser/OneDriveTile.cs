// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace OneDriveApiBrowser
{
    using System;
    using System.Drawing;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    using Microsoft.Graph;
    using Microsoft.OneDrive.Sdk;

    public partial class OneDriveTile : UserControl
    {
        private Item _sourceItem;
        private bool _selected;
        private IOneDriveClient oneDriveClient;

        public OneDriveTile(IOneDriveClient oneDriveClient)
        {
            this.oneDriveClient = oneDriveClient;
            InitializeComponent();
        }

        public Item SourceItem 
        {
            get { return _sourceItem; }
            set
            {
                if (value == _sourceItem)
                    return;

                _sourceItem = value;
                SourceItemChanged();
            }
        }

        private void SourceItemChanged()
        {
            if (null == _sourceItem) return;
            labelName.Text = _sourceItem.Name;

            LoadThumbnail();
        }

        private async void LoadThumbnail()
        {
            var thumbnail = await this.ThumbnailUrlAsync("medium");
            if (null != thumbnail)
            {
                string thumbnailUri = thumbnail.Url;
                pictureBoxThumbnail.ImageLocation = thumbnailUri;
            }
        }

        /// <summary>
        /// Retrieve a specific size thumbnail's metadata. If it isn't already 
        /// available make a call to the service to retrieve it.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public async Task<Thumbnail> ThumbnailUrlAsync(string size = "large")
        {
            bool loadedThumbnails = this._sourceItem != null && this._sourceItem.Thumbnails != null &&
                                    this._sourceItem.Thumbnails.CurrentPage != null;
            if (loadedThumbnails)
            {
                // See if we already have that thumbnail
                Thumbnail thumbnail = null;
                ThumbnailSet thumbnailSet = null;

                switch (size.ToLower())
                {
                    case "small":
                        thumbnailSet = this._sourceItem.Thumbnails.CurrentPage.FirstOrDefault(set => set.Small != null);
                        thumbnail = thumbnailSet == null ? null : thumbnailSet.Small;
                        break;
                    case "medium":
                        thumbnailSet = this._sourceItem.Thumbnails.CurrentPage.FirstOrDefault(set => set.Medium != null);
                        thumbnail = thumbnailSet == null ? null : thumbnailSet.Medium;
                        break;
                    case "large":
                        thumbnailSet = this._sourceItem.Thumbnails.CurrentPage.FirstOrDefault(set => set.Large != null);
                        thumbnail = thumbnailSet == null ? null : thumbnailSet.Large;
                        break;
                    default:
                        thumbnailSet = this._sourceItem.Thumbnails.CurrentPage.FirstOrDefault(set => set[size] != null);
                        thumbnail = thumbnailSet == null ? null : thumbnailSet[size];
                        break;
                }

                if (thumbnail != null)
                {
                    return thumbnail;
                }

            }

            if (!loadedThumbnails)
            {
                try
                {
                    // Try to load the thumbnail from the service if we haven't loaded thumbnails.
                    return await this.oneDriveClient.Drive.Items[this._sourceItem.Id].Thumbnails["0"][size].Request().GetAsync();
                }
                catch (ServiceException exception)
                {
                    if (exception.IsMatch(OneDriveErrorCode.ItemNotFound.ToString()))
                    {
                        // Just swallow not found. We don't want an error popup and we just won't render a thumbnail
                        return null;
                    }
                }
            }

            return null;
        }

        private void Control_Click(object sender, EventArgs e)
        {
            OnClick(EventArgs.Empty);
        }

        private void Control_DoubleClick(object sender, EventArgs e)
        {
            OnDoubleClick(EventArgs.Empty);
        }

        public bool Selected
        {
            get { return _selected; }
            set
            {
                if (value != _selected)
                {
                    _selected = value;
                    labelName.Font = _selected ? new Font(labelName.Font, FontStyle.Bold) : new Font(labelName.Font, FontStyle.Regular);
                }
            }
        }

        //protected override void OnPaint(PaintEventArgs e)
        //{
        //    base.OnPaint(e);

        //    if (this.Selected)
        //    {
        //        Color color = Color.White;
        //        int width = 2;
        //        ButtonBorderStyle style = ButtonBorderStyle.Solid;

        //        ControlPaint.DrawBorder(e.Graphics, ClientRectangle,
        //            color, width, style,
        //            color, width, style,
        //            color, width, style,
        //            color, width, style);
        //    }
        //}

    }
}
