using Project.BaseLib.Enums;
using Project.BaseLib.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.HWC
{
    public class CrevisSensors
    {

        protected BitArray[] bits = null;
        //
        public CrevisSensors()
        {
        }

        public CrevisSensors(string svalues)
        {
            byte[] values = HexStringToByteArray(svalues);



            int chcount = values.Length / 2;

            byte[] bytes = new byte[2];
            bits = new BitArray[chcount];

            int k = 0;
            for (int i = 0; i < values.Length; i += 2)
            {
                Array.Copy(values, i, bytes, 0, 2);
                bits[k++] = new BitArray(bytes);
            }

            string result = string.Empty;

            for (int i = 0; i < bits.Length; i++)
            {
                for (int j = 0; j < bits[i].Count; j++)
                {
                    result += bits[i][j] ? "1" : "0";
                }
                result += Environment.NewLine;
            }
        }

        public CrevisSensors(byte[] values)
        {
            if(values != null)
            {
                int chcount = values.Length / 2;

                byte[] bytes = new byte[2];
                bits = new BitArray[chcount];

                int k = 0;
                for (int i = 0; i < values.Length; i += 2)
                {
                    Array.Copy(values, i, bytes, 0, 2);
                    bits[k++] = new BitArray(bytes);
                }

                string result = string.Empty;

                for (int i = 0; i < bits.Length; i++)
                {
                    for (int j = 0; j < bits[i].Count; j++)
                    {
                        result += bits[i][j] ? "1" : "0";

                    }
                    result += Environment.NewLine;
                }
            }
            else
            {
                int chcount = 6;
                byte[] bytes = new byte[2];
                bytes[0] = 0x00;
                bytes[1] = 0x00;
                int k = 0;
                for (int i = 0; i < chcount *2; i += 2)
                {
                    Array.Copy(values, i, bytes, 0, 2);
                    bits[k++] = new BitArray(bytes);
                }
            }

        }

        public byte[] HexStringToByteArray(string hexString)
        {
            // 문자열 길이가 짝수인지 확인
            if (hexString.Length % 2 != 0)
            {
                //throw new ArgumentException("16진수 문자열의 길이는 반드시 짝수여야 합니다.");
                AppLogger.Error()("16진수 문자열의 길이는 반드시 짝수여야 합니다.");
            }

            // byte 배열로 변환
            byte[] byteArray = new byte[hexString.Length / 2];

            // 16진수 문자열을 2자리씩 잘라서 byte로 변환
            for (int i = 0; i < hexString.Length; i += 2)
            {
                byteArray[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            }

            return byteArray;
        }

        public override string ToString()
        {
            string result = string.Empty;

            for (int i = 0; i < bits.Length; i++)
            {
                result += ToStringChannel(i);

                if (i != bits.Length - 1)
                    result += ", ";
            }

            return result;
        }

        public string ToStringChannel(int ch)
        {
            string result = string.Empty;

            string channel = string.Format($"ch{ch} : ");

            string data = string.Empty;

            for (int j = 0; j < bits[ch].Count; j++)
            {
                data += bits[ch][j] ? "1" : "0";
            }
            return channel + data;
        }

        public BitArray[] Bits
        {
            get
            {
                return bits;
            }
        }

        public void SetIOState(int ch, int no, IOStates state)
        {
            bits[ch][no] = state == IOStates.On ? true : false;
        }

        public void SetIOChannel(int ch, byte [] data)
        {
            bits[ch] = new BitArray(data);
        }
    }
}
