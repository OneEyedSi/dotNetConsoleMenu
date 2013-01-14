///////////////////////////////////////////////////////////////////////////////////////////////////
// Project      -   Utilities
// General      -   Set of generic classes that may be useful in any project.
//
// File Name    -   Encryptor.cs
// File Title   -   Encryptor / Decryptor
// Description  -   Simple class for performing encryption and decryption.
// Notes        -   
//
// $History: Encryptor.cs $
// 
// *****************  Version 1  *****************
// User: Simone       Date: 13/02/08   Time: 3:58p
// Created in $/UtilitiesClassLibrary/UtilitiesClassLibrary/Utilities
// 
// *****************  Version 1  *****************
// User: Simone       Date: 24/10/07   Time: 12:09p
// Created in $/UtilitiesClassLibrary/Utilities
// Added file header comment block, changed some variable names to conform
// to standard naming convention.
///////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Globalization;
using System.Text;
using System.Security.Cryptography;
using System.Data;
using Microsoft.Win32;

namespace Utilities.Miscellaneous
{
    /// <summary>
	/// Simple class for performing encryption and decryption.
    /// </summary>
	public class Encryptor
	{
		#region Class Variables ***************************************************************************************

		// Key to reconstruct the RSA object.
		private const string _xmlKey = "<Modulus>xJUkb5S9yRzrm348w/0hEtiKwx6ctnyV+4ps8Qdw/RP0bN5inyYj/bTcUaUgraejeKOA/aM3a7oeqffhLrdD+sChCrqi1lSoG5qOmzT9gJP7Yy+PcVL1bNkGK61vBCBFNGCPmFAcRx+QAwkEFj7KluI62mABNC0EzaRgPKZEyIM=</Modulus><Exponent>AQAB</Exponent><P>5NgUp2EjplQw5h72fB9SGgAwdf1rFMSNNBu4TMzQqaScfMn4s6Iv9EeDugRgM+pCoRCyXgbBcXJmx8q+fJr9iw==</P><Q>2+kCWWOUlntCOimA4a1NYYwfJZU/x2+xifmrHfjRzUgqrUsx7GIvBU5iks1vZ+AYx8CK4aMEjLRlQNN8Xeav6Q==</Q><DP>mww3+ivbjo8OTmv+Dpzd8JXeP6MCkSCWlw6M8SP34GiSSg5BvduOaBCoFDlwwNvgZuY8I26qU+Xx8z3Pj/cm/w==</DP><DQ>RBcNhyfyJfXcN64KHdZPE1kTe8uOh+3phtMrTIhyTaF+tVGHD64G6RmwI8xAJmWYxqCzX9Hd4sMoZr4Uz+5RoQ==</DQ><InverseQ>cKoNU9GTe1iaP0pIl5EXkeGTcZLtMOAiCRZIQd0tqBYC7/Dc8bihAkebDamf9O+jsCvx3Erae268yCDUmkdpTQ==</InverseQ><D>f5ex635mZGeSAP3BoQ/l7J6CCj0PSF661mY1aYgD7S+LgTIiXtvZlm1SZue/uxbIwp+VNItAiHpoNre9/51R0Ab8K+xi37Nom4mphs/AWQmL5vAxq/seTbeMut6Ro+LPkys98X3PNo2XthjspO5dUzztsN9yAAEJdY/ML3A6tZE=</D>";

		#endregion

		#region Constructors and Destructors **************************************************************************
		
		#endregion

		#region Properties ********************************************************************************************
		
		#endregion

		#region Public Methods ****************************************************************************************
		
		/// <summary>
		/// Encrypts the given string.
		/// </summary>
		/// <param name="PlainText">The string to be encrypted.</param>
        /// <returns>A string of hexadecimal digits representing the byte array that was output by the RSA Encrypt method.</returns>
        public string EncryptString(string plainText)
		{
			byte[] bPlain;
			byte[] bEncrypted;
			ASCIIEncoding encoder = new ASCIIEncoding();

			// CREATE A NEW INSTANCE OF RSACryptoServiceProvider USING THE PRE-SPECIFIED KEY.
			RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
			RSA.FromXmlString("<RSAKeyValue>" + _xmlKey + "</RSAKeyValue>");

			// ENCRYPT THE REQUIRED STRING.
			bPlain = encoder.GetBytes(plainText);
			bEncrypted = RSA.Encrypt(bPlain, false);
			
			string sOutput = "";
			for (int i = 0; i < bEncrypted.Length; i++)
			{
				sOutput +=  bEncrypted[i].ToString("X").PadLeft(2, '0');
			}
			return sOutput;
		}

		/// <summary>
		/// Decrypts the given string of hexadecimal digits which represent a string encrypted 
		/// using the EncryptString method.
		/// </summary>
		/// <param name="HexString">The encrypted data in the form of a string of hexadecimal digits.</param>
        /// <returns>The decrypted string.</returns>
        public string DecryptString(string hexString)
		{
			string sOutput = "";
			byte[] bPlain;
			ASCIIEncoding encoder = new ASCIIEncoding();

			byte[] bEncrypted = new byte[(hexString.Length / 2)];

			char[] cHex = hexString.ToCharArray();
			for (int i = 0; i < hexString.Length; i += 2)
			{
				string sHex = cHex[i].ToString() + cHex[i + 1].ToString();
				bEncrypted[(i/2)] = byte.Parse(sHex, NumberStyles.HexNumber);  
			}

            // CREATE A NEW INSTANCE OF RSACryptoServiceProvider USING THE PRE-SPECIFIED KEY.
			RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
			RSA.FromXmlString("<RSAKeyValue>" + _xmlKey + "</RSAKeyValue>");

            // DECRYPT THE REQUIRED STRING.
			bPlain = RSA.Decrypt(bEncrypted, false);
			sOutput = encoder.GetString(bPlain);

			return sOutput;
		}

		#endregion

		#region Private and Protected Methods *************************************************************************

		#endregion
	}
}
