using System;
using System.IO;
using System.Runtime.InteropServices;

namespace TripDownMemoryLane.Demo01
{
    public class SampleDisposable
        : IDisposable
    {
        private FileStream _fileStream;
        private IntPtr _handle = Marshal.AllocHGlobal(4);

        public SampleDisposable(FileStream fileStream)
        {
            _fileStream = fileStream;
        }

        /// <summary>
        /// Dispose managed resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// When finalizing (GC), destroy unmanaged resources
        /// </summary>
        ~SampleDisposable()
        {
            Dispose(false);
        }

        /// <summary>
        /// Actual dispose
        /// </summary>
        /// <param name="destroyManaged">Should dispose managed objects?</param>
        private void Dispose(bool destroyManaged)
        {
            if (destroyManaged)
            {
                _fileStream.Dispose();
            }

            Marshal.FreeHGlobal(_handle);
        }
    }
}