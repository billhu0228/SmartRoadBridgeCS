using System;
using System.Collections;
using System.Collections.Generic;

namespace SmartRoadBridge.Alignment
{

    public class AlignCollection : ICollection<Align>
    {
        private List<Align> innerCol;

        public AlignCollection()
        {
            innerCol = new List<Align>();
        }


        public Align this[int index]
        {
            get { return (Align)innerCol[index]; }
            set { innerCol[index] = value; }
        }

        public int Count => innerCol.Count;

        public bool IsReadOnly => false;

        public void Add(Align item)
        {
            if (!Contains(item))
            {
                innerCol.Add(item);
            }
        }

        public void Clear()
        {
            innerCol.Clear();
        }

        public bool Contains(Align item)
        {
            bool found = false;

            foreach (Align bx in innerCol)
            {
                if (bx.Name == item.Name)
                {
                    found = true;
                }
            }

            return found;
        }

        public void CopyTo(Align[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("The array cannot be null.");
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("The starting array index cannot be negative.");
            if (Count > array.Length - arrayIndex + 1)
                throw new ArgumentException("The destination array has fewer elements than the collection.");

            for (int i = 0; i < innerCol.Count; i++)
            {
                array[i + arrayIndex] = innerCol[i];
            }
        }

        public IEnumerator<Align> GetEnumerator()
        {
            return new AlignEnumerator(this);
        }

        public bool Remove(Align item)
        {
            bool result = false;

            // Iterate the inner collection to 
            // find the box to be removed.
            for (int i = 0; i < innerCol.Count; i++)
            {

                Align curAlign = (Align)innerCol[i];

                if (item.Name == curAlign.Name)
                {
                    innerCol.RemoveAt(i);
                    result = true;
                    break;
                }
            }
            return result;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new AlignEnumerator(this);
        }
    }
}
