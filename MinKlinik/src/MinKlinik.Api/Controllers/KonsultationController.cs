using Microsoft.AspNetCore.Mvc;
using MinKlinik.UseCases.Dtos;
using MinKlinik.UseCases.Konsultationer;
using MinKlinik.UseCases.Readers;

namespace MinKlinik.Api.Controllers;

[ApiController]
[Route("api/konsultationer")]
public class KonsultationController : ControllerBase
{
    private readonly IOpretKonsultationUseCase _opretUC;
    private readonly IAfslutKonsultationUseCase _afslutUC;
    private readonly IAflysKonsultationUseCase _aflysUC;
    private readonly IKonsultationReader _reader;

    public KonsultationController(
        IOpretKonsultationUseCase opretUC,
        IAfslutKonsultationUseCase afslutUC,
        IAflysKonsultationUseCase aflysUC,
        IKonsultationReader reader)
    {
        _opretUC = opretUC;
        _afslutUC = afslutUC;
        _aflysUC = aflysUC;
        _reader = reader;
    }

    /// <summary>
    /// Opret en ny konsultation.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Opret(OpretKonsultationRequest request)
    {
        await _opretUC.Udfør(request);
        return Ok();
    }

    /// <summary>
    /// Hent alle konsultationer.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> HentAlle()
    {
        var result = await _reader.HentAlleAsync();
        return Ok(result);
    }

    /// <summary>
    /// Hent en konsultation via ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> Hent(Guid id)
    {
        var dto = await _reader.HentAsync(id);
        return dto is null ? NotFound() : Ok(dto);
    }

    /// <summary>
    /// Afslut en konsultation med et notat.
    /// </summary>
    [HttpPut("{id}/afslut")]
    public async Task<IActionResult> Afslut(Guid id, AfslutKonsultationRequest request)
    {
        await _afslutUC.Udfør(request with { KonsultationId = id });
        return NoContent();
    }

    /// <summary>
    /// Aflys en konsultation.
    /// </summary>
    [HttpPut("{id}/aflys")]
    public async Task<IActionResult> Aflys(Guid id)
    {
        await _aflysUC.Udfør(new AflysKonsultationRequest(id));
        return NoContent();
    }
}
