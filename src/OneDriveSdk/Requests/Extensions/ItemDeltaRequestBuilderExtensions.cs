// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.OneDrive.Sdk
{
    public partial class ItemDeltaRequestBuilder
    {
        /// <summary>
        /// Constructs a new <see cref="ItemDeltaRequestBuilder"/>.
        /// </summary>
        /// <param name="requestUrl">The URL for the request.</param>
        /// <param name="client">The <see cref="IBaseClient"/> for handling requests.</param>
        /// <param name="token">A token parameter for the OData method call.</param>
        /// <param name="top">Makes a request for the maximum number of items to be returned.</param>
        public ItemDeltaRequestBuilder(
            string requestUrl,
            IBaseClient client,
            string token,
            int? top)
            : this(requestUrl, client, token)
        {
            this.SetParameter("top", top, true);
        }
    }
}
