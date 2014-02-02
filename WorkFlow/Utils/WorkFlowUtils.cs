using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace WorkFlow.Utils
{
    public static class WorkFlowUtils
    {
        internal static IEnumerable<string> BinaryToStrings(byte[] data, int maxStringLength)
        {
            var buffer = new char[maxStringLength];

            using (MemoryStream stream = new MemoryStream(data))
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                while (true)
                {
                    int count = reader.Read(buffer, 0, maxStringLength);

                    if (count == 0)
                    {
                        break;
                    }

                    yield return new string(buffer, 0, count);
                }
            }
        }
    }
}
