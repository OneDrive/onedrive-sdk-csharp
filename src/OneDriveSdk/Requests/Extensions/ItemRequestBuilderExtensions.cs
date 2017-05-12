// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.OneDrive.Sdk
{
    /// <summary>
    /// The type  ItemRequestBuilder.
    /// </summary>
    public partial class ItemRequestBuilder
    {
        /// <summary>
        /// Gets children request.
        /// <returns>The children request.</returns>
        /// </summary>
        public IItemRequestBuilder ItemWithPath(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                if (!path.StartsWith("/"))
                {
                    path = string.Format("/{0}", path);
                }
            }

            return new ItemRequestBuilder(
                string.Format("{0}:{1}:", this.RequestUrl, path),
                this.Client);
        }


        /// <summary>
        /// Gets the request builder for ItemDelta.
        /// </summary>
        /// <returns>The <see cref="IItemDeltaRequestBuilder"/>.</returns>
        public IItemDeltaRequestBuilder Delta(
            string token = null,
            int? top = null)
        {
            return new ItemDeltaRequestBuilder(
                this.AppendSegmentToRequestUrl("oneDrive.delta"),
                this.Client,
                token,
                top);
        }
    }
}