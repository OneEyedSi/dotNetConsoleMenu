//
// $History: CabCompressor.cs $
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
    #region NextCabinet Event

    /// <summary>
    /// Holds data for the NextCabinet event
    /// </summary>
    /// <remarks></remarks>
    public class NextCabinetEventArgs : System.EventArgs
    {
        private FciCurrentCab ccab;
        private int cbPrevCab;
        private object userData;
        private bool result;

        /// <summary>
        /// Initializes a new instance of the NextCabinetEventArgs class
        /// </summary>
        /// <param name="c">The FciCurrentCab class instance for this event.</param>
        /// <param name="cb">Size of the previous cabinet.</param>
        /// <param name="uData">User data.</param>
        /// <remarks></remarks>
        public NextCabinetEventArgs(FciCurrentCab c, int cb, object uData)
        {
            Trace.WriteLine("NextCabinetEventArgs constructor");
            ccab = c;
            cbPrevCab = cb;
            userData = uData;
            result = true;
        }

        /// <summary>
        /// Gets the cabinet parameters for the current cabinet file.
        /// </summary>
        /// <value>An FciCurrentCab instance</value>
        public FciCurrentCab CabInfo
        {
            get { return ccab; }
        }

        /// <summary>
        /// Gets the size of the previous cabinet file.
        /// </summary>
        /// <value>Size of the previous cabinet</value>
        public int PrevCabSize
        {
            get { return cbPrevCab; }
        }

        /// <summary>
        /// Gets the user data object
        /// </summary>
        /// <value>An Object instance which the calling function must cast to the proper type.</value>
        public object UserData
        {
            get { return userData; }
        }

        /// <summary>
        /// Gets or sets the result of the event handler.
        /// </summary>
        public bool Result
        {
            get { return result; }
            set { result = value; }
        }
    }

    /// <summary>
    /// Occurs when a new cabinet file is being created.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A <see cref="NextCabinetEventArgs"/> that contains the event data.</param>
    public delegate void NextCabinetEventHandler(object sender, NextCabinetEventArgs e);

    #endregion

    #region FilePlaced Event

    /// <summary>
    /// Holds data for the FilePlaced event
    /// </summary>
    public class FilePlacedEventArgs : System.EventArgs
    {
        public FciCurrentCab ccab;
        private string fileName;
        private Int32 cbFile;
        private bool fContinuation;
        private object userData;
        private int result = 0;

        /// <summary>
        /// Initializes a new instance of the FilePlacedEventArgs class
        /// </summary>
        /// <param name="c">Cabinet file parameters of type FciCurrentCab.</param>
        /// <param name="fname">Name of the file being placed.</param>
        /// <param name="cb">Size of the file.</param>
        /// <param name="fCont">A value indicating whether this is a continuation.</param>
        /// <param name="udata">User data object.</param>
        /// <remarks></remarks>
        public FilePlacedEventArgs(FciCurrentCab c, string fname, Int32 cb,
            bool fCont, object udata)
        {
            ccab = c;
            fileName = fname;
            cbFile = cb;
            fContinuation = fCont;
            userData = udata;
        }

        /// <summary>
        /// Gets the cabinet parameters for the current cabinet file.
        /// </summary>
        /// <value>An FciCurrentCab instance</value>
        public FciCurrentCab CabInfo
        {
            get { return ccab; }
        }

        /// <summary>
        /// Gets the name of the file being placed in the cabinet.
        /// </summary>
        /// <value>A file name.</value>
        public string FileName
        {
            get { return fileName; }
        }

        /// <summary>
        /// Gets the size of the file being placed in the cabinet.
        /// </summary>
        /// <value>The file size.</value>
        public Int32 FileSize
        {
            get { return cbFile; }
        }

        /// <summary>
        /// Gets the user data object.
        /// </summary>
        /// <value>An Object instance which the calling function must cast to the proper type.</value>
        public object UserData
        {
            get { return userData; }
        }

        /// <summary>
        /// Gets a value that indicates whether this file is a continuation of a previous file.
        /// </summary>
        public bool Continuation
        {
            get { return fContinuation; }
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
    /// Occurs when a file is placed in the cabinet.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A <see cref="FilePlacedEventArgs"/> that contains the event data.</param>
    public delegate void FilePlacedEventHandler(object sender, FilePlacedEventArgs e);

    #endregion

    #region Progress Event

    /// <summary>
    /// Holds data for the Progress event
    /// </summary>
    public class ProgressEventArgs : System.EventArgs
    {
        private int cb1;
        private int cb2;
        private object userData;
        private int result;

        /// <summary>
        /// Initializes a new instance of the ProgressEventArgs class
        /// </summary>
        /// <param name="c1">First parameter supplied by FCI.</param>
        /// <param name="c2">Second parameter supplied by FCI.</param>
        /// <param name="udata">User data object.</param>
        /// <remarks></remarks>
        public ProgressEventArgs(int c1, int c2, object udata)
        {
            cb1 = c1;
            cb2 = c2;
            userData = udata;
            result = 0;
        }

        /// <summary>
        /// Gets the compressed block size for FileAdded progress notifications.
        /// </summary>
        public int CompressedBlockSize
        {
            get { return cb1; }
        }

        /// <summary>
        /// Gets the uncompressed block size for FileAdded progress notifications.
        /// </summary>
        public int UncompressedBlockSize
        {
            get { return cb2; }
        }

        /// <summary>
        /// Gets the number of bytes copied for FolderComplete progress notifications.
        /// </summary>
        public int FolderBytesCopied
        {
            get { return cb1; }
        }

        /// <summary>
        /// Gets the folder size for FolderComplete progress notifications.
        /// </summary>
        public int FolderSize
        {
            get { return cb2; }
        }

        /// <summary>
        /// Get the estimated cabinet file size for CabinetComplete notifications.
        /// </summary>
        public int EstimatedCabSize
        {
            get { return cb1; }
        }

        /// <summary>
        /// Gets the actual cabinet file size for CabinetComplete notifications.
        /// </summary>
        public int ActualCabSize
        {
            get { return cb2; }
        }

        /// <summary>
        /// Gets the user data object.
        /// </summary>
        /// <value>An Object instance which the calling function must cast to the proper type.</value>
        public object UserData
        {
            get { return userData; }
        }

        /// <summary>
        /// Gets or sets the event handler result.
        /// </summary>
        public int Result
        {
            get { return result; }
            set { result = value; }
        }
    }

    /// <summary>
    /// Occurs periodically to inform calling applications of compression progress.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A <see cref="ProgressEventArgs"/> that contains the event data.</param>
    public delegate void ProgressEventHandler(object sender, ProgressEventArgs e);

    #endregion

    /// <summary>
    /// Provides access to the Cabinet SDK's File Compression Interface (FCI)
    /// </summary>
    public class CabCompressor : IDisposable
    {
        #region Private data members

        //gerrard: added private members for the delegates
        private FciFilePlacedDelegate FilePlacedDelegate;
        private FciMemAllocDelegate MemAllocDelegate;
        private FciMemFreeDelegate MemFreeDelegate;
        private FciFileOpenDelegate FileOpenDelegate;
        private FciFileReadDelegate FileReadDelegate;
        private FciFileWriteDelegate FileWriteDelegate;
        private FciFileCloseDelegate FileCloseDelegate;
        private FciFileSeekDelegate FileSeekDelegate;
        private FciFileDeleteDelegate FileDeleteDelegate;
        private FciGetTempFileDelegate GetTempFileDelegate;

        private bool disposed = false;

        private CabError erf = new CabError();
        private FciCurrentCab ccab = new FciCurrentCab();

        private object userData = null;
        private GCHandle gchUserData;
        private bool nullUserData = true;

        private IntPtr hfci = IntPtr.Zero;

        #endregion

        #region Constructor and destructor
        /// <summary>
        /// Initializes new instance of the CabCompressor class.
        /// </summary>
        public CabCompressor()
        {
            // user must initialize a CabInfo structure
            //gerrard: initializing new private members
            FilePlacedDelegate = new FciFilePlacedDelegate(FilePlacedCallback);
            MemAllocDelegate = new FciMemAllocDelegate(MemAlloc);
            MemFreeDelegate = new FciMemFreeDelegate(MemFree);
            FileOpenDelegate = new FciFileOpenDelegate(FileOpen);
            FileReadDelegate = new FciFileReadDelegate(FileRead);
            FileWriteDelegate = new FciFileWriteDelegate(FileWrite);
            FileCloseDelegate = new FciFileCloseDelegate(FileClose);
            FileSeekDelegate = new FciFileSeekDelegate(FileSeek);
            FileDeleteDelegate = new FciFileDeleteDelegate(FileDelete);
            GetTempFileDelegate = new FciGetTempFileDelegate(GetTempFile);
        }

        /// <summary>
        /// The CabCompressor class destructor.  Disposes a CabCompressor by calling Dispose(false). 
        /// This method overrides System.Object.Finalize.  Application code should not call this method.
        /// An object's Finalize method is automatically invoked during garbage collection unless finalization
        /// by the garbage collector has been disabled by a call to the GC.SuppressFinalize method.
        /// </summary>
        ~CabCompressor()
        {
            Dispose(false);
        }

        /// <summary>
        /// Releases all resources used by the CabCompressor.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the CabCompressor and optionally releases the managed resources. 
        /// </summary>
        /// <param name="disposing"></param>
        /// <remarks></remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    // if disposing is true, dispose all managed and unmanaged resources
                }
                if (hfci != IntPtr.Zero)
                {
                    CabSdk.FciDestroy(hfci);
                    hfci = IntPtr.Zero;
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
        /// Gets the current error status.
        /// </summary>
        public CabError ErrorInfo
        {
            get { return erf; }
        }

        /// <summary>
        /// Gets the current cabinet file parameters.
        /// </summary>
        public FciCurrentCab CabInfo
        {
            get { return ccab; }
        }

        /// <summary>
        /// Gets or sets the user data object.
        /// </summary>
        public object UserData
        {
            get
            {
                Trace.WriteLine("get_UserData");
                return userData;
            }
            set
            {
                Trace.WriteLine("Set user data");
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

        private object GetUserDataObject(IntPtr udata)
        {
            Trace.WriteLine("GetUserDataObject");
            if (udata == IntPtr.Zero)
                return null;
            return ((GCHandle)udata).Target;
        }

        /// <summary>
        /// Gets the current FCI context.
        /// </summary>
        public IntPtr FciContext
        {
            get
            {
                Trace.WriteLine("get_FciContext");
                if (disposed)
                {
                    throw new ObjectDisposedException("CabCompressor");
                }
                if (hfci == IntPtr.Zero)
                {
                    Trace.WriteLine("Creating FCI context");
                    //gerrard: using the new private members
                    hfci = CabSdk.FciCreate(ref erf,
                        FilePlacedDelegate,
                        MemAllocDelegate,
                        MemFreeDelegate,
                        FileOpenDelegate,
                        FileReadDelegate,
                        FileWriteDelegate,
                        FileCloseDelegate,
                        FileSeekDelegate,
                        FileDeleteDelegate,
                        GetTempFileDelegate,
                        ccab,
                        (IntPtr)gchUserData);
                    if (hfci == IntPtr.Zero)
                    {
                        throw new ApplicationException("Failed to create FCI context.");
                    }
                }
                return hfci;
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Adds a file to the current cabinet.
        /// </summary>
        /// <param name="filename">The full path name of the file to be added.</param>
        /// <param name="nameInCab">The name that the file will have in the cabinet.</param>
        /// <param name="execOnDecompress">A flag that indicates whether the file should be executed when it is decompressed.
        /// This is just a notification flag.  Decompression programs must query the flag and execute the file if desired.</param>
        /// <param name="compressType">Type of compression to use for this file.</param>
        public void AddFile(string filename, string nameInCab,
            bool execOnDecompress, FciCompression compressType)
        {
            Trace.WriteLine("AddFile");
            if (disposed)
            {
                throw new ObjectDisposedException("CabCompressor");
            }
            if (!CabSdk.FciAddFile(FciContext, filename, nameInCab, execOnDecompress,
                new FciGetNextCabinetDelegate(GetNextCabinet),
                new FciStatusDelegate(ProgressFunc),
                new FciGetOpenInfoDelegate(GetOpenInfo),
                compressType))
            {
                throw new ApplicationException(string.Format("AddFile failed with code {0}", erf.FciErrorCode));
            }
        }

        /// <summary>
        /// Flus the current folder in the cabinet file.
        /// </summary>
        public void FlushFolder()
        {
            Trace.WriteLine("FlushFolder");
            if (disposed)
            {
                throw new ObjectDisposedException("CabCompressor");
            }
            if (!CabSdk.FciFlushFolder(FciContext,
                new FciGetNextCabinetDelegate(GetNextCabinet),
                new FciStatusDelegate(ProgressFunc)))
            {
                Trace.WriteLine("FciFlushFolder failed.");
                throw new ApplicationException(string.Format("FlushFolder failed with code {0}", erf.FciErrorCode));
            }
        }

        /// <summary>
        /// Flush the current cabinet file.
        /// </summary>
        public void FlushCabinet()
        {
            Trace.WriteLine("FlushCabinet");
            if (disposed)
            {
                throw new ObjectDisposedException("CabCompressor");
            }
            if (!CabSdk.FciFlushCabinet(FciContext, false,
                new FciGetNextCabinetDelegate(GetNextCabinet),
                new FciStatusDelegate(ProgressFunc)))
            {
                Trace.WriteLine("FciFlushCabinet failed.");
                throw new ApplicationException(string.Format("FlushCabinet failed with code {0}", erf.FciErrorCode));
            }
        }
        #endregion

        #region FCI Callbacks

        /// <summary>
        /// Allocate a block of memory.
        /// </summary>
        /// <param name="cb">The number of bytes to be allocated.</param>
        /// <returns>An IntPtr that references the allocated memory.</returns>
        /// <remarks>The File Compression Interface (FCI) calls this function to allocate memory
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
        /// <remarks>The File Compression Interface (FCI) calls this function to free
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
        /// <param name="err">Error return value.</param>
        /// <param name="pUserData">User data object.</param>
        /// <returns>Returns an IntPtr that references the open file handle.  Returns -1 on error.</returns>
        /// <remarks>The File Compression Interface (FCI) calls this function to open files.</remarks>
        protected virtual IntPtr FileOpen(string fileName, int oflag, int pmode,
            ref int err, IntPtr pUserData)
        {
            Trace.WriteLine(string.Format("FileOpen {0}", fileName));
            return CabIO.FileOpen(fileName, oflag, pmode, ref err, ((GCHandle)pUserData).Target);
        }

        /// <summary>
        /// Reads bytes from a file.
        /// </summary>
        /// <param name="hf">The file handle from which to read.</param>
        /// <param name="buffer">The buffer where read bytes are placed.</param>
        /// <param name="cb">Size of the read buffer.</param>
        /// <param name="err">Error return value.</param>
        /// <param name="pUserData">User data object.</param>
        /// <returns>Returns the number of bytes read.</returns>
        /// <remarks>The File Compression Interface (FCI) calls this function to read from a file.</remarks>
        protected virtual int FileRead(IntPtr hf, byte[] buffer, int cb,
            ref int err, IntPtr pUserData)
        {
            Trace.WriteLine(string.Format("FileRead {0}", hf));
            int bytesRead = CabIO.FileRead(hf, buffer, cb, ref err, ((GCHandle)pUserData).Target);
            Trace.WriteLine(string.Format("Returning {0}", bytesRead));
            return bytesRead;
        }

        /// <summary>
        /// Writes bytes to a file.
        /// </summary>
        /// <param name="hf">The file handle to which data is to be written.</param>
        /// <param name="buffer">The buffer that contains the data to be written.</param>
        /// <param name="cb">The number of bytes to be written.</param>
        /// <param name="err">Error return value.</param>
        /// <param name="pUserData">User data object.</param>
        /// <returns>Returns the number of bytes written.</returns>
        /// <remarks>The File Compression Interface (FCI) calls this function to write to a file.</remarks>
        protected virtual int FileWrite(IntPtr hf, byte[] buffer, int cb,
            ref int err, IntPtr pUserData)
        {
            Trace.WriteLine(string.Format("FileWrite {0}", hf));
            return CabIO.FileWrite(hf, buffer, cb, ref err, ((GCHandle)pUserData).Target);
        }

        /// <summary>
        /// Closes a file.
        /// </summary>
        /// <param name="hf">Handle to the file to be closed.</param>
        /// <param name="err">Error return value.</param>
        /// <param name="pUserData">User data object.</param>
        /// <returns>Returns 0 on success.  Returns -1 on error.</returns>
        /// <remarks>The File Compression Interface (FCI) calls this function to close a file.</remarks>
        protected virtual int FileClose(IntPtr hf, ref int err, IntPtr pUserData)
        {
            Trace.WriteLine(string.Format("FileClose {0}", hf));
            return CabIO.FileClose(hf, ref err, ((GCHandle)pUserData).Target);
        }

        /// <summary>
        /// Sets the current position in a file to the given value.
        /// </summary>
        /// <param name="hf">The handle of an open file.</param>
        /// <param name="dist">The number of bytes to move the pointer.</param>
        /// <param name="seekType">The starting position for the move.  Values are SEEK_CUR, SEEK_END, or SEEK_SET.</param>
        /// <param name="err">Error return value.</param>
        /// <param name="pUserData">User data object.</param>
        /// <returns>Returns the new file position.  Returns -1 on error.</returns>
        /// <remarks>The File Compression Interface (FCI) calls this function to position the file pointer.</remarks>
        protected virtual int FileSeek(IntPtr hf, int dist, int seekType,
            ref int err, IntPtr pUserData)
        {
            Trace.WriteLine(string.Format("FileSeek {0}", hf));
            return CabIO.FileSeek(hf, dist, seekType, ref err, ((GCHandle)pUserData).Target);
        }

        /// <summary>
        /// Delete a file.
        /// </summary>
        /// <param name="fileName">The name of the file to be deleted.</param>
        /// <param name="err">Error return value.</param>
        /// <param name="pUserData">User data object.</param>
        /// <returns>Returns 0 on success.  Returns -1 on error.</returns>
        /// <remarks>The File Compression Interface (FCI) calls this function to delete a file.</remarks>
        protected virtual int FileDelete(string fileName, ref int err, IntPtr pUserData)
        {
            Trace.WriteLine(string.Format("FileDelete {0}", fileName));
            return CabIO.FileDelete(fileName, ref err, ((GCHandle)pUserData).Target);
        }

        /// <summary>
        /// Creates a unique temporary file.
        /// </summary>
        /// <param name="tempName">An IntPtr which points to a block of memory into which the file name is to be stored.</param>
        /// <param name="cbTempName">Size of the file name buffer.</param>
        /// <param name="pUserData">User data pointer.</param>
        /// <returns>Returns True on success.  Returns False on failure.  On success, the name of the temporary file is
        /// returned in the memory pointed to by the tempName parameter.</returns>
        /// <remarks>The File Compression Interface (FCI) calls this function to obtain the name of a temporary file.</remarks>
        protected virtual bool GetTempFile(
            IntPtr tempName,
            int cbTempName,
            IntPtr pUserData)
        {
            Trace.WriteLine("GetTempFile");
            try
            {
                string fname = Path.GetTempFileName();
                try
                {
                    if (fname != string.Empty && fname.Length < cbTempName)
                    {
                        // Get default code page representation of string and copy to buffer
                        byte[] theString = System.Text.Encoding.Default.GetBytes(fname);
                        Marshal.Copy(theString, 0, tempName, theString.Length);
                        // add the null terminator
                        Marshal.WriteByte(tempName, theString.Length, 0);
                        return true;
                    }
                }
                finally
                {
                    // Path.GetTempFileName creates the file.
                    // Need to delete it because the cabinet functions want to create it new.
                    File.Delete(fname);
                }
            }
            catch (Exception)
            {
                // swallow it.  Yum!
                // FCI doesn't give me the opportunity to return an error code here.
            }

            return false;
        }

        /// <summary>
        /// Get information about a file.
        /// </summary>
        /// <param name="fileName">Name of the file for which information is requested.</param>
        /// <param name="rDate">A reference parameter into which the file date is stored.</param>
        /// <param name="rTime">A reference parameter into which the file time is stored.</param>
        /// <param name="attribs">A reference parameter into which file attributes are stored.</param>
        /// <param name="err">Error return value.</param>
        /// <param name="pUserData">User data object.</param>
        /// <returns>On success, returns a handle to the open file, and the rDate, rTime, and attribs parameters are filled.
        /// Returns -1 on error.</returns>
        /// <remarks>The File Compression Interface calls this function to open a file and return information about it.</remarks>
        protected virtual IntPtr GetOpenInfo(
            string fileName, //
            ref short rDate,
            ref short rTime,
            ref short attribs,
            ref int err,
            IntPtr pUserData)
        {
            Trace.WriteLine(string.Format("GetOpenInfo {0}", fileName));
            try
            {
                // Get file date/time and attributes
                FileAttributes fattr = File.GetAttributes(fileName);
                DateTime fdate = File.GetLastWriteTime(fileName);

                // Convert to format that FCI understands
                attribs = FCntl.FAttrsFromFileAttributes(fattr);
                FCntl.DosDateTimeFromDateTime(fdate, ref rDate, ref rTime);
                // open file and return handle
                return CabIO.FileOpen(fileName, FileAccess.Read, FileShare.None, FileMode.Open, ref err);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                err = 1;
            }
            return (IntPtr)(-1);
        }

        /// <summary>
        /// Gets the name of the next cabinet file.
        /// </summary>
        /// <param name="ccab">Current cabinet information.</param>
        /// <param name="cbPrevCab">Number of the previous cabinet file.</param>
        /// <param name="pUserData">User data object.</param>
        /// <returns>Returns True on success, and the name of the new cabinet file is stored in ccab.CabName.
        /// Returns False on error.</returns>
        /// <remarks>The File Compression Interface (FCI) calls this function to get the name of the next cabinet file.
        /// Clients must respond to the NextCabinet event raised by this function and supply the next cabinet file name.</remarks>
        protected virtual bool GetNextCabinet(
            FciCurrentCab ccab,
            int cbPrevCab,
            IntPtr pUserData)
        {
            Trace.WriteLine("GetNextCabinet");
            Trace.WriteLine(string.Format("UserDataPointer = {0}", pUserData));
            Trace.Flush();
            Trace.WriteLine(string.Format("UserData = {0}", GetUserDataObject(pUserData)));
            Trace.Flush();
            NextCabinetEventArgs e = new NextCabinetEventArgs(ccab, cbPrevCab, GetUserDataObject(pUserData));
            Trace.WriteLine("Created event args");
            try
            {
                OnNextCabinet(e);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                e.Result = false;
            }
            return e.Result;
        }

        /// <summary>
        /// Receives notification when a file is placed on the cabinet.
        /// </summary>
        /// <param name="ccab">Current cab file parameters.</param>
        /// <param name="fileName">Name of the file that was placed.</param>
        /// <param name="cbFile">Size of the placed file.</param>
        /// <param name="fContinuation">A flag that indicates whether this file is a continuation from another folder or cabinet.</param>
        /// <param name="pUserData">User data object.</param>
        /// <returns>Returns 0 on success.  Returns -1 on failure.</returns>
        /// <remarks>The File Compression Interface calls this function when a file has been placed in a cabinet.
        /// This is purely a notification message.  Clients that want to be notified of this event must handle the
        /// FilePlaced event raised by this function.</remarks>
        protected virtual int FilePlacedCallback(
            FciCurrentCab ccab,
            string fileName,
            Int32 cbFile,
            bool fContinuation,
            IntPtr pUserData)
        {
            Trace.WriteLine("FilePlacedCallback");
            Trace.WriteLine(string.Format("UserDataPointer = {0}", pUserData));
            Trace.Flush();
            FilePlacedEventArgs e = new FilePlacedEventArgs(ccab, fileName, cbFile, fContinuation,
                GetUserDataObject(pUserData));
            try
            {
                OnFilePlaced(e);
            }
            catch (Exception)
            {
                e.Result = -1;
            }
            return e.Result;
        }

        /// <summary>
        /// Provides progress notification during compression.
        /// </summary>
        /// <param name="typeStatus">An <see cref="FciStatus"/> value that contains the type of notification.</param>
        /// <param name="cb1">First parameter in notification.  Meaning depends on the notification type.</param>
        /// <param name="cb2">Second parameter in notification.  Meaning depends on the notification type.</param>
        /// <param name="pUserData">User data object.</param>
        /// <returns>Returns 0 on success.  Returns -1 on failure.</returns>
        /// <remarks>The File Compression Interface (FCI) calls this function periodically during the compression process.
        /// FCI notifications are provided for cabinets, files, and folders.  Clients that want these notifications must
        /// handle the CabinetComplete, FolderComplete, and FileAdded events.</remarks>
        protected virtual int ProgressFunc(
            FciStatus typeStatus,
            int cb1,
            int cb2,
            IntPtr pUserData)
        {
            Trace.WriteLine(string.Format("ProgressFunc: {0}", typeStatus));
            Trace.WriteLine(string.Format("UserDataPointer = {0}", pUserData));
            Trace.Flush();
            ProgressEventArgs e = new ProgressEventArgs(cb1, cb2, GetUserDataObject(pUserData));
            try
            {
                switch (typeStatus)
                {
                    case FciStatus.Cabinet:
                        OnCabinetComplete(e);
                        break;
                    case FciStatus.File:
                        OnFileAdded(e);
                        break;
                    case FciStatus.Folder:
                        OnFolderComplete(e);
                        break;
                }
            }
            catch (Exception)
            {
                e.Result = -1;
            }
            Trace.WriteLine(string.Format("ProgressFunc returns {0}", e.Result));
            return e.Result;
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the File Compression Interface needs to open a new cabinet file.
        /// </summary>
        /// <remarks>Clients must handle this event and supply the name of a new cabinet file.</remarks>
        public event NextCabinetEventHandler NextCabinet;

        /// <summary>
        /// A notification event that occurs when a file is placed on a cabinet.
        /// </summary>
        public event FilePlacedEventHandler FilePlaced;

        /// <summary>
        /// A notification event that occurs when a FileAdded progress notification is sent by the File Compression Interface (FCI).
        /// </summary>
        public event ProgressEventHandler FileAdded;

        /// <summary>
        /// A notification event that occurs when a FolderComplete progress notification is sent by the File Compression Interface (FCI).
        /// </summary>
        public event ProgressEventHandler FolderComplete;

        /// <summary>
        /// A notification event that occurs when a CabinetComplete progress notification is sent by the File Compression Interface (FCI).
        /// </summary>
        public event ProgressEventHandler CabinetComplete;

        /// <summary>
        /// Raises the <see cref="NextCabinet"/> event.
        /// </summary>
        /// <param name="e">A NextCabinetEventArgs that contains the event data.</param>
        protected virtual void OnNextCabinet(NextCabinetEventArgs e)
        {
            Trace.WriteLine("OnNextCabinet");
            if (NextCabinet != null)
                NextCabinet(this, e);
        }

        /// <summary>
        /// Raises the <see cref="FilePlaced"/> event.
        /// </summary>
        /// <param name="e">A FilePlacedEventArgs that contains the event data.</param>
        protected virtual void OnFilePlaced(FilePlacedEventArgs e)
        {
            Trace.WriteLine("OnFilePlaced");
            if (FilePlaced != null)
                FilePlaced(this, e);
        }

        /// <summary>
        /// Raises the <see cref="FileAdded"/> event.
        /// </summary>
        /// <param name="e">A <see cref="ProgressEventArgs"/> that contains the event data.</param>
        protected virtual void OnFileAdded(ProgressEventArgs e)
        {
            Trace.WriteLine("OnFileAdded");
            if (FileAdded != null)
                FileAdded(this, e);
        }

        /// <summary>
        /// Raises the <see cref="FolderComplete"/> event.
        /// </summary>
        /// <param name="e">A <see cref="ProgressEventArgs"/> that contains the event data.</param>
        protected virtual void OnFolderComplete(ProgressEventArgs e)
        {
            Trace.WriteLine("OnFolderComplete");
            if (FolderComplete != null)
                FolderComplete(this, e);
        }

        /// <summary>
        /// Raises the <see cref="CabinetComplete"/> event.
        /// </summary>
        /// <param name="e">A <see cref="ProgressEventArgs"/> that contains the event data.</param>
        protected virtual void OnCabinetComplete(ProgressEventArgs e)
        {
            Trace.WriteLine("OnCabinetComplete");
            if (CabinetComplete != null)
                CabinetComplete(this, e);
        }

        #endregion
    }
}