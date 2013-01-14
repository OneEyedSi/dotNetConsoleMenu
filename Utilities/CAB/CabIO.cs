//
// $History: CabIO.cs $
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
	/// <summary>
	/// Provides IO helper functions for Cabinet SDK interface
	/// </summary>
	public sealed class CabIO
	{
		private CabIO() {}

        /// <summary>
        /// Allocates a block of memory
        /// </summary>
        /// <param name="cb">The number of bytes to allocate.</param>
        /// <returns>Returns a pointer to the memory block.  On error, it returns a null pointer.</returns>
		public static IntPtr MemAlloc(int cb)
		{
            try
            {
                return Marshal.AllocHGlobal(cb);
            }
            catch (Exception)
            {
                return IntPtr.Zero;
            }
		}

        /// <summary>
        /// Deallocates a block of memory previously allocated with MemAlloc.
        /// </summary>
        /// <param name="memory">Pointer to the memory to be deallocated</param>
	    public static void MemFree(IntPtr memory)
		{
            try
            {
                Marshal.FreeHGlobal(memory);
            }
            catch (Exception)
            {
                // just swallow it
            }
		}

        /// <summary>
        /// Opens a file.
        /// </summary>
        /// <param name="fileName">The name of the file to open.</param>
        /// <param name="oflag">Windows open mode flags.</param>
        /// <param name="pmode">Windows mode flags.</param>
        /// <param name="err">Returned error flag.</param>
        /// <param name="userData">User data passed to the function (not used, but required by FDI/FCI).</param>
        /// <returns>Returns a handle to the open file.  On error, returns -1.</returns>
        /// <remarks>FCI and FDI use Windows semantics to open files.  This function converts the
        /// Windows semantics to .NET, and opens the file accordingly.</remarks>
		public static IntPtr FileOpen(
			string fileName,
			int oflag,
			int pmode,
			ref int err,
			object userData)
		{
			try
			{
				FileAccess fAccess = FCntl.FileAccessFromOFlag(oflag);
				FileMode fMode = FCntl.FileModeFromOFlag(oflag);
				FileShare fShare = FCntl.FileShareFromPMode(pmode);

				return FileOpen(fileName, fAccess, fShare, fMode, ref err);
			}
			catch (Exception)
			{
				return (IntPtr)(-1);
			}
		}

        /// <summary>
        /// Opens a file using .NET semantics.
        /// </summary>
        /// <param name="fileName">Name of the file to open.</param>
        /// <param name="fAccess">FileAccess flags.</param>
        /// <param name="fShare">FileShare flags.</param>
        /// <param name="fMode">FileMode flags.</param>
        /// <param name="err">Returned error flag.</param>
        /// <returns>Returns a handle to the open file.</returns>
	    // Updated 2005-11-11 to allow for decompressing CAB files that contain directories
		public static IntPtr FileOpen(string fileName, FileAccess fAccess,
			FileShare fShare, FileMode fMode, ref int err)
		{
			string dir = Path.GetDirectoryName(fileName);
            // Create the directory if necessary
            if ((fAccess & FileAccess.Write) != 0)
                if (dir != string.Empty && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

            FileStream f = new FileStream(fileName, fMode, fAccess, fShare);
			GCHandle gch = GCHandle.Alloc(f);
			return (IntPtr)gch;
		} 

        // Helper function to return a FileStream from a passed IntPtr
		private static FileStream FileStreamFromHandle(IntPtr hf)
		{
			return (FileStream)((GCHandle)hf).Target;
		}

        /// <summary>
        /// Reads bytes from a file.
        /// </summary>
        /// <param name="hf">The file handle from which to read.</param>
        /// <param name="buffer">The buffer into which bytes will be read.</param>
        /// <param name="cb">The size of the buffer.</param>
        /// <param name="err">Error return.</param>
        /// <param name="userData">User data passed to the function from CAB API.</param>
        /// <returns>Returns the number of bytes read.  Returns 0 on error.</returns>
		public static int FileRead(
			IntPtr hf,
			byte[] buffer,
			int cb,
			ref int err,
			object userData)
		{
			try
			{
				FileStream f = FileStreamFromHandle(hf);
				int bytesRead = f.Read(buffer, 0, cb);
				return bytesRead;
			}
			catch (Exception)
			{
				return 0;
			}
		}

        /// <summary>
        /// Writes bytes to a file.
        /// </summary>
        /// <param name="hf">The file to which will be written.</param>
        /// <param name="buffer">The buffer to write.</param>
        /// <param name="cb">Size of the buffer.</param>
        /// <param name="err">Error return.</param>
        /// <param name="userData">User data passed to the function from CAB API.</param>
        /// <returns>Returns the number of bytes written.  Returns 0 on error.</returns>
		public static int FileWrite(
			IntPtr hf, 
			byte[] buffer, 
			int cb, 
			ref int err,
			object userData)
		{
			try
			{
				FileStream f = FileStreamFromHandle(hf);
				f.Write(buffer, 0, cb);
				return cb;
			}
			catch (Exception)
			{
				return 0;
			}
		}

        /// <summary>
        /// Closes a file.
        /// </summary>
        /// <param name="hf">Handle of the file to be closed.</param>
        /// <param name="err">Error return.</param>
        /// <param name="userData">User data passed to the function from CAB API.</param>
        /// <returns>Returns 0 on success.  Returns -1 on error.</returns>
		public static int FileClose(
			IntPtr hf,
			ref int err,
			object userData)
		{
			// FCI seems to try closing a file that doesn't exist.
			if (hf == (IntPtr)(-1))
				return 0;
			try
			{
				FileStream f = FileStreamFromHandle(hf);
				f.Close();
				return 0;
			}
			catch (Exception)
			{
				return -1;
			}
			finally
			{
				// free the GCHandle for this FileStream
				((GCHandle)(hf)).Free();
			}
		}

        /// <summary>
        /// Moves the current position value in a file
        /// </summary>
        /// <param name="hf">The file handle.</param>
        /// <param name="dist">Number of bytes to move the pointer.</param>
        /// <param name="seektype">A value that specifies how the pointer should be moved.  Valid values are SEEK_CUR, SEEK_END, and SEEK_SET.</param>
        /// <param name="err">Error return.</param>
        /// <param name="userData">User data passed to the function from CAB API.</param>
        /// <returns>Returns the new file position.  Returns -1 on error.</returns>
		public static int FileSeek(
			IntPtr hf,
			int dist,
			int seektype,
			ref int err,
			object userData)
		{
			try
			{
				FileStream f = FileStreamFromHandle(hf);
				System.IO.SeekOrigin origin = FCntl.SeekOriginFromSeekType(seektype);
				// cast to int because FileStream.Seek returns a long
				return (int)f.Seek(dist, origin);
			}
			catch (Exception)
			{
				return -1;
			}
		}

        /// <summary>
        /// Deletes a file.
        /// </summary>
        /// <param name="fileName">The name of the file to delete.</param>
        /// <param name="err">Error return.</param>
        /// <param name="userData">User data passed to the function from CAB API.</param>
        /// <returns>Returns 0 on success.  Returns -1 on error.</returns>
		public static int FileDelete(string fileName, ref int err, object userData)
		{
			try
			{
				File.Delete(fileName);
				return 0;
			}
			catch (Exception)
			{
				return -1;
			}
		}
	}
}
