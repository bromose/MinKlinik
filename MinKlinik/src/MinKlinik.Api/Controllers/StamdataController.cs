using Microsoft.AspNetCore.Mvc;
using MinKlinik.UseCases.Readers;

namespace MinKlinik.Api.Controllers;

/// <summary>
/// Stamdata: behandlingstyper, patienter, behandlere.
/// Bruges til at slå ID'er op til brug i OpretKonsultation.
/// </summary>
[ApiController]
[Route("api/stamdata")]
public class StamdataController : ControllerBase
{
    private readonly IBehandlingstypeReader _behandlingstyper;
    private readonly IPatientReader _patienter;
    private readonly IBehandlerReader _behandlere;

    public StamdataController(
        IBehandlingstypeReader behandlingstyper,
        IPatientReader patienter,
        IBehandlerReader behandlere)
    {
        _behandlingstyper = behandlingstyper;
        _patienter = patienter;
        _behandlere = behandlere;
    }

    [HttpGet("behandlingstyper")]
    public async Task<IActionResult> HentBehandlingstyper()
        => Ok(await _behandlingstyper.HentAlleAsync());

    [HttpGet("patienter")]
    public async Task<IActionResult> HentPatienter()
        => Ok(await _patienter.HentAlleAsync());

    [HttpGet("behandlere")]
    public async Task<IActionResult> HentBehandlere()
        => Ok(await _behandlere.HentAlleAsync());
}
