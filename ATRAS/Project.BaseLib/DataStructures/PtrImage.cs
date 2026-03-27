using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Project.BaseLib.DataStructures
{
    [DataContract]
    [SerializableAttribute()]
    public class PtrImage : ImageBase
    {
        private IntPtr imagePtr;

        public IntPtr Ptr
        {
            get { return imagePtr; }
            set { imagePtr = value; }
        }

        private GCHandle handle;

        public PtrImage()
        {
        }

        public PtrImage(IntPtr imagePtr, int width, int height, int pitch)
            : base(width, height, pitch)
        {
            this.imagePtr = imagePtr;
        }

        public PtrImage(IntPtr imagePtr, int width, int height)
            : base(width, height)
        {
            this.imagePtr = imagePtr;
        }

        public PtrImage(IntPtr imagePtr, ImageDimension dimension)
            : base(dimension)
        {
            this.imagePtr = imagePtr;
        }

        public PtrImage(ICollection arr, int width, int height, int pitch)
            : base(width, height, pitch)
        {
            Alloc(arr);
        }

        public PtrImage(ICollection arr, ImageDimension dimension)
            : base(dimension)
        {
            Alloc(arr);
        }

        private void Alloc(ICollection arr)
        {
            if (arr == null)
                throw new ArgumentNullException();

            if (arr.Count != this.Length)
                throw new Exception(String.Format("Length {1} and Count {0} are not equal", arr.Count, this.Length));

            handle = GCHandle.Alloc(arr, GCHandleType.Pinned);
            imagePtr = handle.AddrOfPinnedObject();
        }

        public unsafe void* ToPointer()
        {
            return imagePtr.ToPointer();
        }

        ~PtrImage()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (handle.IsAllocated)
            {
                handle.Free();
            }
        }

        #region IDisposable Members
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion


        public override long[] CalculateGrayscaleHistogram(out double average, out int maxGL, out int histogramPeak)
        {
            average = 0;
            maxGL = 0;
            histogramPeak = 0;
            long[] histogram = new long[256];

            unsafe
            {
                byte* ptr = (byte*)Ptr;
                for (int i = 0; i < Length; i++)
                {
                    var value = ptr[i];
                    histogram[value]++;

                    average += value;
                }
            }

            average /= Length;

            long max = 0;
            for (int i = 0; i < histogram.Length; i++)
            {
                if (histogram[i] > 0)
                    maxGL = i;

                if (histogram[i] > max)
                {
                    max = histogram[i];
                    histogramPeak = i;
                }
            }

            return histogram;
        }
    }
}
