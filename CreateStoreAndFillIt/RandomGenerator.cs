using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateStoreAndFillIt
{
    public static class RandomGenerator
    {

        #region Fields

        static Random rand;

        #endregion

        #region Public methods

        /// <summary>
        /// Initializes the random number generator
        /// </summary>
        public static void Initialize()
        {
            rand = new Random();
        }

        /// <summary>
        /// Returns a nonnegative random number less than maxValue (exclusive)
        /// </summary>
        /// <param name="maxValue">the exclusive max value</param>
        /// <returns>the random number</returns>
        public static int Next(int maxValue)
        {
            return rand.Next(maxValue);
        }

        /// <summary>
        /// Returns a nonnegative random number less than maxValue (exclusive)
        /// </summary>
        /// <param name="maxValue">the exclusive max value</param>
        /// <returns>the random number</returns>
        public static float NextFloat(float maxValue)
        {
            return (float)rand.NextDouble() * maxValue;
        }

        /// <summary>
        /// Returns a random number between 0.0 and 1.0
        /// </summary>
        /// <returns>the random number</returns>
        public static double NextDouble()
        {
            return rand.NextDouble();
        }

        public static decimal NextDecimal(int maxValue, int decimals = 2)
        {

            return Math.Round(rand.Next(maxValue+1)*1m + (decimal)rand.NextDouble(), decimals);
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[rand.Next(s.Length)]).ToArray());
        }

        #endregion
    }
}
