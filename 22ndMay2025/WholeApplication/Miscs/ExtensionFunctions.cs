using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WholeApplication.Misc
{
    public static class ExtensionFunctions
    {
        /**
        * Performs a custom validation check on a string.
        *
        * @param {string} str - The string to validate.
        * @returns {boolean} - True if the string starts with "s" (case-insensitive) and has a length of 6; otherwise, false.
        */
        public static bool StringValidationCheck(this string str)
        {
            if (str.Substring(0, 1).ToLower() == "s" && str.Length == 6)
                return true;
            return false;
        }
    }
}