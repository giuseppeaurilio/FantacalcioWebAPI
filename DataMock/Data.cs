using System;
using System.Linq;
using System.Text;

namespace DataMock
{
    public class FakeData
    {
        private static Random random = new Random();
        private static string lowercaseChars = "abcdefghijklmnopqrstuvwxyz";
        private static string uppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static string numbers = "1234567890";
        private static string specialChars = "!£$%&()='?^|+*@°#§<>,;.:-_{~}";// "\/
        public static string GetString(int length, bool useUppercase = false, bool useLowercase = false, bool useNumber = false, bool useSpecial = false)
        {
            StringBuilder charsSB = new StringBuilder();
            if (useUppercase)
                charsSB.Append(uppercaseChars);
            if (useLowercase)
                charsSB.Append(lowercaseChars);
            if (useNumber)
                charsSB.Append(numbers);
            if (useSpecial)
                charsSB.Append(specialChars);

            string chars = charsSB.ToString();

            StringBuilder resultStringBuilder = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                resultStringBuilder.Append(chars[random.Next(chars.Length)]);
            }
            return resultStringBuilder.ToString();
        }

        public static string GetEmail()
        {
            string user = FakeData.GetString(8, false, true, true, false);
            string domain = FakeData.GetString(8, false, true, false, false);
            string final = FakeData.GetString(3, false, true, true, false);

            return string.Format("{0}@{1}.{2}", user, domain, final);

        }

        public static double GetDouble(int parteIntera, int partedecimale)
        {
            string pi = FakeData.GetString(1, false, false, true, false);
            string pd = FakeData.GetString(2, false, false, true, false);
            return double.Parse(string.Format("{0},{1}", pi, pd));
        }

        /// <summary>
        /// restituisce una data successiva a quella passata, entro un range di giorni definito da min e max
        /// </summary>
        /// <param name="startDate">data a partire dalla quale si calcola il giorno randomico</param>
        /// <param name="min">minimo numero di ore da aggiungere alla data di partenza</param>
        /// <param name="max"> massimo numero di ore da aggiungere alla data di partenza</param>
        /// <returns></returns>
        public static DateTime GetDate(DateTime startDate, int min = 0, int max = 365)
        {
            Random random = new Random();
            return startDate.AddHours(random.Next(min, max));
        }

        /// <summary>
        /// restitnuisce un numero intero casuale
        /// </summary>
        /// <param name="min">numero minimo casuale</param>
        /// <param name="max">numero massimo casuale</param>
        /// <returns></returns>
        public static int GetIntegerNumber(int min = int.MinValue, int max = int.MaxValue)
        {
            Random random = new Random();
            return random.Next(min, max);
        }
    }
}
