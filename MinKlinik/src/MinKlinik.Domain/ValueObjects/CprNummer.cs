using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace MinKlinik.Domain.ValueObjects
{
    public record CprNummer
    {
        public string CprNummerAsString { get; init; }
        public DateOnly Fødselsdag { get; init; }

        public int Alder { get => BeregnAlder(); }

        public CprNummer(string cpr)
        {
            if (string.IsNullOrWhiteSpace(cpr) || cpr.Length != 11)
                throw new ArgumentException("Ugyldigt CPR-nummer");

            CprNummerAsString = cpr;

            var dag = int.Parse(cpr.Substring(0, 2));
            var maaned = int.Parse(cpr.Substring(2, 2));
            var aarToCifre = int.Parse(cpr.Substring(4, 2));
            var kontrolCiffer = int.Parse(cpr.Substring(7, 1));

            var aarFireCifre = 0;

            // Logik for at bestemme århundrede ud fra det 7. ciffer
            switch (kontrolCiffer)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                    aarFireCifre = 1900 + aarToCifre;
                    break;
                case 4:
                case 9:
                    aarFireCifre = aarToCifre <= 36 ? 2000 + aarToCifre : 1900 + aarToCifre;
                    break;
                case 5:
                case 6:
                case 7:
                case 8:
                    aarFireCifre = aarToCifre <= 57 ? 2000 + aarToCifre : 1800 + aarToCifre;
                    break;
            }

            var fødselsdato = new DateTime(aarFireCifre, maaned, dag);
            Fødselsdag = DateOnly.FromDateTime(fødselsdato);
        }

        private int BeregnAlder()
        {
            var nu = DateTime.Today;
            var alder = nu.Year - Fødselsdag.Year;

            // Hvis fødselsdagen ikke er nået endnu i år, trækkes ét år fra
            if (Fødselsdag > DateOnly.FromDateTime(nu.AddYears(-alder)))
                alder--;

            return alder;
        }


    }
}
