///////////////////////////////////////////////////////////////////////////////////////////////////
// Project      -   Utilities
// General      -   Set of generic classes that may be useful in any project.
//
// File Name    -   ConfigManager.cs
// File Title   -   Configuration Manager
// Description  -   Simplifies reading appSettings from a config file.
// Notes        -   Handles decryption of passwords.  Allows connection string values apart from 
//                  passwords to be in plain text (cf the default behaviour which would encrypt 
//                  the whole connection string section of a config file).
//
// $History: ConfigManager.cs $
// 
// *****************  Version 3  *****************
// User: Simone       Date: 11/03/09   Time: 2:45p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities
// Make static methods thread safe.
// 
// *****************  Version 1  *****************
// User: Simone       Date: 13/02/08   Time: 3:58p
// Created in $/UtilitiesClassLibrary/UtilitiesClassLibrary/Utilities
// 
// *****************  Version 1  *****************
// User: Simone       Date: 24/10/07   Time: 11:57a
// Created in $/UtilitiesClassLibrary/Utilities
// Copied from version 2 in
// $/ServiceAlliance/JobsByEmail/JobsByEmailGeneric/EmailParser/Utilities.
///////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Configuration;

namespace Utilities.Miscellaneous
{
    /// <summary>
    /// Reads settings from config file.  Handles decryption of passwords.
    /// </summary>
    public static class ConfigManager
    {
        #region Data Members **********************************************************************

		private static bool _PwdsEncrypted;
		private static Encryptor _encryptor = new Encryptor();

		private static object _lockGetValue = new object();

		#endregion

		#region Constructors, Destructors / Finalizers and Dispose Methods ************************

		static ConfigManager()
		{
			string pwdsEncryptedText = ConfigurationManager.AppSettings["PwdsEncrypted"];
			_PwdsEncrypted = (pwdsEncryptedText == "1");
		}

		#endregion

		#region Properties ************************************************************************

		/// <summary>
		/// Indicates whether passwords are encrypted or not.
		/// </summary>
		public static bool PwdsEncrypted
		{
			get {return _PwdsEncrypted;}
		}

		#endregion

		#region Public Methods ********************************************************************

		/// <summary>
		/// Gets settings value.
		/// </summary>
		/// <param name="keyName">Name of key to return value for.</param>
		/// <returns>Settings value.  If there is no configuration setting with the given keyName, 
		/// returns null.</returns>
		/// <remarks>
		/// Will decrypt any password using the Encryptor class if passwords are encrypted.
		/// ASSUMPTION: Password key names contain either "pwd" or "password".
		/// </remarks>
		public static string GetValue(string keyName)
		{
			lock (_lockGetValue)
			{
				string sValue = ConfigurationManager.AppSettings[keyName];

				if (_PwdsEncrypted)
				{
					if (keyName.ToLower().IndexOf("pwd") > -1
					|| keyName.ToLower().IndexOf("password") > -1)
					{
						sValue = _encryptor.DecryptString(sValue);
					}
				}

				return sValue;
			}
		}

        /// <summary>
        /// Clears the cached version of the AppSettings section of a config file, forcing the 
        /// application to re-read the section from the file on disk.
        /// </summary>
        /// <remarks>
        /// Used to refresh the config settings if the config file has been updated while 
        /// the application is running.  Normally the application would have to be shut down 
        /// and restarted to pick up changes to the config file.
        /// </remarks>
        public static void Refresh()
        {
            ConfigurationManager.RefreshSection("appSettings");
		}

		#endregion
	}
}
