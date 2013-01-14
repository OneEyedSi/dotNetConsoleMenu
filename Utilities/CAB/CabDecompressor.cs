//
// $History: CabDecompressor.cs $
// 
// *****************  Version 1  *****************
// User: Brentfo      Date: 9/08/11    Time: 10:20a
// Created in $/UtilitiesClassLibrary/Utilities/CAB
// Added CAB library to Utilities library for extracting and creating CAB
// files.
// 

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Utilities.CAB
{
    #region FdiDecrypt Event

    /// <summary>
    /// Holds data for the Decrypt Event
    /// </summary>
    public class FdiDecryptEventArgs : EventArgs
    {
        private FdiDecrypt fdid;
        private int result;

        /// <summary>
        /// Initializes a new instance of the FdiDecryptEventArgs class.
        /// </summary>
        /// <param name="f">An FdiDecrypt structure that contains information about the decryption operation.</param>
        public FdiDecryptEventArgs(FdiDecrypt f)
        {
            fdid = f;
            result = 0;
        }

        /// <summary>
        /// Gets the FdiDecrypt structure.
        /// </summary>
        public FdiDecrypt args
        {
            get { return fdid; }
        }

        /// <summary>
        /// Gets or sets the result of the event handler.
        /// </summary>
        public int Result
        {
            get { return result; }
            set { result = value; }
        }
    }

    /// <summary>
    /// Occurs when the File Decompression Interface (FDI) needs to decrypt data.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A <see cref="FdiDecryptEventArgs"/> that contains the event data.</param>
    public delegate void DecryptEventHandler(object sender, FdiDecryptEventArgs e);

    #endregion

    #region Notify event

    /// <summary>
    /// Holds data for the Notify event.
    /// </summary>
    public class NotifyEventArgs : EventArgs
    {
        private FdiNotification fdin;
        private int result;

        /// <summary>
        /// Initializes a new instance of the NotifyEventArgs class.
        /// </summary>
        /// <param name="f">An <see cref="FdiNotification"/> struction that contains notification information.</param>
        public NotifyEventArgs(FdiNotification f)
        {
            fdin = f;
            result = 0;
        }

        /// <summary>
        /// Gets the <see cref="FdiNotification"/> information for the event.
        /// </summary>
        public FdiNotification args
        {
            get { return fdin; }
        }

        /// <summary>
        /// Gets or sets the result of the event handler.
        /// </summary>
        public int Result
        {
            get { return result; }
            set { result = value; }
        }
    }

    /// <summary>
    /// Occurs when the File Decompression Interface (FDI) sends a notification message.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A <see cref="NotifyEventArgs"/> that contains the event data.</param>
    public delegate void NotifyEventHandler(object sender, NotifyEventArgs e);
    #endregion

    /// <summary>
    /// Provides access to the Cabinet SDK's File Decompression Interface (FDI).
    /// </summary>
    public class CabDecompressor : IDisposable
    {
        #region Private members
        private IntPtr hfdi = IntPtr.Zero;	// decompression context

        //gerrard: added private members for the delegates
        private FdiFileReadDelegate FileReadDelegate;
        private FdiFileOpenDelegate FileOpenDelegate;
        private FdiMemAllocDelegate MemAllocDelegate;
        private FdiFileSeekDelegate FileSeekDelegate;
        private FdiMemFreeDelegate MemFreeDelegate;
        private FdiFileWriteDelegate FileWriteDelegate;
        private FdiFileCloseDelegate FileCloseDelegate;

        private bool disposed = false;

        private CabError erf = new CabError();

        private object userData = null;
        private GCHandle gchUserData;
        private bool nullUserData = true;
        #endregion

        #region Constructor and Destructor

        /// <summary>
        /// Initializes a new instance of the CabDecompressor class.
        /// </summary>
        public CabDecompressor()
        {
            //gerrard: initializing new private members
            FileReadDelegate = new FdiFileReadDelegate(FileRead);
            FileOpenDelegate = new FdiFileOpenDelegate(FileOpen);
            MemAllocDelegate = new FdiMemAllocDelegate(MemAlloc);
            FileSeekDelegate = new FdiFileSeekDelegate(FileSeek);
            MemFreeDelegate = new FdiMemFreeDelegate(MemFree);
            FileWriteDelegate = new FdiFileWriteDelegate(FileWrite);
            FileCloseDelegate = new FdiFileCloseDelegate(FileClose);
        }

        /// <summary>
        /// The CabDecompressor class destructor.  Disposes a CabDecompressor by calling Dispose(false). 
        /// This method overrides System.Object.Finalize.  Application code should not call this method.
        /// An object's Finalize method is automatically invoked during garbage collection unless finalization
        /// by the garbage collector has been disabled by a call to the GC.SuppressFinalize method.
        /// </summary>
        ~CabDecompressor()
        {
            Dispose(false);
        }

        /// <summary>
        /// Releases all resources used by the CabDecompressor.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the CabDecompressor and optionally releases the managed resources. 
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    // if disposing is true, dispose all managed and unmanaged resources
                }
                if (hfdi != IntPtr.Zero)
                {
                    Trace.WriteLine("Destroying...");
                    CabSdk.FdiDestroy(hfdi);
                    hfdi = IntPtr.Zero;
                }
                if (!nullUserData)
                {
                    gchUserData.Free();
                    nullUserData = true;
                }

                disposed = true;
            }
        }
        #endregion

        #region Public properties

        /// <summary>
        /// Gets the error object.
        /// </summary>
        public CabError ErrorInfo
        {
            get { return erf; }
        }

        /// <summary>
        /// Gets or sets the user data object.
        /// </summary>
        public object UserData
        {
            get { return userData; }
            set
            {
                if (!nullUserData)
                {
                    gchUserData.Free();
                    nullUserData = true;
                }
                userData = value;
                nullUserData = (userData == null);
                if (!nullUserData)
                    gchUserData = GCHandle.Alloc(userData);
            }
        }

        /// <summary>
        /// Returns the current FDI context (hfdi).
        /// </summary>
        /// <remarks>
        /// If there is no FDI context defined, one is created.
        /// </remarks>
        public IntPtr FdiContext
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("CabDecompressor");
                }

                if (hfdi == IntPtr.Zero)
                {
                    //gerrard: using the new private members
                    hfdi = CabSdk.FdiCreate(
                        MemAllocDelegate,
                        MemFreeDelegate,
                        FileOpenDelegate,
                        FileReadDelegate,
                        FileWriteDelegate,
                        FileCloseDelegate,
                        FileSeekDelegate,
                        ref erf);
                    if (hfdi == IntPtr.Zero)
                    {
                        throw new ApplicationException("Failed to create FDI context.");
                    }
                }
                return hfdi;
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Determines whether a file, identified by a file name, is a cabinet file.
        /// </summary>
        /// <param name="filename">The name of the file to be tested.</param>
        /// <param name="cabinfo">A <see cref="FdiCabinetInfo"/> object that will receive information about the cabinet file.</param>
        /// <returns>Returns True if the file is a cabinet.  If so, the cabinfo object is filled with information about the cabinet file.
        /// Returns False if the file is not a cabinet.</returns>
        public bool IsCabinetFile(string filename, FdiCabinetInfo cabinfo)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("CabDecompressor");
            }

            using (FileStream fs = File.Open(filename, FileMode.Open,
                       FileAccess.Read, FileShare.ReadWrite))
            {
                return IsCabinetFile(fs, cabinfo);
            }
        }

        /// <summary>
        /// Determines whether a file, identified by a stream, is a cabinet file.
        /// </summary>
        /// <param name="fstream">The open file stream to be tested.</param>
        /// <param name="cabinfo">A <see cref="FdiCabinetInfo"/> object that will receive information about the cabinet file.</param>
        /// <returns>Returns True if the file is a cabinet.  If so, the cabinfo object is filled with information about the cabinet file.
        /// Returns False if the file is not a cabinet.</returns>
        public bool IsCabinetFile(Stream fstream, FdiCabinetInfo cabinfo)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("CabDecompressor");
            }

            GCHandle gch = GCHandle.Alloc(fstream);
            try
            {
                return CabSdk.FdiIsCabinet(FdiContext, (IntPtr)gch, cabinfo);
            }
            finally
            {
                gch.Free();
            }
        }

        /// <summary>
        /// Extract files from a cabinet.
        /// </summary>
        /// <param name="cabinetFullPath">Full path name of the cabinet from which files are to be extracted.</param>
        /// <returns>Returns True if extraction was successful.  Returns False on error.</returns>
        /// <remarks>On error, the <see cref="ErrorInfo"/> structure will contain error information.</remarks>
        public bool ExtractFiles(string cabinetFullPath)
        {
            Trace.WriteLine("ExtractFiles: {0}", cabinetFullPath);
            if (disposed)
            {
                throw new ObjectDisposedException("CabDecompressor");
            }

            string path = Path.GetDirectoryName(cabinetFullPath);
            string filename = Path.GetFileName(cabinetFullPath);
            Trace.WriteLine("path = {0}", path);
            Trace.WriteLine("filename = {0}", filename);
            return CabSdk.FdiCopy(FdiContext,
                filename,
                path,
                new FdiNotifyDelegate(NotifyCallback),
                null, //new FdiDecryptDelegate(DecryptCallback),
                userData);
        }
        #endregion

        #region FDI callbacks
        /// <summary>
        /// Allocate a block of memory.
        /// </summary>
        /// <param name="cb">The number of bytes to be allocated.</param>
        /// <returns>An IntPtr that references the allocated memory.</returns>
        /// <remarks>The File Decompression Interface (FDI) calls this function to allocate memory
        /// for its internal use.</remarks>
        protected virtual IntPtr MemAlloc(int cb)
        {
            IntPtr m = CabIO.MemAlloc(cb);
            Trace.WriteLine(string.Format("MemAlloc {0} = {1}", cb, m));
            return m;
        }

        /// <summary>
        /// Frees a block of memory previously allocated by MemAlloc.
        /// </summary>
        /// <param name="mem">An IntPtr that references the memory to be freed.</param>
        /// <remarks>The File Decompression Interface (FDI) calls this function to free
        /// memory that it allocated with a call to <see cref="MemAlloc"/>.</remarks>
        protected virtual void MemFree(IntPtr mem)
        {
            Trace.WriteLine(string.Format("MemFree {0}", mem));
            CabIO.MemFree(mem);
        }

        /// <summary>
        /// Opens a file.
        /// </summary>
        /// <param name="fileName">The path name of the file to be opened.</param>
        /// <param name="oflag">Open mode flags.</param>
        /// <param name="pmode">Share mode flags.</param>
        /// <returns>Returns an IntPtr that references the open file handle.  Returns -1 on error.</returns>
        /// <remarks>The File Decompression Interface (FDI) calls this function to open files.</remarks>
        protected virtual IntPtr FileOpen(string fileName, int oflag, int pmode)
        {
            Trace.WriteLine("FileOpen {0}", fileName);
            int err = 0;
            try
            {
                return CabIO.FileOpen(fileName, oflag, pmode, ref err, userData);
            }
            finally
            {
                erf.ErrorType = err;
            }
        }

        /// <summary>
        /// Reads bytes from a file.
        /// </summary>
        /// <param name="hf">The file handle from which to read.</param>
        /// <param name="buffer">The buffer where read bytes are placed.</param>
        /// <param name="cb">Size of the read buffer.</param>
        /// <returns>Returns the number of bytes read.</returns>
        /// <remarks>The File Decompression Interface (FDI) calls this function to read from a file.</remarks>
        protected virtual int FileRead(IntPtr hf, byte[] buffer, int cb)
        {
            Trace.WriteLine(string.Format("FileRead {0}", hf));
            int err = 0;
            try
            {
                return CabIO.FileRead(hf, buffer, cb, ref err, userData);
            }
            finally
            {
                erf.ErrorType = err;
            }
        }

        /// <summary>
        /// Writes bytes to a file.
        /// </summary>
        /// <param name="hf">The file handle to which data is to be written.</param>
        /// <param name="buffer">The buffer that contains the data to be written.</param>
        /// <param name="cb">The number of bytes to be written.</param>
        /// <returns>Returns the number of bytes written.</returns>
        /// <remarks>The File Decompression Interface (FDI) calls this function to write to a file.</remarks>
        protected virtual int FileWrite(IntPtr hf, byte[] buffer, int cb)
        {
            Trace.WriteLine(string.Format("FileWrite {0}", hf));
            int err = 0;
            try
            {
                return CabIO.FileWrite(hf, buffer, cb, ref err, userData);
            }
            finally
            {
                erf.ErrorType = err;
            }
        }

        /// <summary>
        /// Closes a file.
        /// </summary>
        /// <param name="hf">Handle to the file to be closed.</param>
        /// <returns>Returns 0 on success.  Returns -1 on error.</returns>
        /// <remarks>The File Decompression Interface (FDI) calls this function to close a file.</remarks>
        protected virtual int FileClose(IntPtr hf)
        {
            Trace.WriteLine(string.Format("FileWrite {0}", hf));
            int err = 0;
            try
            {
                return CabIO.FileClose(hf, ref err, userData);
            }
            finally
            {
                erf.ErrorType = err;
            }
        }

        /// <summary>
        /// Sets the current position in a file to the given value.
        /// </summary>
        /// <param name="hf">The handle of an open file.</param>
        /// <param name="dist">The number of bytes to move the pointer.</param>
        /// <param name="seekType">The starting position for the move.  Values are SEEK_CUR, SEEK_END, or SEEK_SET.</param>
        /// <returns>Returns the new file position.  Returns -1 on error.</returns>
        /// <remarks>The File Decompression Interface (FDI) calls this function to position the file pointer.</remarks>
        protected virtual int FileSeek(IntPtr hf, int dist, int seektype)
        {
            Trace.WriteLine(string.Format("FileSeek {0}", hf));
            int err = 0;
            try
            {
                return CabIO.FileSeek(hf, dist, seektype, ref err, userData);
            }
            finally
            {
                erf.ErrorType = err;
            }
        }

        /// <summary>
        /// Receives decryption callback requests from the File Decompression Interface (FDI)
        /// </summary>
        /// <param name="fdid">A <see cref="FdiDecrypt"/> structure containing decrypt information.</param>
        /// <returns>Returns -1 on error.  Any other value indicates success.</returns>
        /// <remarks>The File Decompression Interface (FDI) calls this function during decompression so that
        /// clients can decrypt encrypted data.  There are three notification types:  Decrypt, NewCabinet, and NewFolder.
        /// Clients that wish to receive these notifications must handle the <see cref="DecryptDataBlock"/>, <see cref="DecryptNewCabinet"/>,
        /// and <see cref="DecryptNewFolder"/> events.</remarks>
        protected virtual int DecryptCallback(ref FdiDecrypt fdid)
        {
            Trace.WriteLine("DecryptCallback");
            FdiDecryptEventArgs e = new FdiDecryptEventArgs(fdid);

            // fire the proper event
            switch (fdid.DecryptType)
            {
                case FdiDecryptType.Decrypt:
                    OnDecryptDataBlock(e);
                    break;
                case FdiDecryptType.NewCabinet:
                    OnDecryptNewCabinet(e);
                    break;
                case FdiDecryptType.NewFolder:
                    OnDecryptNewFolder(e);
                    break;
            }
            return 0;
        }

        /// <summary>
        /// Receives notification callbacks from the File Decompression Interface (FDI).
        /// </summary>
        /// <param name="fdint">A <see cref="FdiNotificationType"/> value that specifies which notification is being sent.</param>
        /// <param name="fdin">A <see cref="FdiNotification"/> that contains information about the notification</param>
        /// <returns>The return value is dependent on the notification type.</returns>
        /// <remarks>The File Decompression Interface (FDI)provides notifications for six different events.  Clients must respond to those
        /// notifications by handling the <see cref="NotifyCabinetInfo"/>, <see cref="NotifyCloseFile"/>, <see cref="NotifyCopyFile"/>,
        /// <see cref="NotifyEnumerate"/>, <see cref="NotifyNextCabinet"/>, and <see cref="NotifyPartialFile"/> events.</remarks>
        protected virtual int NotifyCallback(FdiNotificationType fdint, FdiNotification fdin)
        {
            Trace.WriteLine(string.Format("NotifyCallback: type {0}", fdint));

            NotifyEventArgs e = new NotifyEventArgs(fdin);

            // fire the proper event
            switch (fdint)
            {
                case FdiNotificationType.CabinetInfo:
                    OnNotifyCabinetInfo(e);
                    break;
                case FdiNotificationType.CloseFileInfo:
                    OnNotifyCloseFile(e);
                    break;
                case FdiNotificationType.CopyFile:
                    OnNotifyCopyFile(e);
                    break;
                case FdiNotificationType.Enumerate:
                    OnNotifyEnumerate(e);
                    break;
                case FdiNotificationType.NextCabinet:
                    OnNotifyNextCabinet(e);
                    break;
                case FdiNotificationType.PartialFile:
                    OnNotifyPartialFile(e);
                    break;
            }
            return e.Result;
        }

        #endregion


        #region Decryption Events

        /// <summary>
        /// Occurs when the File Decompression Interface (FDI) sends a <see cref="FdiDecryptType.NewCabinet"/> notification.
        /// </summary>
        public event DecryptEventHandler DecryptNewCabinet;

        /// <summary>
        /// Occurs when the File Decompression Interface (FDI) sends a <see cref="FdiDecryptType.NewFolder"/> notification.
        /// </summary>
        public event DecryptEventHandler DecryptNewFolder;

        /// <summary>
        /// Occurs when the File Decompression Interface (FDI) sends a <see cref="FdiDecryptType.NewCabinet"/> notification.
        /// </summary>
        public event DecryptEventHandler DecryptDataBlock;

        /// <summary>
        /// Raises the <see cref="DecryptNewCabinet"/> event.
        /// </summary>
        /// <param name="e">A <see cref="FdiDecryptEventArgs"/> that contains the event data.</param>
        protected virtual void OnDecryptNewCabinet(FdiDecryptEventArgs e)
        {
            if (DecryptNewCabinet != null)
                DecryptNewCabinet(this, e);
        }

        /// <summary>
        /// Raises the <see cref="DecryptNewFolder"/> event.
        /// </summary>
        /// <param name="e">A <see cref="FdiDecryptEventArgs"/> that contains the event data.</param>
        protected virtual void OnDecryptNewFolder(FdiDecryptEventArgs e)
        {
            if (DecryptNewFolder != null)
                DecryptNewFolder(this, e);
        }

        /// <summary>
        /// Raises the <see cref="DecryptDataBlock"/> event.
        /// </summary>
        /// <param name="e">A <see cref="FdiDecryptEventArgs"/> that contains the event data.</param>
        protected virtual void OnDecryptDataBlock(FdiDecryptEventArgs e)
        {
            if (DecryptDataBlock != null)
                DecryptDataBlock(this, e);
        }
        #endregion

        #region Notification Events

        /// <summary>
        /// Occurs when the File Decompression Interface (FDI) sends a <see cref="FdiNotificationType.CabinetInfo"/> notification.
        /// </summary>
        public event NotifyEventHandler NotifyCabinetInfo;

        /// <summary>
        /// Occurs when the File Decompression Interface (FDI) sends a <see cref="FdiNotificationType.PartialFile"/> notification.
        /// </summary>
        public event NotifyEventHandler NotifyPartialFile;

        /// <summary>
        /// Occurs when the File Decompression Interface (FDI) sends a <see cref="FdiNotificationType.CopyFile"/> notification.
        /// </summary>
        public event NotifyEventHandler NotifyCopyFile;

        /// <summary>
        /// Occurs when the File Decompression Interface (FDI) sends a <see cref="FdiNotificationType.CloseFileInfo"/> notification.
        /// </summary>
        public event NotifyEventHandler NotifyCloseFile;

        /// <summary>
        /// Occurs when the File Decompression Interface (FDI) sends a <see cref="FdiNotificationType.NextCabinet"/> notification.
        /// </summary>
        public event NotifyEventHandler NotifyNextCabinet;

        /// <summary>
        /// Occurs when the File Decompression Interface (FDI) sends a <see cref="FdiNotificationType.Enumerate"/> notification.
        /// </summary>
        public event NotifyEventHandler NotifyEnumerate;

        /// <summary>
        /// Raises the <see cref="NotifyCabinetInfo"/> event.
        /// </summary>
        /// <param name="e">A <see cref="NotifyEventArgs"/> that contains the event data.</param>
        protected virtual void OnNotifyCabinetInfo(NotifyEventArgs e)
        {
            Trace.WriteLine("OnNotifyCabinetInfo");
            if (NotifyCabinetInfo != null)
                NotifyCabinetInfo(this, e);
        }

        /// <summary>
        /// Raises the <see cref="NotifyPartialFile"/> event.
        /// </summary>
        /// <param name="e">A <see cref="NotifyEventArgs"/> that contains the event data.</param>
        protected virtual void OnNotifyPartialFile(NotifyEventArgs e)
        {
            Trace.WriteLine("OnNotifyPartialFile");
            if (NotifyPartialFile != null)
                NotifyPartialFile(this, e);
        }

        /// <summary>
        /// Raises the <see cref="NotifyCopyFile"/> event.
        /// </summary>
        /// <param name="e">A <see cref="NotifyEventArgs"/> that contains the event data.</param>
        protected virtual void OnNotifyCopyFile(NotifyEventArgs e)
        {
            Trace.WriteLine("OnNotifyCopyFile");
            if (NotifyCopyFile != null)
                NotifyCopyFile(this, e);
        }

        /// <summary>
        /// Raises the <see cref="NotifyCloseFile"/> event.
        /// </summary>
        /// <param name="e">A <see cref="NotifyEventArgs"/> that contains the event data.</param>
        protected virtual void OnNotifyCloseFile(NotifyEventArgs e)
        {
            Trace.WriteLine("OnNotifyCloseFile");
            if (NotifyCloseFile != null)
                NotifyCloseFile(this, e);
        }

        /// <summary>
        /// Raises the <see cref="NotifyNextCabinet"/> event.
        /// </summary>
        /// <param name="e">A <see cref="NotifyEventArgs"/> that contains the event data.</param>
        protected virtual void OnNotifyNextCabinet(NotifyEventArgs e)
        {
            Trace.WriteLine("OnNotifyNextCabinet");
            if (NotifyNextCabinet != null)
                NotifyNextCabinet(this, e);
        }

        /// <summary>
        /// Raises the <see cref="NotifyEnumerate"/> event.
        /// </summary>
        /// <param name="e">A <see cref="NotifyEventArgs"/> that contains the event data.</param>
        protected virtual void OnNotifyEnumerate(NotifyEventArgs e)
        {
            Trace.WriteLine("OnNotifyEnumerate");
            if (NotifyEnumerate != null)
                NotifyEnumerate(this, e);
        }

        #endregion
    }
}