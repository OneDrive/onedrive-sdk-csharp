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
    using System.Collections.Generic;

    public class CollectionPage<T> : IList<T>
    {
        public CollectionPage()
        {
            this.CurrentPage = new List<T>();
        }

        public CollectionPage(IList<T> currentPage)
        {
            this.CurrentPage = currentPage;
        }

        public IList<T> CurrentPage { get; private set; }

        public int IndexOf(T item)
        {
            return this.CurrentPage.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            this.CurrentPage.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            this.CurrentPage.RemoveAt(index);
        }

        public T this[int index]
        {
            get { return this.CurrentPage[index]; }
            set { this.CurrentPage[index] = value; }
        }

        public void Add(T item)
        {
            this.CurrentPage.Add(item);
        }

        public void Clear()
        {
            this.CurrentPage.Clear();
        }

        public bool Contains(T item)
        {
            return this.CurrentPage.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            this.CurrentPage.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return this.CurrentPage.Count; }
        }

        public bool IsReadOnly
        {
            get { return this.CurrentPage.IsReadOnly; }
        }

        public bool Remove(T item)
        {
            return this.CurrentPage.Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.CurrentPage.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.CurrentPage.GetEnumerator();
        }

        public IDictionary<string, object> AdditionalData { get; set; }
    }
}
