using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS3SaveUnpacker
{
    static class Utils
    {
        /// <summary>
        /// Throw an exception when the given condition fails.
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="message"></param>
        public static void Assert(bool condition, string message = "Assertion error")
        {
            if (!condition) { throw new Exception(message); }
        }

        /// <summary>
        /// Throw an exception when the given condition fails.
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="message"></param>
        public static void Assert(Func<bool> condition, string message = "Assertion error")
        {
            if (!condition()) { throw new Exception(message); }
        }
    }
}
