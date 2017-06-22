using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace HSPI_NESTSIID.Models
{
    [DataContract]
    class Login : IDisposable
    {
        [DataMember(Name = "access_token")]
        public string access_token { get; set; }


        public Login(string access_token)
        {
            this.access_token = access_token;

        }

        // Disposable Interface
        public bool Disposed { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Login()
        {
            Debug.Assert(Disposed, "WARNING: Object finalized without being disposed!");
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    DisposeManagedResources();
                }

                DisposeUnmanagedResources();
                Disposed = true;
            }
        }

        protected virtual void DisposeManagedResources() { }
        protected virtual void DisposeUnmanagedResources() { }
    }
}
