using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gaia.Helpers
{   public class ConsoleMessage
    {
        private static object _MessageLock = new object();

        public static void WriteMessage(string message)
        {
            lock (_MessageLock)
            {
                Console.BackgroundColor = ConsoleColor.White;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(message);
                Console.ResetColor();

            }
        }
    }
}
