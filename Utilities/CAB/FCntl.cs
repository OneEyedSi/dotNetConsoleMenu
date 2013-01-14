//
// $History: FCntl.cs $
// 
// *****************  Version 1  *****************
// User: Brentfo      Date: 9/08/11    Time: 10:20a
// Created in $/UtilitiesClassLibrary/Utilities/CAB
// Added CAB library to Utilities library for extracting and creating CAB
// files.
// 

using System;
using System.IO;

namespace Utilities.CAB
{
	/// <summary>
	/// Provides constants and helper functions to convert between C standard library
	/// file attributes and .NET file attributes.
	/// </summary>
	public sealed class FCntl
	{
		// private constructor prevents instantiation
		private FCntl() {}

		// File open modes
		public const int O_RDONLY = 0x0000;
		public const int O_WRONLY = 0x0001;
		public const int O_RDWR   = 0x0002;
		public const int O_APPEND = 0x0008;
		public const int O_CREAT  = 0x0100;
		public const int O_TRUNC  = 0x0200;
		public const int O_EXCL   = 0x0400;
		public const int O_TEXT   = 0x4000;
		public const int O_BINARY = 0x8000;

		// File attributes
		public const int A_NORMAL = 0x00;    // Normal file - No read/write restrictions
		public const int A_RDONLY = 0x01;    // Read only file
		public const int A_HIDDEN = 0x02;    // Hidden file
		public const int A_SYSTEM = 0x04;    // System file
		public const int A_SUBDIR = 0x10;    // Subdirectory
		public const int A_ARCH   = 0x20;    // Archive file

		// File open modes
		public const int S_IREAD  = 0x0100;
		public const int S_IWRITE = 0x0080;

		// File access types and such
		public const int O_TEMPORARY    = 0x0040;

		public const int O_SHORT_LIVED  = 0x1000;
		public const int O_SEQUENTIAL   = 0x0020;
		public const int O_RANDOM       = 0x0010;

		// Seek types
		public const int SEEK_CUR   = 1;
		public const int SEEK_END   = 2;
		public const int SEEK_SET   = 0;

        /// <summary>
        /// Returns a .NET FileAccess value from the passed Windows file access flags
        /// </summary>
        /// <param name="oflag">The Windows file access flags.</param>
        /// <returns>The FileAccess value that corresponds to the passed Windows flags.</returns>
		public static FileAccess FileAccessFromOFlag(int oflag)
		{
			FileAccess fAccess = FileAccess.Read;

			// Translate access and sharing flags into .NET equivalents.
			switch (oflag & (FCntl.O_RDONLY | FCntl.O_WRONLY | FCntl.O_RDWR))
			{
				case FCntl.O_RDONLY:
					fAccess = FileAccess.Read;
					break;
				case FCntl.O_WRONLY:
					fAccess = FileAccess.Write;
					break;
				case FCntl.O_RDWR:
					fAccess = FileAccess.ReadWrite;
					break;
			}
			return fAccess;
		}

        /// <summary>
        /// Returns a .NET FileShare value from the passed Windows file share flags
        /// </summary>
        /// <param name="pmode">The Windows file share flags.</param>
        /// <returns>The FileShare value that corresponds to the passed Windows flags.</returns>
		public static FileShare FileShareFromPMode(int pmode)
		{
			FileShare fShare = FileShare.None;

			// decode sharing flags
			if ((pmode & FCntl.S_IREAD) != 0)
				fShare |= FileShare.Read;
			if ((pmode & FCntl.S_IWRITE) != 0)
				fShare |= FileShare.Write;

			return fShare;
		}

        /// <summary>
        /// Returns a .NET FileMode value from the passed Windows file mode flags.
        /// </summary>
        /// <param name="oflag">The Windows file mode flags</param>
        /// <returns>The FileMode value that corresponds to the passed Windows flags.</returns>
		public static FileMode FileModeFromOFlag(int oflag)
		{
			FileMode fMode;

			// creation mode flags
			if ((oflag & FCntl.O_CREAT) != 0)
			{
				if ((oflag & FCntl.O_EXCL) != 0)
					fMode = FileMode.CreateNew;
				else if ((oflag & FCntl.O_TRUNC) != 0)
					fMode = FileMode.Create;
				else
					fMode = FileMode.OpenOrCreate;
			}
			else if ((oflag & FCntl.O_TRUNC) != 0)
				fMode = FileMode.Truncate;
			else if ((oflag & FCntl.O_EXCL) != 0)
				fMode = FileMode.Open;
			else
				fMode = FileMode.Open;

			return fMode;
		}

        /// <summary>
        /// Returns a .NET SeekOrigin value from the passed Windows seek type.
        /// </summary>
        /// <param name="seektype">The Windows seek type</param>
        /// <returns>The .NET SeekOrigin value that corresponds to the passed Windows seek type</returns>
		public static SeekOrigin SeekOriginFromSeekType(int seektype)
		{
			SeekOrigin origin = SeekOrigin.Begin;
			switch (seektype)
			{
				case FCntl.SEEK_SET:
					origin = SeekOrigin.Begin;
					break;
				case FCntl.SEEK_CUR:
					origin = SeekOrigin.Current;
					break;
				case FCntl.SEEK_END:
					origin = SeekOrigin.End;
					break;
				default:
					// TODO: should I throw an exception if seektype is
					// any other value?
					break;
			}
			return origin;
		}

        /// <summary>
        /// Returns a .NET FileAttributes value from the passed Windows file attributes flags.
        /// </summary>
        /// <param name="attrs">The Windows file access flags.</param>
        /// <returns>The FileAttributes value that corresponds to the passed Windows file attributes flags.</returns>
		public static FileAttributes FileAttributesFromFAttrs(short attrs)
		{
			FileAttributes fa = FileAttributes.Normal;
			if ((attrs & FCntl.A_ARCH) != 0)
				fa |= FileAttributes.Archive;
			if ((attrs & FCntl.A_HIDDEN) != 0)
				fa |= FileAttributes.Hidden;
			if ((attrs & FCntl.A_RDONLY) != 0)
				fa |= FileAttributes.ReadOnly;
			if ((attrs & FCntl.A_SYSTEM) != 0)
				fa |= FileAttributes.System;
			return fa;
		}
	
        /// <summary>
        /// Returns the Windows file access flags from the passed FileAttributes value
        /// </summary>
        /// <param name="fa">The .NET FileAttributes value to convert.</param>
        /// <returns>The Windows file access flags that correspond to the passed FileAttributes value.</returns>
		public static short FAttrsFromFileAttributes(FileAttributes fa)
		{
			int attrs = 0;
			if ((fa & FileAttributes.Archive) != 0)
				attrs |= A_ARCH;
			if ((fa & FileAttributes.Hidden) != 0)
				attrs |= A_HIDDEN;
			if ((fa & FileAttributes.ReadOnly) != 0)
				attrs |= A_RDONLY;
			if ((fa & FileAttributes.System) != 0)
				attrs |= A_SYSTEM;
			return (short)attrs;
		}

        /// <summary>
        /// Creates a System.DateTime from the passed DOS date and time values.
        /// </summary>
        /// <param name="_date">The date to include.</param>
        /// <param name="time">The time to include.</param>
        /// <returns>A DateTime value that corresponds to the passed DOS date and time</returns>
		public static DateTime DateTimeFromDosDateTime(short date, short time)
		{
			if (date <= 0 || time <= 0)
				return DateTime.Now;
		
			// Format of date is:
			// Bits 0-4 - Day of month (1-31)
			// Bits 5-8 - Month (1-12)
			// Bits 9-15 - Year offset from 1980 (1990 = 10)
			int day = date & 0x001f;
			int month = (date >> 5) & 0x000f;
			int year = 1980 + ((date >> 9) & 0x007f);

			// Format of time is:
			// Bits 0-4 - second divided by 2
			// Bits 5-10 - minute (0-59)
			// Bits 11-15 - (0-23 on a 24-hour clock)
			int second = 2*(time & 0x001f);
			int minute = (time >> 5) & 0x003f;
			int hour = (time >> 11) & 0x001f;

			return new DateTime(year, month, day, hour, minute, second);
		}

        /// <summary>
        /// Creates DOS date/time fields from a System.DateTime value
        /// </summary>
        /// <param name="fileDate">The System.DateTime to convert.</param>
        /// <param name="fatDate">The returned DOS date.</param>
        /// <param name="fatTime">The returned DOS time.</param>
		public static void DosDateTimeFromDateTime(DateTime fileDate, 
			ref short fatDate, ref short fatTime)
		{
			fatDate = (short)(((fileDate.Year-1980) << 9) | (fileDate.Month << 5) | fileDate.Day);
			fatTime = (short)((fileDate.Hour << 11) | (fileDate.Minute << 5) | fileDate.Second/2);
		}
	}
}
