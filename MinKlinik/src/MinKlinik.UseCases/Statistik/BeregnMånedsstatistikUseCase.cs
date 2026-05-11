using MinKlinik.UseCases.Dtos;
using MinKlinik.UseCases.Readers;
using System;
using System.Collections.Generic;
using System.Text;

namespace MinKlinik.UseCases.Statistik
{
    internal class BeregnMånedsstatistikUseCase
    {
        private readonly IKonsultationReader _reader;
        public BeregnMånedsstatistikUseCase(IKonsultationReader reader) => _reader = reader;

        public async Task<MånedsstatistikDto> Udfør(int år, int måned)
        {
            // Hent rådata med async (I/O-bound)
            var konsultationer = await _reader.HentForMånedAsync(år, måned);

            // Beregn parallelt (CPU-bound)
            var samletScore = await Task.Run(() =>
            {
                decimal total = 0;
                Lock lås = new();

                Parallel.For(
                    fromInclusive: 0,
                    toExclusive: konsultationer.Count,
                    localInit: () => 0m,
                    body: (i, state, lokal) =>
                        lokal + BeregnComplexityScore(konsultationer[i]),
                    localFinally: lokal =>
                    {
                        lock (lås) total += lokal;
                    });

                return total;
            });

            return new MånedsstatistikDto(år, måned, konsultationer.Count, samletScore);
        }

        private decimal BeregnComplexityScore(KonsultationDto k)
        {
             /* ... */
             return 1;
        }
    }
}
