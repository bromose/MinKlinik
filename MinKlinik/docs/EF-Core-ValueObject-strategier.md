# EF Core Value Object-strategier for `Tidsinterval`

MinKlinik demonstrerer tre relevante mappings, som omtalt i kapitel 12:

- `OwnsOne`: klassisk owned type-mapping.
- `ComplexProperty`: EF Core 10 complex type-mapping.
- `ComplexProperty(...).ToJson()`: complex type lagret som JSON-kolonne.

Aktuel implementation i MinKlinik bruger `ComplexProperty(...).ToJson()` i
`src/MinKlinik.Infrastructure/Configurations/KonsultationConfiguration.cs`.

Som supplerende reference findes søsterprojektet `EfValueObjectsSqlServer2025Demo`,
hvor de tre strategier er sammenlignet side om side.
