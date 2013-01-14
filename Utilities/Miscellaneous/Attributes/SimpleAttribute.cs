using System;

using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Utilities.Miscellaneous.Attributes
{
    /// <summary>
    /// SimpleAttribute defines an attribute base which contains a single value.
    /// Use this to simplify adding attributes that only require simple values.
    /// </summary>
    public abstract class SimpleAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the SimpleAttribute class.
        /// </summary>
        /// <param name="value">The value of the attribute.</param>
        public SimpleAttribute(string value)
        {
            Value = value;
        }

		private string _value;
        /// <summary> Gets the value of this attribute. </summary>
        public string Value 
		{
			get
			{
				return _value;
			}
			private set
			{
				_value = value;
			}
		}

        public static string GetDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes.Length > 0)
            {
                return attributes[0].Description;
            }
            else
            {
                return value.ToString();
            }
        }
    }
}