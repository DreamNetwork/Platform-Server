using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace StatusPlatformTest
{
    class PrintUtils
    {
        private static readonly object ConsoleLockObj = new object();

        public static void HexDisplay(IEnumerable<byte> data, string comment = null)
        {
            var bytes = data as byte[] ?? data.ToArray();
            HexDisplay(bytes, 0, bytes.Length, comment);
        }

        public static void HexDisplay(IEnumerable<byte> data, int offset, int length, string comment = null)
        {
            lock (ConsoleLockObj)
            {
                var bytes = data.Skip(offset).Take(length).ToArray();

                comment = offset != 0 ? string.Format("{2} at 0x{0:X8} ({1} bytes)", offset, length, comment ?? "Raw data") : string.Format("{1} ({0} bytes)", length, comment ?? "Raw data");

                Debug.WriteLine(comment);

                for (var i = 0; i < bytes.Length; i += 16)
                {
                    var rowBytes = bytes.Skip(i).Take(Math.Min(16, bytes.Length - i)).ToArray();
                    Debug.WriteLine("\t{0}\t{1}",
                        BitConverter.ToString(rowBytes).Replace("-", " ").PadRight(3*16),
                        new string(rowBytes.Select(b => (char) b).Select(c => IsPrintable(c) ? c : '.').ToArray())
                        );
                }
            }
        }

        private static bool IsPrintable(char c)
        {
            return !Char.IsControl(c)
                   && c != (char) 0x2028 && c != (char) 0x2029; // see comment in http://stackoverflow.com/a/13499234
        }
    }
}
