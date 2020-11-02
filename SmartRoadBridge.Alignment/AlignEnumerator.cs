using System;
using System.Collections;
using System.Collections.Generic;

namespace SmartRoadBridge.Alignment
{
    class AlignEnumerator : IEnumerator<Align>
    {
        private AlignCollection _collection;
        private int curIndex;
        private Align curBox;

        public AlignEnumerator(AlignCollection collection)
        {
            _collection = collection;
            curIndex = -1;
            curBox = default(Align);
        }


        public Align Current => curBox;

        object IEnumerator.Current => Current;

        void IDisposable.Dispose() { }

        public bool MoveNext()
        {
            //Avoids going beyond the end of the collection.
            if (++curIndex >= _collection.Count)
            {
                return false;
            }
            else
            {
                // Set current box to next item in collection.
                curBox = _collection[curIndex];
            }
            return true;
        }

        public void Reset() { curIndex = -1; }

    }
}
