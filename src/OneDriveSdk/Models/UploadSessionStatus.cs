// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.OneDrive.Sdk.Models
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization;
    using Microsoft.Graph;
    using Newtonsoft.Json;

    /// <summary>
    /// The type UploadSessionStatus.
    /// </summary>
    [DataContract]
    [JsonConverter(typeof(DerivedTypeConverter))]
    public partial class UploadSessionStatus
    {
        /// <summary>
        /// Gets or sets expiration.
        /// </summary>
        [DataMember(Name = "expriationDateTime", EmitDefaultValue = false, IsRequired = false)]
        public DateTime ExpirationDateTime { get; set; }

        /// <summary>
        /// Gets or sets the next expected ranges.
        /// </summary>
        [DataMember(Name = "nextExpectedRanges", EmitDefaultValue = false, IsRequired = false)]
        public IEnumerable<string> NextExpectedRanges { get; set; }
    }
}
