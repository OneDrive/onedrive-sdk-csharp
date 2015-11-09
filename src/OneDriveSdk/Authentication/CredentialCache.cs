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

namespace Microsoft.OneDrive.Sdk
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Notification delegate for cache access.
    /// </summary>
    /// <param name="args">The argument set for the notification.</param>
    public delegate void CredentialCacheNotification(CredentialCacheNotificationArgs args);

    public class CredentialCache
    {
        internal readonly IDictionary<CredentialCacheKey, AccountSession> cacheDictionary =
            new ConcurrentDictionary<CredentialCacheKey, AccountSession>();

        private const int CacheVersion = 1;

        /// <summary>
        /// Instantiates a new <see cref="CredentialCache"/>.
        /// </summary>
        /// <param name="serializer">The <see cref="ISerializer"/> for serializing cache contents.</param>
        public CredentialCache(ISerializer serializer = null)
            : this(null, serializer)
        {
        }

        /// <summary>
        /// Instantiates a new <see cref="CredentialCache"/>.
        /// </summary>
        /// <param name="blob">The cache contents for initialization.</param>
        /// <param name="serializer">The <see cref="ISerializer"/> for serializing cache contents.</param>
        public CredentialCache(byte[] blob, ISerializer serializer = null)
        {
            this.Serializer = serializer ?? new Serializer();
            this.InitializeCacheFromBlob(blob);
        }

        /// <summary>
        /// Gets or sets the notification delegate for before accessing the cache.
        /// </summary>
        public virtual CredentialCacheNotification BeforeAccess { get; set; }

        /// <summary>
        /// Gets or sets the notification delegate for before writing to the cache.
        /// </summary>
        public virtual CredentialCacheNotification BeforeWrite { get; set; }

        /// <summary>
        /// Gets or sets the notification delegate for after accessing the cache.
        /// </summary>
        public virtual CredentialCacheNotification AfterAccess { get; set; }

        /// <summary>
        /// Gets or sets whether or not the cache state has changed.
        /// </summary>
        public virtual bool HasStateChanged { get; set; }

        protected ISerializer Serializer { get; private set; }

        /// <summary>
        /// Gets the contents of the cache.
        /// </summary>
        /// <returns>The cache contents.</returns>
        public virtual byte[] GetCacheBlob()
        {
            using (var stream = new MemoryStream())
            using (var binaryReader = new BinaryReader(stream))
            using (var binaryWriter = new BinaryWriter(stream))
            {
                binaryWriter.Write(CredentialCache.CacheVersion);
                binaryWriter.Write(this.cacheDictionary.Count);
                foreach (var cacheItem in this.cacheDictionary)
                {
                    binaryWriter.Write(this.Serializer.SerializeObject(cacheItem.Key));
                    binaryWriter.Write(this.Serializer.SerializeObject(cacheItem.Value));
                }

                var length = (int)stream.Position;
                stream.Position = 0;

                return binaryReader.ReadBytes(length);
            }
        }

        /// <summary>
        /// Initializes the cache from the specified contents.
        /// </summary>
        /// <param name="cacheBytes">The cache contents.</param>
        public virtual void InitializeCacheFromBlob(byte[] cacheBytes)
        {
            if (cacheBytes == null)
            {
                this.cacheDictionary.Clear();
            }
            else
            {
                using (var stream = new MemoryStream())
                using (var binaryReader = new BinaryReader(stream))
                using (var binaryWriter = new BinaryWriter(stream))
                {
                    binaryWriter.Write(cacheBytes);
                    stream.Position = 0;

                    this.cacheDictionary.Clear();

                    var version = binaryReader.ReadInt32();

                    if (version != CredentialCache.CacheVersion)
                    {
                        // If the cache version doesn't match, skip deserialization
                        return;
                    }

                    var count = binaryReader.ReadInt32();

                    for (int i=0; i < count; i++)
                    {
                        var keyString = binaryReader.ReadString();
                        var authResultString = binaryReader.ReadString();

                        if (!string.IsNullOrEmpty(keyString) && !string.IsNullOrEmpty(authResultString))
                        {
                            var credentialCacheKey = this.Serializer.DeserializeObject<CredentialCacheKey>(keyString);
                            var authResult = this.Serializer.DeserializeObject<AccountSession>(authResultString);

                            this.cacheDictionary.Add(credentialCacheKey, authResult);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Clears the cache contents.
        /// </summary>
        public virtual void Clear()
        {
            var cacheNotificationArgs = new CredentialCacheNotificationArgs { CredentialCache = this };

            this.OnBeforeAccess(cacheNotificationArgs);
            this.OnBeforeWrite(cacheNotificationArgs);

            this.cacheDictionary.Clear();

            this.HasStateChanged = true;
            this.OnAfterAccess(cacheNotificationArgs);
        }

        internal virtual void AddToCache(AccountSession accountSession)
        {
            var cacheNotificationArgs = new CredentialCacheNotificationArgs { CredentialCache = this };

            this.OnBeforeAccess(cacheNotificationArgs);
            this.OnBeforeWrite(cacheNotificationArgs);

            var cacheKey = this.GetKeyForAuthResult(accountSession);
            this.cacheDictionary[cacheKey] = accountSession;

            this.HasStateChanged = true;
            this.OnAfterAccess(cacheNotificationArgs);
        }

        internal virtual void DeleteFromCache(AccountSession accountSession)
        {
            if (accountSession != null)
            {
                var cacheNotificationArgs = new CredentialCacheNotificationArgs { CredentialCache = this };
                this.OnBeforeAccess(cacheNotificationArgs);
                this.OnBeforeWrite(cacheNotificationArgs);

                var credentialCacheKey = this.GetKeyForAuthResult(accountSession);
                this.cacheDictionary.Remove(credentialCacheKey);

                this.HasStateChanged = true;

                this.OnAfterAccess(cacheNotificationArgs);
            }
        }

        internal virtual AccountSession GetResultFromCache(AccountType accountType, string clientId, string userId)
        {
            var cacheNotificationArgs = new CredentialCacheNotificationArgs { CredentialCache = this };
            this.OnBeforeAccess(cacheNotificationArgs);

            var credentialCacheKey = new CredentialCacheKey
            {
                AccountType = accountType,
                ClientId = clientId,
                UserId = userId,
            };

            AccountSession cacheResult = null;
            this.cacheDictionary.TryGetValue(credentialCacheKey, out cacheResult);

            this.OnAfterAccess(cacheNotificationArgs);

            return cacheResult;
        }

        private CredentialCacheKey GetKeyForAuthResult(AccountSession accountSession)
        {
            return new CredentialCacheKey
            {
                AccountType = accountSession.AccountType,
                ClientId = accountSession.ClientId,
                UserId = accountSession.UserId,
            };
        }

        protected void OnAfterAccess(CredentialCacheNotificationArgs args)
        {
            if (this.AfterAccess != null)
            {
                this.AfterAccess(args);
            }
        }

        protected void OnBeforeAccess(CredentialCacheNotificationArgs args)
        {
            if (this.BeforeAccess != null)
            {
                this.BeforeAccess(args);
            }
        }

        protected void OnBeforeWrite(CredentialCacheNotificationArgs args)
        {
            if (this.BeforeWrite != null)
            {
                this.BeforeWrite(args);
            }
        }
    }
}
