//
// $History: cabsdk.cs $
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

namespace Utilities.CAB
{
    #region FDI/FCI error structure

    /// <summary>
    /// Error structure returned by FCI/FDI functions.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CabError
    {
        public int erfOper;
        public int erfType;
        public int fError;
        /// <summary>
        /// FDI error code.
        /// </summary>
        /// <remarks>
        /// Test this when error returned from FDI function.
        /// See FdiError enumeration for details.
        /// </remarks>
        public FdiError FdiErrorCode
        {
            get { return (FdiError)erfOper; }
        }

        /// <summary>
        /// FCI error code.
        /// </summary>
        /// <remarks>
        /// Test this when error returned from FCI function.
        /// See FciError enumeration for details.
        /// </remarks>
        public FciError FciErrorCode
        {
            get { return (FciError)erfOper; }
        }

        /// <summary>
        /// Error value filled in by FCI/FDI for some error codes.
        /// </summary>
        public int ErrorType
        {
            get { return erfType; }
            set { erfType = value; }
        }

        /// <summary>
        /// True => error present
        /// </summary>
        public bool HasError
        {
            get { return (fError == 0); }
        }
    }

    #endregion

    #region FdiError enumeration

    /// <summary>
    /// Error codes returned by FDI functions in the CabError.FdiError property.
    /// </summary>
    /// <remarks>
    /// In general, FDI will only fail if one of the passed-in memory or file I/O
    /// functions fails.  Other errors are unlikely and are caused by corrupted
    /// cabinet files, passing in a file which is not a cabinet, or cabinet
    /// files out of order.
    /// </remarks>
    public enum FdiError
    {
        /// <summary>
        /// No Error.
        /// </summary>
        None,
        /// <summary>
        /// The cabinet file was not found.
        /// </summary>
        CabinetNotFound,
        /// <summary>
        /// The referenced file does not have the correct format.
        /// </summary>
        NotACabinet,
        /// <summary>
        /// The cabinet file has an unknown version number.
        /// </summary>
        UnknownCabinetVersion,
        /// <summary>
        /// The cabinet file is corrupt.
        /// </summary>
        CorruptCabinet,
        /// <summary>
        /// Could not allocate memory.
        /// </summary>
        AllocFail,
        /// <summary>
        /// A folder in a cabinet has an unknown compression type.
        /// </summary>
        BadCompressionType,
        /// <summary>
        /// Failure decompressing data from a cabinet file.
        /// </summary>
        MdiFail,
        /// <summary>
        /// Failure writing to the target file.
        /// </summary>
        TargetFile,
        /// <summary>
        /// Cabinets in a set do not have the same reserve sizes.
        /// </summary>
        ReserveMismatch,
        /// <summary>
        /// Cabinet returned from the NextCabinet notification is incorrect.
        /// </summary>
        WrongCabinet,
        /// <summary>
        /// FDI aborted.
        /// </summary>
        UserAbort
    }

    #endregion

    #region FdiCabinetInfo class
    /// <summary>
    /// Information about a cabinet file.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class FdiCabinetInfo
    {
        private int cbCabinet = 0;		// Total length of cabinet file
        private short cFolders = 0;		// Count of folders in cabinet
        private short cFiles = 0;			// Count of files in cabinet
        private short setID = 0;			// Cabinet set ID
        private short iCabinet = 0;		// Cabinet number in set (0 based)
        private bool fReserve = false;   // TRUE => RESERVE present in cabinet
        private bool hasprev = false;    // TRUE => Cabinet is chained prev
        private bool hasnext = false;    // TRUE => Cabinet is chained next

        /// <summary>
        /// Total length of cabinet file.
        /// </summary>
        public int Length
        {
            get { return cbCabinet; }
        }

        /// <summary>
        /// Count of folders in the cabinet file
        /// </summary>
        public short FolderCount
        {
            get { return cFolders; }
        }

        /// <summary>
        /// Count of files in the cabinet file
        /// </summary>
        public short FileCount
        {
            get { return cFiles; }
        }

        /// <summary>
        /// Cabinet set ID
        /// </summary>
        public short SetId
        {
            get { return setID; }
        }

        /// <summary>
        /// Cabinet number in set (0 based).
        /// </summary>
        public short CabinetNumber
        {
            get { return iCabinet; }
        }

        /// <summary>
        /// True if reserve is present in cabinet.
        /// </summary>
        public bool HasReserve
        {
            get { return fReserve; }
        }

        /// <summary>
        /// True if cabinet is chained prev.
        /// </summary>
        public bool HasPrev
        {
            get { return hasprev; }
        }

        /// <summary>
        /// True if cabinet is chained next.
        /// </summary>
        public bool HasNext
        {
            get { return hasnext; }
        }
    }

    #endregion

    #region FDI memory allocation and file I/O delegates

    /// <summary>
    /// Allocate memory and return a pointer that the unmanaged code can use.
    /// Return IntPtr.Zero on error.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr FdiMemAllocDelegate(int numBytes);

    /// <summary>
    /// Free the memory pointed to by the 'mem' parameter.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void FdiMemFreeDelegate(IntPtr mem);

    /// <summary>
    /// Open the file specified by fileName, using the open mode and sharing modes given.
    /// The open and sharing modes use C semantics.
    /// Return a file handle that the other file I/O delegates can use.
    /// Return 0 on error.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr FdiFileOpenDelegate(string fileName, int oflag, int pmode);

    /// <summary>
    /// Read from the file referenced by the hf parameter into the passed array.
    /// The number of bytes to be read is given in the cb parameter.
    /// Return the number of bytes read.  Return 0 on error.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate Int32 FdiFileReadDelegate(IntPtr hf,
[In, Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2, ArraySubType = UnmanagedType.U1)] byte[] buffer, int cb);

    /// <summary>
    /// Write bytes from the passed array to the file referenced by the hf parameter.
    /// The number of bytes to write is given in the cb parameter.
    /// Return the number of bytes written.  Return 0 on error.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate Int32 FdiFileWriteDelegate(IntPtr hf,
[In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2, ArraySubType = UnmanagedType.U1)] byte[] buffer, int cb);

    /// <summary>
    /// Close the file referenced by the hf parameter.
    /// Return 0 on success.  Return -1 on error.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate Int32 FdiFileCloseDelegate(IntPtr hf);

    /// <summary>
    /// Seek to the requested position in the file referenced by hf.
    /// Return new position.  On error, return -1.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate Int32 FdiFileSeekDelegate(IntPtr hf, int dist, int seektype);

    #endregion

    #region FDI Decryption

    /// <summary>
    /// Command types for decrypt callback.
    /// See description of FdiDecryptDelegate for full information.
    /// </summary>
    public enum FdiDecryptType
    {
        /// <summary>
        /// New cabinet.
        /// </summary>
        NewCabinet,
        /// <summary>
        /// New folder
        /// </summary>
        NewFolder,
        /// <summary>
        /// Decrypt a data block.
        /// </summary>
        Decrypt
    }

    /// <summary>
    /// Defines the variant part of the FdiDecrypt class when the DecryptType
    /// property is FdiDecryptType.NewCabinet.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct FdiNewCabinetArgs
    {
        private IntPtr pHeaderReserve;   // RESERVE section from CFHEADER
        private short cbHeaderReserve;  // Size of pHeaderReserve
        private short setID;            // Cabinet set ID
        private int iCabinet;         // Cabinet number in set (0 based)

        /// <summary>
        /// Reserve section from cab file header.
        /// This is a pointer to unmanaged memory.
        /// </summary>
        public IntPtr HeaderReserve
        {
            get { return pHeaderReserve; }
        }

        /// <summary>
        /// Length (in bytes) of the memory block referenced by HeaderReserve.
        /// </summary>
        public short HeaderReserveLength
        {
            get { return cbHeaderReserve; }
        }

        /// <summary>
        /// Cabinet set ID
        /// </summary>
        public short SetId
        {
            get { return setID; }
        }

        /// <summary>
        /// Cabinet number in set (0 based)
        /// </summary>
        public int CabinetNumber
        {
            get { return iCabinet; }
        }
    }

    /// <summary>
    /// Defines the variant part of the FdiDecrypt class when the DecryptType
    /// property is FdiDecryptType.NewFolder.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct FdiNewFolderArgs
    {
        private IntPtr pFolderReserve;   // RESERVE section from CFFOLDER
        private short cbFolderReserve;  // Size of pFolderReserve
        private short iFolder;          // Folder number in cabinet (0 based)

        /// <summary>
        /// Reserve section from folder header.
        /// This is a pointer to unmanaged memory.
        /// </summary>
        public IntPtr FolderReserve
        {
            get { return pFolderReserve; }
        }

        /// <summary>
        /// Length (in bytes) of the memory block referenced by FolderReserve.
        /// </summary>
        public short FolderReserveLength
        {
            get { return cbFolderReserve; }
        }

        /// <summary>
        /// Folder number in cabinet (0 based)
        /// </summary>
        public short FolderNumber
        {
            get { return iFolder; }
        }
    }

    /// <summary>
    /// Defines the variant part of the FdiDecrypt class when the DecryptType
    /// property is FdiDecryptType.NewFolder.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct FdiDecryptArgs
    {
        private IntPtr pDataReserve;   // RESERVE section from CFDATA
        private short cbDataReserve;  // Size of pDataReserve
        private IntPtr pbData;         // Data buffer
        private short cbData;         // Size of data buffer
        private bool fSplit;			// TRUE if this is a split data block
        private short cbPartial;      // 0 if this is not a split block, or
        //  the first piece of a split block;
        // Greater than 0 if this is the
        //  second piece of a split block.

        /// <summary>
        /// Reserve section from data header.
        /// This is pointer to unmanaged memory.
        /// </summary>
        public IntPtr DataReserve
        {
            get { return pDataReserve; }
        }

        /// <summary>
        /// Length (in bytes) of the memory block referenced by DataReserve.
        /// </summary>
        public short DataReserveLength
        {
            get { return cbDataReserve; }
        }

        /// <summary>
        /// Pointer to the data buffer.  This is a pointer to unmanaged memory.
        /// </summary>
        public IntPtr DataBuffer
        {
            get { return pbData; }
        }

        /// <summary>
        /// Size (in bytes) of the data buffer referenced by DataBuffer
        /// </summary>
        public short DataLength
        {
            get { return cbData; }
        }

        /// <summary>
        /// Will be True if this is a split data block.
        /// </summary>
        public bool IsSplit
        {
            get { return fSplit; }
        }

        /// <summary>
        /// 0 if this is not a split block, or is the first part of a split block.
        /// Greater than 0 if this is the second piece of a split block.
        /// </summary>
        public short PartialCount
        {
            get { return cbPartial; }
        }
    }

    /// <summary>
    /// Data passed to Decrypt callback.
    /// </summary>
    /// <remarks>
    /// The DecryptType and Context arguments are available in all cases.
    /// The DecryptType property determines the contents of the class.
    /// If DecryptType == FdiDecryptType.NewCabinet, then access the NewCabinet structure.
    /// If DecryptType == FdiDecryptType.NewFolder, then access the NewFolder structure.
    /// If DecryptType == FdiDecryptType.Decrypt, then access the Decrypt structure.
    /// Unexpected results will occur if you access the wrong structure at the wrong time.
    /// </remarks>
    [StructLayout(LayoutKind.Explicit)]
    public struct FdiDecrypt
    {
        [FieldOffset(0)]
        private FdiDecryptType fdidt;		// Command type (selects union below)
        [FieldOffset(4)]
        private IntPtr pvUser;		// Decryption context

        /// <summary>
        /// Arguments used when DecryptType is NewCabinet.
        /// </summary>
        [FieldOffset(8)]
        public FdiNewCabinetArgs NewCabinet;

        /// <summary>
        /// Arguments used when DecryptType is NewFolder.
        /// </summary>
        [FieldOffset(8)]
        public FdiNewFolderArgs NewFolder;

        /// <summary>
        /// Arguments used when DecryptType is Decrypt.
        /// </summary>
        [FieldOffset(8)]
        public FdiDecryptArgs Decrypt;

        /// <summary>
        /// Decryption callback message type.
        /// </summary>
        /// <remarks>
        /// The value of this field determines which of the Args records should be
        /// used to access the data in this structure.
        /// </remarks>
        public FdiDecryptType DecryptType
        {
            get { return fdidt; }
        }

        /// <summary>
        /// Decryption context passed to FdiCopy.
        /// </summary>
        public object UserData
        {
            get { return (object)((GCHandle)pvUser).Target; }
        }
    }

    /// <summary>
    /// The CAB SDK documentation indicates that decryption is not supported.
    /// The callback seems to be called, but you use at your own risk.
    /// There doesn't appear to be any support for encryption in the FCI stuff.
    /// See the FDI.H header file for details.
    /// Return 1 on success, -1 on error.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int FdiDecryptDelegate(ref FdiDecrypt fdid);

    #endregion

    #region FDI Notification

    /// <summary>
    /// Structure passed to FdiNotification callback.
    /// See Cab SDK documentation (especially FDI.H) for full details.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class FdiNotification
    {
        private int cb;
        private string psz1;
        private string psz2;
        // TODO: test this with fdintNEXT_CABINET.  psz3 can be modified by the client.
        private string psz3;               // Points to a 256 character buffer.
        private IntPtr userData;			// Value for client

        private int hf;

        private short date;
        private short time;
        private short attribs;

        private short setID;
        private short iCabinet;      // Cabinet number (0-based)
        private short iFolder;       // Folder number (0-based)
        private FdiError fdie;

        public int Size
        {
            get { return cb; }
        }

        /// <summary>
        /// Current file position in cabinet.
        /// </summary>
        public int FilePosition
        {
            // The Enumerate callback notification overloads
            // cb to be the current file position.
            get { return cb; }
            set { cb = value; }
        }

        public bool RunAfterExtract
        {
            // The CloseFile callback notification overloads cb
            // to be a "Run after extract" flag.
            get { return cb == 1; }
        }

        public string str1
        {
            get { return psz1; }
        }

        public string str2
        {
            get { return psz2; }
        }

        public string str3
        {
            get { return psz3; }
        }

        public string CabinetPathName
        {
            get { return psz3; }
            set { psz3 = value; }
        }

        /// <summary>
        /// The user's context object returned as System.Object.
        /// </summary>
        public object UserData
        {
            get { return (object)((GCHandle)userData).Target; }
        }

        /// <summary>
        /// File handle to close.  Used by FileClose notification.
        /// </summary>
        public IntPtr FileHandle
        {
            get { return (IntPtr)hf; }
        }

        public short FileDate
        {
            get { return date; }
        }

        public short FileTime
        {
            get { return time; }
        }

        public short FileAttributes
        {
            get { return attribs; }
        }

        public short SetId
        {
            get { return setID; }
        }

        public short CabinetNumber
        {
            get { return iCabinet; }
        }

        public short FolderNumber
        {
            get { return iFolder; }
        }

        public short NumFilesRemaining
        {
            // Enumerate callback overloads iFolder.
            get { return iFolder; }
            set { iFolder = value; }
        }

        public FdiError ErrorCode
        {
            get { return fdie; }
        }
    }

    /// <summary>
    /// FDI notification types.
    /// </summary>
    public enum FdiNotificationType
    {
        CabinetInfo,        // General information about cabinet
        PartialFile,        // First file in cabinet is continuation
        CopyFile,           // File to be copied
        CloseFileInfo,      // close the file, set relevant info
        NextCabinet,        // File continued to next cabinet
        Enumerate           // Enumeration status
    }

    /// <summary>
    /// Notification callback.  fdint tells which notification is being given.
    /// fdin is the notification structure that can be modified by the client.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int FdiNotifyDelegate(FdiNotificationType fdint, FdiNotification fdin);

    #endregion

    #region FciError Enumeration
    public enum FciError
    {
        /// <summary>
        /// No error.
        /// </summary>
        None,
        /// <summary>
        /// Failure opening file to be stored in cabinet
        /// </summary>
        OpenSrc,
        /// <summary>
        /// Failure reading file to be stored in cabinet
        /// </summary>
        ReadSrc,
        /// <summary>
        /// Unable to allocate memory
        /// </summary>
        AllocFail,
        /// <summary>
        /// Could not create a temporary file
        /// </summary>
        TempFile,
        /// <summary>
        /// Unknown compression type requested
        /// </summary>
        BadCompressionType,
        /// <summary>
        /// Unable to create cabinet file
        /// </summary>
        CabFile,
        /// <summary>
        /// Client requested abort
        /// </summary>
        UserAbort,
        /// <summary>
        /// Failure compressing data
        /// </summary>
        MciFail
    }

    #endregion

    #region FciCompression Enumeration
    /// <summary>
    /// Compression type flags.
    /// </summary>
    /// <remarks>
    /// These are passed to FCIAddFile(), and are also stored in the CFFOLDER
    /// structures in cabinet files.
    ///
    /// NOTE: We reserve bits for the TYPE, QUANTUM_LEVEL, and QUANTUM_MEM
    /// to provide room for future expansion.  Since this value is stored
    /// in the CFDATA records in the cabinet file, we don't want to
    /// have to change the format for existing compression configurations
    /// if we add new ones in the future.  This will allow us to read
    /// old cabinet files in the future.
    /// </remarks>
    [Flags]
    public enum FciCompression
    {
        MaskType = 0x000f,			// Mask for compression type
        None = 0x0000,				// No compression
        MsZip = 0x0001,				// MSZIP
        Quantum = 0x0002,			// Quantum
        Lzx = 0x0003,				// LZX
        Bad = 0x000f,				// Unspecified compression type

        MaskLzxWindow = 0x1f00,		// Mask for LZX Compression Memory
        LzxWindowLo = 0x0f00,		// Lowest LZX Memory (15)
        LzxWindowHi = 0x1500,		// Highest LZX Memory (21)
        ShiftLzxWindow = 8,			// Amount to shift over to get int

        MaskQuantumLevel = 0x00f0,	// Mask for Quantum Compression Level
        QuantumLevelLo = 0x0010,	// Lowest Quantum Level (1)
        QuantumLevelHi = 0x0070,	// Highest Quantum Level (7)
        ShiftQuantumLevel = 4,		// Amount to shift over to get int

        MaskQuantumMem = 0x1f00,	// Mask for Quantum Compression Memory
        QuantumMemLo = 0x0a00,		// Lowest Quantum Memory (10)
        QuantumMemHi = 0x1500,		// Highest Quantum Memory (21)
        ShiftQuantumMem = 8,		// Amount to shift over to get int

        MaskReserved = 0xe000		// Reserved bits (3)
    }
    #endregion

    #region FciCurrentCab class
    /// <summary>
    /// CurrentCab structure used to pass cabinet creation parameters to FciCreate.
    /// </summary>
    /// <remarks>
    /// This structure also is passed to the FciGetNextCabinet delegate to provide 
    /// cabinet information to the client program.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class FciCurrentCab
    {
        private int cb;                     // size available for cabinet on this media
        private int cbFolderThresh;         // Thresshold for forcing a new Folder

        private int cbReserveCFHeader;      // Space to reserve in CFHEADER
        private int cbReserveCFFolder;      // Space to reserve in CFFOLDER
        private int cbReserveCFData;        // Space to reserve in CFDATA
        private int iCab;                   // sequential numbers for cabinets
        private int iDisk;                  // Disk number
        // This field is not documented in the SDK
        private int fFailOnIncompressible;  // TRUE => Fail if a block is incompressible

        /// <summary>
        /// Cabinet set ID
        /// </summary>
        private short setID;

        /// <summary>
        /// Current disk name
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        private string szDisk;

        /// <summary>
        /// Current cabinet name
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        private string szCab;

        /// <summary>
        /// path for creating cabinet
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        private string szCabPath;

        public int MaxCabinetSize
        {
            get { return cb; }
            set { cb = value; }
        }

        public int MaxFolderSize
        {
            get { return cbFolderThresh; }
            set { cbFolderThresh = value; }
        }

        public int HeaderReserve
        {
            get { return cbReserveCFHeader; }
            set { cbReserveCFHeader = value; }
        }

        public int FolderReserve
        {
            get { return cbReserveCFFolder; }
            set { cbReserveCFFolder = value; }
        }

        public int DataReserve
        {
            get { return cbReserveCFData; }
            set { cbReserveCFData = value; }
        }

        public int CabinetNumber
        {
            get { return iCab; }
            set { iCab = value; }
        }

        public int DiskNumber
        {
            get { return iDisk; }
            set { iDisk = value; }
        }

        public short SetId
        {
            get { return setID; }
            set { setID = value; }
        }

        public string DiskName
        {
            get { return szDisk; }
            set { szDisk = value; }
        }

        public string CabName
        {
            get { return szCab; }
            set { szCab = value; }
        }

        public string CabPath
        {
            get { return szCabPath; }
            set { szCabPath = value; }
        }
    }

    #endregion

    #region FCI Memory allocation and file I/O delegates

    /// <summary>
    /// Allocate memory and return a pointer that the unmanaged code can use.
    /// Return IntPtr.Zero on error.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr FciMemAllocDelegate(int cb);

    /// <summary>
    /// Free the memory pointed to by the 'mem' parameter.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void FciMemFreeDelegate(IntPtr memory);

    /// <summary>
    /// Open the file specified by fileName, using the open mode and sharing modes given.
    /// The open and sharing modes use C semantics.
    /// Return a file handle that the other file I/O delegates can use.
    /// Return 0 on error, and set the err value to a meaningful value for your application.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr FciFileOpenDelegate(
string fileName,
int oflag,
int pmode,
ref int err,
IntPtr userData);

    /// <summary>
    /// Read from the file referenced by the hf parameter into the passed array.
    /// The number of bytes to be read is given in the cb parameter.
    /// Return the number of bytes read.  Return 0 on error and set the err value.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int FciFileReadDelegate(
IntPtr hf,
[In, Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2, ArraySubType = UnmanagedType.U1)] byte[] buffer,
int cb,
ref int err,
IntPtr userData);

    /// <summary>
    /// Write bytes from the passed array to the file referenced by the hf parameter.
    /// The number of bytes to write is given in the cb parameter.
    /// Return the number of bytes written.  Return 0 on error and set the err value.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int FciFileWriteDelegate(
IntPtr hf,
[In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2, ArraySubType = UnmanagedType.U1)] byte[] buffer,
int cb,
ref int err,
IntPtr userData);

    /// <summary>
    /// Close the file referenced by the hf parameter.
    /// Return 0 on success.  Return -1 on error and set the err value.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int FciFileCloseDelegate(
IntPtr hf,
ref int err,
IntPtr userData);

    /// <summary>
    /// Seek to the requested position in the file referenced by hf.
    /// Return new position.  On error, return -1 and set the err value.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int FciFileSeekDelegate(
IntPtr hf,
int dist,
int seektype,
ref int err,
IntPtr userData);

    /// <summary>
    /// Delete the file passed in the fileName parameter.
    /// Return 0 on success.  On error, return non-zero and set the err value.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int FciFileDeleteDelegate(
string fileName,
ref int err,
IntPtr userData);

    #endregion

    #region FCI operation delegates

    /// <summary>
    /// Get the name and other information about the next cabinet.
    /// ccab is a reference to the FciCurrentCab structure to modify.
    /// cbPrevCab is an estimate of the size of the previous cabinet.
    /// At minimum, the function should change the ccab.CabName value.
    /// The CurrentCab value in the structure will have been updated by FCI.
    /// Return true on success.  Return false on failure.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool FciGetNextCabinetDelegate(
[In, Out][MarshalAs(UnmanagedType.LPStruct)] FciCurrentCab ccab,
int cbPrevCab,
IntPtr userData);

    /// <summary>
    /// Called when FCI places a file in a cabinet.
    /// This is a notification only, and the client should not modify the ccab structure.
    /// ccab is a reference to the cabinet parameters structure.
    /// fileName is the name of the file that was placed.
    /// cbFile is the length of the file in bytes.
    /// fContinuation is true if this is the later segment of a continued file.
    /// Return 0 on success.  Return -1 on error.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int FciFilePlacedDelegate(
[In, Out][MarshalAs(UnmanagedType.LPStruct)] FciCurrentCab ccab,
string fileName,
Int32 cbFile,
bool fContinuation,
IntPtr userData);

    /// <summary>
    /// Open a file and return information about it.
    /// The file to be opened is specified by the fileName parameter
    /// rDate and rTime must be set to the file's last access time.  These values are
    /// DOS file date/time values.
    /// Set attribs to the file's attributes.  This uses C-format file attributes.
    /// On success, return the file handle.
    /// On error, return -1 and set the err value.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr FciGetOpenInfoDelegate(
string fileName,
ref short rDate,
ref short rTime,
ref short attribs,
ref int err,
IntPtr userData);

    /// <summary>
    /// Status values passed to the status callback.
    /// </summary>
    public enum FciStatus
    {
        /// <summary>
        /// File added to Folder
        /// </summary>
        File = 0,
        /// <summary>
        /// File added to Cabinet
        /// </summary>
        Folder = 1,
        /// <summary>
        /// Cabinet completed
        /// </summary>
        Cabinet = 2
    }

    /// <summary>
    /// Status notification callback.  There are three different status values
    /// as defined by the FciStatus enumeration.  The meanings of cb1 and cb2
    /// differ depending on the status value.  See FCI.H in the Cabinet SDK
    /// for full information.
    /// 
    /// Return 0 on success, but check FCI.H for information about special return values
    /// when typeStatus == Cabinet.
    /// Return -1 on error.  FCI will abort.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int FciStatusDelegate(
FciStatus typeStatus,
int cb1,
int cb2,
IntPtr userData);

    // Ideally, the tempName parameter would be a StringBuilder.
    // However, there seems to be a bug in the runtime that makes all StringBuilder
    // objects have a MaxCapacity of 16.
    // http://support.microsoft.com/?kbid=317577
    // So use an IntPtr and get hands dirty fiddling with bytes.
    //
    // That bug supposedly is fixed in .NET Framework 1.1 SP1, but StringBuilder still
    // doesn't work here.
    /// <summary>
    /// Get the name of a temporary file and return it in the buffer pointed to
    /// by tempName.  The length of the buffer is passed in cbTempName.
    /// The file must not exist when this function returns.
    /// Return true on success.  Return false on error.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool FciGetTempFileDelegate(
        //        [MarshalAs(UnmanagedType.LPStr, SizeConst=256)] StringBuilder tempName,
IntPtr tempName,
int cbTempName,
IntPtr userData);

    #endregion

    public sealed class CabSdk
    {
        // prevents instantiation
        private CabSdk() { }

        #region Common constants and helper functions

        // Constants
        public const int CB_MAX_CHUNK = 32768;
        public const int CB_MAX_DISK = 0x7ffffff;
        public const int CB_MAX_FILENAME = 256;
        public const int CB_MAX_CABINET_NAME = 256;
        public const int CB_MAX_CAB_PATH = 256;
        public const int CB_MAX_DISK_NAME = 256;

        /// <summary>
        /// Gets the compression type (0-15) from the FciCompression value.
        /// </summary>
        /// <param name="tc">FciCompression flags.</param>
        /// <returns>The compression type (0-15)</returns>
        public static Int16 GetCompressionType(FciCompression tc)
        {
            return unchecked((short)(tc & FciCompression.MaskType));
        }

        /// <summary>
        /// Gets the quantum compression level (1-7) from the FciCompression value.
        /// </summary>
        /// <param name="tc">FciCompression flags.</param>
        /// <returns>The quantum compression level (1-7).</returns>
        public static Int16 GetQuantumLevel(FciCompression tc)
        {
            return unchecked((short)((int)(tc & FciCompression.MaskQuantumLevel) >>
                (int)FciCompression.ShiftQuantumLevel));
        }

        /// <summary>
        /// Gets the memory level (10-21) from the FciCompression value.
        /// </summary>
        /// <param name="tc">FciCompression flags.</param>
        /// <returns>The memory level (10-21).</returns>
        public static Int16 GetQuantumMemoryLevel(FciCompression tc)
        {
            return (unchecked((short)((int)(tc & FciCompression.MaskQuantumMem) >>
                (int)FciCompression.ShiftQuantumMem)));
        }

        /// <summary>
        /// Gets the LZX compression window size (15-21) from the FciCompression value.
        /// </summary>
        /// <param name="tc">FciCompression flags.</param>
        /// <returns>The compression window size (15-21)</returns>
        public static Int16 GetLzxCompressionWindow(FciCompression tc)
        {
            return (unchecked((short)((int)(tc & FciCompression.MaskLzxWindow) >>
                (int)FciCompression.ShiftLzxWindow)));
        }

        /// <summary>
        /// Builds an FciCompression flags value from type, level, and memory values.
        /// </summary>
        /// <param name="type">Compression type(0-15)</param>
        /// <param name="level">Compression level (1-7)</param>
        /// <param name="memory">Compression memory (10-21)</param>
        /// <returns>A fully-constructed FciCompression value that contains type,
        /// level, and memory values.</returns>
        public static FciCompression FciCompressionFromTypeLevelMemory(
            int type, int level, int memory)
        {
            int fcic =
                (memory << (int)FciCompression.ShiftQuantumMem) |
                (level << (int)FciCompression.ShiftQuantumLevel) |
                (type);
            return (FciCompression)fcic;
        }

        /// <summary>
        /// Builds an FciCompression flags value for LZX compression with the
        /// specified window size.
        /// </summary>
        /// <param name="window">Desired LZX window size.</param>
        /// <returns>The fully-constructed FciCompression value.</returns>
        public static FciCompression FciCompressionFromLzxWindow(int window)
        {
            int fcic = (window << (int)FciCompression.ShiftLzxWindow) |
                (int)FciCompression.Lzx;
            return (FciCompression)fcic;
        }

        /// <summary>
        /// FAT file attribute flag used by FCI/FDI to indicate that
        /// the filename in the CAB is a UTF string 
        /// </summary>
        public const int _A_NAME_IS_UTF = 0x80;

        /// <summary>
        /// FAT file attribute flag used by FCI/FDI to indicate that
        /// the file should be executed after extraction.  
        /// </summary>
        public const int _A_EXEC = 0x40;

        #endregion

        #region FDI - File Decompression Interface.

        /// <summary>
        /// Create an FDI context.
        /// </summary>
        /// <remarks>
        /// This is the managed prototype to the unmanaged FDICreate function.
        /// It is marked private to force users to go through the managed wrapper.
        /// </remarks>
        [DllImport("cabinet.dll",
             CallingConvention = CallingConvention.Cdecl,
             EntryPoint = "FDICreate")]
        private static extern IntPtr FdiCreate(
            FdiMemAllocDelegate fnMemAlloc,
            FdiMemFreeDelegate fnMemFree,
            FdiFileOpenDelegate fnFileOpen,
            FdiFileReadDelegate fnFileRead,
            FdiFileWriteDelegate fnFileWrite,
            FdiFileCloseDelegate fnFileClose,
            FdiFileSeekDelegate fnFileSeek,
            int cpuType,    // ignored by 32-bit FDI
            ref CabError erf);

        /// <summary>
        /// "unknown cpu type" flag to cause FDI to do its own detection.
        /// </summary>
        /// <remarks>
        /// This is used as the cpuType parameter to FdiCreate().
        /// The cpuType parameter is ignored in the 32-bit version of CAB.
        /// </remarks>
        private const int cpuTypeUnknown = -1;

        /// <summary>
        /// Create an FDI context.
        /// </summary>
        /// <param name="fnMemAlloc">Memory allocation delegate.</param>
        /// <param name="fnMemFree">Memory free delegate.</param>
        /// <param name="fnFileOpen">File open delegate.</param>
        /// <param name="fnFileRead">File read delegate.</param>
        /// <param name="fnFileWrite">File write delegate.</param>
        /// <param name="fnFileClose">File close delegate.</param>
        /// <param name="fnFileSeek">File seek delegate.</param>
        /// <param name="erf">Error structure that will be filled in with error information.</param>
        /// <returns>On success, returns a non-null FDI context handle.
        /// If an error occurs, the return value will be IntPtr.Zero,
        /// and the passed CabError structure will contain error information.</returns>
        public static IntPtr FdiCreate(
            FdiMemAllocDelegate fnMemAlloc,
            FdiMemFreeDelegate fnMemFree,
            FdiFileOpenDelegate fnFileOpen,
            FdiFileReadDelegate fnFileRead,
            FdiFileWriteDelegate fnFileWrite,
            FdiFileCloseDelegate fnFileClose,
            FdiFileSeekDelegate fnFileSeek,
            ref CabError erf)
        {
            return FdiCreate(fnMemAlloc, fnMemFree, fnFileOpen, fnFileRead, fnFileWrite,
                fnFileClose, fnFileSeek, cpuTypeUnknown, ref erf);
        }

        /// <summary>
        /// Determines if a file is a cabinet, and returns information about the cabinet
        /// if so.
        /// </summary>
        /// <param name="hfdi">FDI context created by FdiCreate.</param>
        /// <param name="hf">File handle compatible with Read and Seek delegates passed
        /// to FdiCreate.  The file should be positioned at offset 0 in the file to test.</param>
        /// <param name="cabInfo">Structure to receive information about the cabinet file.</param>
        /// <returns>Returns true if the file appears to be a valid cabinet.  Information
        /// about the file is placed in the passed cabInfo structure.
        /// Returns false if the file is not a cabinet.</returns>
        [DllImport("cabinet.dll",
             CallingConvention = CallingConvention.Cdecl,
             EntryPoint = "FDIIsCabinet")]
        public static extern bool FdiIsCabinet(
            IntPtr hfdi,
            IntPtr hf,
            FdiCabinetInfo cabInfo);

        /// <summary>
        /// Extract files from a cabinet.
        /// </summary>
        /// <remarks>
        /// This is the managed prototype to the unmanaged FDICopy function.
        /// It is marked private to force users to go through the managed wrapper.
        /// </remarks>
        [DllImport("cabinet.dll",
             CallingConvention = CallingConvention.Cdecl,
             EntryPoint = "FDICopy")]
        private static extern bool FdiCopy(
            IntPtr hfdi,
            string cabinetName,
            string cabinetPath,
            int flags,
            FdiNotifyDelegate fnNotify,
            FdiDecryptDelegate fnDecrypt,
            IntPtr userData);

        /// <summary>
        /// Extract files from a cabinet.
        /// </summary>
        /// <param name="hfdi">Handle to FDI context created by FdiCreate.</param>
        /// <param name="cabinetName">Name and extension of cabinet file.</param>
        /// <param name="cabinetPath">Path that contains cabinet files.</param>
        /// <param name="fnNotify">Notification delegate.  Must not be null.</param>
        /// <param name="fnDecrypt">Decryption delegate.  Pass null if not used.</param>
        /// <param name="userData">User specified data to pass to notification and decryption
        /// functions.  May be null if not used.</param>
        /// <returns>Returns True if successful.
        /// Returns False if an error occurs.  The CabError structure that was passed to the
        /// FdiCreate call will contain error information.</returns>
        public static bool FdiCopy(
            IntPtr hfdi,
            string cabinetName,
            string cabinetPath,
            FdiNotifyDelegate fnNotify,
            FdiDecryptDelegate fnDecrypt,
            object userData)
        {
            // create a GCHandle for the user data
            GCHandle gch = GCHandle.Alloc(userData);
            try
            {
                // Ensure that the cabinet path has a trailing directory separator.
                string cabPath;
                if (cabinetPath == string.Empty || cabinetPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    cabPath = cabinetPath;
                else
                    cabPath = cabinetPath + Path.DirectorySeparatorChar;

                // call FdiCopy
                bool rslt = FdiCopy(hfdi, cabinetName, cabPath, 0, fnNotify, fnDecrypt, (IntPtr)gch);
                // Prevent the garbage collector from cleaning up the cabPath
                // string until FdiCopy returns.
                GC.KeepAlive(cabPath);
                return rslt;
            }
            finally
            {
                gch.Free();
            }
        }

        /// <summary>
        /// Destroy an FDI context that was created by FdiCreate.
        /// </summary>
        /// <param name="hfdi">The FDI context to destroy.</param>
        /// <returns>Returns True if successful.  Returns False if unsuccessful.</returns>
        [DllImport("cabinet.dll",
             CallingConvention = CallingConvention.Cdecl,
             EntryPoint = "FDIDestroy")]
        public static extern bool FdiDestroy(IntPtr hfdi);

        #endregion

        #region FCI - File Compression Interface

        /// <summary>
        /// Create a compression context.  Opens a new CAB file and prepares it to
        /// accept files.
        /// </summary>
        /// <param name="erf">Error structure</param>
        /// <param name="fnFilePlaced">Callback for file placement notifications.</param>
        /// <param name="fnMemAlloc">Memory allocation callback.</param>
        /// <param name="fnMemFree">Memory free callback.</param>
        /// <param name="fnFileOpen">File open callback.</param>
        /// <param name="fnFileRead">File read callback.</param>
        /// <param name="fnFileWrite">File write callback.</param>
        /// <param name="fnFileClose">File close callback.</param>
        /// <param name="fnFileSeek">File seek callback.</param>
        /// <param name="fnFileDelete">File delete callback.</param>
        /// <param name="fnTempFile">Callback to return temporary file name.</param>
        /// <param name="ccab">Reference to cabinet file parameters</param>
        /// <param name="userData">User's context pointer</param>
        /// <returns>On success, returns a non-null handle to an FCI context.
        /// On error, the return value will be IntPtr.Zero and the error structure
        /// will have error information.</returns>
        [DllImport("cabinet.dll",
             CallingConvention = CallingConvention.Cdecl,
             EntryPoint = "FCICreate")]
        public static extern IntPtr FciCreate(/*[In, Out][MarshalAs(UnmanagedType.LPStruct)]*/ ref CabError erf,
            FciFilePlacedDelegate fnFilePlaced,
            FciMemAllocDelegate fnMemAlloc,
            FciMemFreeDelegate fnMemFree,
            FciFileOpenDelegate fnFileOpen,
            FciFileReadDelegate fnFileRead,
            FciFileWriteDelegate fnFileWrite,
            FciFileCloseDelegate fnFileClose,
            FciFileSeekDelegate fnFileSeek,
            FciFileDeleteDelegate fnFileDelete,
            FciGetTempFileDelegate fnTempFile,
            [In, Out][MarshalAs(UnmanagedType.LPStruct)] FciCurrentCab ccab,
            IntPtr userData);

        /// <summary>
        /// Add a disk file to a cabinet.
        /// </summary>
        /// <remarks>
        /// This is the managed prototype to the unmanaged FCIAddFile function.
        /// It is marked private to force clients to go through the managed wrapper.
        /// </remarks>
        [DllImport("cabinet.dll",
             CallingConvention = CallingConvention.Cdecl,
             EntryPoint = "FCIAddFile")]
        private static extern bool FciAddFile(
            IntPtr hfci,
            string sourceFileName,
            string fileNameInCabinet,
            bool fExecute,
            FciGetNextCabinetDelegate fnGetNextCab,
            FciStatusDelegate fnStatus,
            FciGetOpenInfoDelegate fnGetOpenInfo,
            short typeCompress);

        /// <summary>
        /// Add a disk file to a cabinet.
        /// </summary>
        /// <param name="hfci">Handle to FCI context returned by FciCreate.</param>
        /// <param name="sourceFileName">Full path and name of the file to add.</param>
        /// <param name="fileNameInCabinet">Name to use when storing in the cabinet.</param>
        /// <param name="fExecute">True if the file should be marked to execute on extraction.</param>
        /// <param name="fnGetNextCab">GetNextCab callback.</param>
        /// <param name="fnStatus">Status callback.</param>
        /// <param name="fnGetOpenInfo">OpenInfo callback.</param>
        /// <param name="typeCompress">Type of compression desired.</param>
        /// <returns>Returns true on success.  Returns false on failure.
        /// In the event of failure, the CabError structure passed to the FciCreate function
        /// that created the current compression context will contain error information.</returns>
        public static bool FciAddFile(
            IntPtr hfci,
            string sourceFileName,
            string fileNameInCabinet,
            bool fExecute,
            FciGetNextCabinetDelegate fnGetNextCab,
            FciStatusDelegate fnStatus,
            FciGetOpenInfoDelegate fnGetOpenInfo,
            FciCompression typeCompress)
        {
            // Calls the unmanaged prototype after converting the FciCompression value
            // to a short.
            return FciAddFile(hfci, sourceFileName, fileNameInCabinet, fExecute,
                fnGetNextCab, fnStatus, fnGetOpenInfo, unchecked((short)typeCompress));
        }

        /// <summary>
        /// Completes the current cabinet under construction, gathering all of the pieces
        /// and writing them to the cabinet file.
        /// </summary>
        /// <param name="hfci">Handle to FCI context returned by FciCreate.</param>
        /// <param name="fGetNextCab">If set to true, forces creation of a new cabinet after this one is closed.
        /// If false, only creates a new cabinet if the current cabinet overflows.</param>
        /// <param name="fnGetNextCab">Callback function to get continuation cabinet information.</param>
        /// <param name="fnStatus">Status callback.</param>
        /// <returns>Returns true on success.  Returns false on failure, and the CabError structure
        /// passed to FciCreate is filled with error information.</returns>
        /// <remarks>Flushing the cabinet causes a folder flush as well.</remarks>
        [DllImport("cabinet.dll",
             CallingConvention = CallingConvention.Cdecl,
             EntryPoint = "FCIFlushCabinet")]
        public static extern bool FciFlushCabinet(
            IntPtr hfci,
            bool fGetNextCab,
            FciGetNextCabinetDelegate fnGetNextCab,
            FciStatusDelegate fnStatus);

        /// <summary>
        /// Forces completion of the current cabinet file folder.
        /// </summary>
        /// <param name="hfci">FCI context handle.</param>
        /// <param name="fnGetNextCab">Callback function to get continuation cabinet information.</param>
        /// <param name="fnStatus">Status callback.</param>
        /// <returns>Returns true on success.  Returns false on failure, and the CabError structure
        /// passed to FciCreate is filled with error information.</returns>
        /// <remarks>
        /// This could cause the current cabinet to be flushed, too.
        /// </remarks>
        [DllImport("cabinet.dll",
             CallingConvention = CallingConvention.Cdecl,
             EntryPoint = "FCIFlushFolder")]
        public static extern bool FciFlushFolder(
            IntPtr hfci,
            FciGetNextCabinetDelegate fnGetNextCab,
            FciStatusDelegate fnStatus);

        /// <summary>
        /// Destroy an FCI context and delete temporary files.
        /// </summary>
        /// <param name="hfci">Handle to FCI context.</param>
        /// <returns>Returns true if successful.  If unsuccessful, returns false and the
        /// CabError structure passed to FciCreate is filled with error information.</returns>
        /// <remarks>
        /// If this function fails, temporary files could be left behind.
        /// </remarks>
        [DllImport("cabinet.dll",
             CallingConvention = CallingConvention.Cdecl,
             EntryPoint = "FCIDestroy")]
        public static extern bool FciDestroy(IntPtr hfci);

        #endregion
    }
}
