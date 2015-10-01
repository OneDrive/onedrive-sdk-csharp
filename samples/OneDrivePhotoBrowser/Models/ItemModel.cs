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

namespace OneDrivePhotoBrowser.Models
{
    using Microsoft.OneDrive.Sdk;
    using System.ComponentModel;
    using System.Linq;
    using Windows.UI.Xaml.Media.Imaging;

    public class ItemModel : INotifyPropertyChanged
    {
        private BitmapSource bitmap;

        public ItemModel(Item item)
        {
            this.Item = item;
        }

        public BitmapSource Bitmap
        {
            get
            {
                return this.bitmap;
            }
            set
            {
                this.bitmap = value;
                OnPropertyChanged("Bitmap");
            }
        }

        public string Icon
        {
            get
            {
                if (this.Item.Folder != null)
                {
                    return "ms-appx:///assets/app/folder.png";
                }
                else if (this.SmallThumbnail != null)
                {
                    return this.SmallThumbnail.Url;
                }

                return null;
            }
        }

        public string Id
        {
            get
            {
                return this.Item == null ? null : this.Item.Id;
            }
        }

        public Item Item { get; private set; }

        public string Name
        {
            get
            {
                return this.Item.Name;
            }
        }

        public Thumbnail SmallThumbnail
        {
            get
            {
                if (this.Item != null && this.Item.Thumbnails != null)
                {
                    var thumbnailSet = this.Item.Thumbnails.FirstOrDefault();
                    if (thumbnailSet != null)
                    {
                        return thumbnailSet.Small;
                    }
                }

                return null;
            }
        }

        //INotifyPropertyChanged members
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (null != PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
