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

namespace Test.OneDriveSdk.WinRT.Mocks
{
    using System.IO;

    using Microsoft.OneDrive.Sdk;

    public delegate void DeserializeObjectStreamCallback(Stream stream);
    public delegate void DeserializeObjectStringCallback(string inputString);
    public delegate string SerializeObjectCallback(object serializableObject);

    public class MockSerializer : ISerializer
    {
        public object DeserializeObjectResponse { get; set; }

        public string SerializeObjectResponse { get; set; }

        public DeserializeObjectStreamCallback OnDeserializeObjectStream { get; set; }

        public DeserializeObjectStringCallback OnDeserializeObjectString { get; set; }

        public SerializeObjectCallback OnSerializeObject { get; set; }

        public T DeserializeObject<T>(string inputString)
        {
            if (this.OnDeserializeObjectString != null)
            {
                this.OnDeserializeObjectString(inputString);
            }

            return (T)this.DeserializeObjectResponse;
        }

        public T DeserializeObject<T>(Stream stream)
        {
            if (this.OnDeserializeObjectStream != null)
            {
                this.OnDeserializeObjectStream(stream);
            }

            return (T)this.DeserializeObjectResponse;
        }

        public string SerializeObject(object serializeableObject)
        {
            if (this.OnSerializeObject != null)
            {
                this.OnSerializeObject(serializeableObject);
            }

            return this.SerializeObjectResponse;
        }
    }
}
