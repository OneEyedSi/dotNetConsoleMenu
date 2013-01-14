using System;

using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Utilities.Miscellaneous.Attributes
{
    public class DescriptionAttribute : SimpleAttribute
    {
        public DescriptionAttribute(string description)
            : base(description)
        {
        }

        public string Description { get { return Value; } }
    }
}
