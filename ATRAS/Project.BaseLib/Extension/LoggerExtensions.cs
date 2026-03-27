using Project.BaseLib.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Project.BaseLib.Extension
{
    public static class LoggerExtensions
    {
        private static string HexDump(byte[] bytes, int bytesPerLine, string message = null)
        {
            int nLine = 0;
            int nCounter = 0;
            int nOffset = 0;

            string result = string.Empty;
            string offset = string.Empty;
            string text = string.Empty;
            string line = string.Empty;

            for (int i = 0; i < bytesPerLine; ++i)
            {
                if (nOffset % 16 == 0)
                {
                    nLine++;
                    result += string.Format("{0:D5}:{1:D5}  ", nLine, nOffset);
                }

                //
                result += string.Format("{0:x2} ", bytes[i]);
                if (bytes[i] >= 0x20 && bytes[i] <= 0x7e)
                {
                    text += Convert.ToChar(bytes[i]);
                }
                else
                {
                    text += '.';
                }

                //
                if (nCounter == 15)
                {
                    result += text;
                    result += Environment.NewLine;
                    nCounter = 0;
                    text = string.Empty;
                }
                else if (nCounter == 7)
                {
                    result += ' ';
                    nCounter++;
                }
                else
                {
                    nCounter++;
                }

                nOffset++;
            }
            //
            if (nCounter != 0)
            {
                for (; nCounter < 16; nCounter++)
                {
                    text += " ";
                    result += "   ";

                    if (nCounter == 7)
                    {
                        result += " ";
                    }
                }

                result += text;
                result += Environment.NewLine;
            }

            return result;
        }
        public static void Binary(this ILogger logger, byte [] bytes, int length, string desc = null, DateTime? datetime = null, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            string line;
            if (desc != null)
            {
                line = string.Format("{0} length={1}", desc, length);
            }
            else
            {
                line = string.Format("length={0}", length);
            }

            line += Environment.NewLine;
            line += HexDump(bytes, length);
            line += "-----------------------------------------------------------------------------";
            line += Environment.NewLine;

            logger.Debug(line, datetime, memberName, filePath, lineNumber);
        }
        public static void Binary(this ILogger logger, string dumpdatas, string desc = null, DateTime? datetime = null, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            byte[] enbyte = ASCIIEncoding.ASCII.GetBytes(dumpdatas);
            Binary(logger, enbyte, dumpdatas.Length, desc);
        }
    }
}
