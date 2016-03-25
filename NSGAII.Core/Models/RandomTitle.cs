using System;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;

namespace NSGAII.Models
{
    public static class RandomTitle
    {
        private const string CHAR_LIST = "K0A1B2C3D4E5F6G7H8I9JLMNOPQRSTUVWXYZ";
        private static char[] _charArray = CHAR_LIST.ToCharArray();

        public static String Encode(BigInteger input)
        {
            if (input.Sign < 0) throw new ArgumentOutOfRangeException(nameof(input), input, "input cannot be negative");

            var result = new Stack<char>();
            while (!input.IsZero)
            {
                var index = (int)(input % 36);
                result.Push(CHAR_LIST[index]);
                input = BigInteger.Divide(input, 36);
            }
            return new string(result.ToArray());
        }

        public static string GetRandomTitle()
        {
            UInt32 minValue = 60466177;
            UInt32 maxValue = 2176782335;

            RandomNumberGenerator rng = new RNGCryptoServiceProvider();
            byte[] tokenData = new byte[25];

            if (minValue > maxValue)
                throw new ArgumentOutOfRangeException("minValue");
            if (minValue == maxValue) return "XXXXX";
            Int64 diff = maxValue - minValue;
            while (true)
            {
                rng.GetBytes(tokenData);
                UInt32 rand = BitConverter.ToUInt32(tokenData, 0);

                Int64 max = (1 + (Int64)UInt32.MaxValue);
                Int64 remainder = max % diff;
                if (rand < max - remainder)
                {
                    var value = (UInt32)(minValue + (rand % diff));
                    return Encode(value);
                }
            }


        }
    }
}
