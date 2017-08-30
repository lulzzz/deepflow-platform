using System;
using System.Security.Cryptography;
using System.Text;

namespace Deepflow.Platform.Series.Providers
{
    public class HashValueGenerator : IValueGenerator
    {
        private static readonly SHA1 Sha1 = SHA1.Create();

        public double GenerateValueBetween(string name, long timeSeconds, long minSeconds, long maxSeconds, double minValue, double maxValue, double variance)
        {
            var fractionBetween = (timeSeconds - minSeconds) / (maxSeconds - minSeconds);
            var midValue = (maxValue - minValue) * fractionBetween + minValue;

            var key = $"{name}_{timeSeconds}";
            var hash = Hash(key);
            var deviation = (new Random(hash).NextDouble() - 0.5) * 2 * variance;
            return midValue + deviation;
        }

        public double GenerateValue(string name, long timeSeconds, double minValue, double maxValue)
        {
            var key = $"{name}_{timeSeconds}";
            var hash = Hash(key);
            return new Random(hash).NextDouble() * (maxValue - minValue) + minValue;
        }

        private int Hash(string src)
        {
            byte[] stringbytes = Encoding.UTF8.GetBytes(src);
            byte[] hashedBytes = Sha1.ComputeHash(stringbytes);
            Array.Resize(ref hashedBytes, 4);
            return BitConverter.ToInt32(hashedBytes, 0);
        }
    }
}
