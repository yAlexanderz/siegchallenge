using FiscalDocuments.Application.Commands;
using FiscalDocuments.Application.DTOs;
using FiscalDocuments.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FiscalDocuments.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class FiscalDocumentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<FiscalDocumentsController> _logger;

    public FiscalDocumentsController(IMediator mediator, ILogger<FiscalDocumentsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Upload de documento fiscal XML (NFe, CTe, NFSe)
    /// </summary>
    /// <param name="request">Arquivo XML</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Informações do documento processado</returns>
    [HttpPost("upload")]
    [ProducesResponseType(typeof(UploadDocumentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Upload([FromForm] UploadDocumentRequest request, CancellationToken cancellationToken)
    {
        if (request.XmlFile == null || request.XmlFile.Length == 0)
        {
            return BadRequest(new { message = "Arquivo XML é obrigatório" });
        }

        using var reader = new StreamReader(request.XmlFile.OpenReadStream());
        var xmlContent = await reader.ReadToEndAsync(cancellationToken);

        var command = new UploadDocumentCommand
        {
            XmlContent = xmlContent,
            FileName = request.XmlFile.FileName
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Listar documentos fiscais com paginação e filtros
    /// </summary>
    /// <param name="pageNumber">Número da página (padrão: 1)</param>
    /// <param name="pageSize">Tamanho da página (padrão: 10)</param>
    /// <param name="startDate">Data inicial (opcional)</param>
    /// <param name="endDate">Data final (opcional)</param>
    /// <param name="cnpj">CNPJ para filtro (opcional)</param>
    /// <param name="uf">UF para filtro (opcional)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista paginada de documentos</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<FiscalDocumentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? cnpj = null,
        [FromQuery] string? uf = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDocumentsPagedQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            StartDate = startDate,
            EndDate = endDate,
            Cnpj = cnpj,
            UF = uf
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Consultar detalhes de um documento específico
    /// </summary>
    /// <param name="id">ID do documento</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Detalhes completos do documento</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(FiscalDocumentDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetDocumentByIdQuery { DocumentId = id };
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
        {
            return NotFound(new { message = $"Documento com ID {id} não encontrado" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Atualizar um documento existente
    /// </summary>
    /// <param name="id">ID do documento</param>
    /// <param name="request">Novo arquivo XML</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Documento atualizado</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(FiscalDocumentDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdateDocumentRequest request, CancellationToken cancellationToken)
    {
        if (request.XmlFile == null || request.XmlFile.Length == 0)
        {
            return BadRequest(new { message = "Arquivo XML é obrigatório" });
        }

        using var reader = new StreamReader(request.XmlFile.OpenReadStream());
        var xmlContent = await reader.ReadToEndAsync(cancellationToken);

        var command = new UpdateDocumentCommand
        {
            DocumentId = id,
            XmlContent = xmlContent
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Excluir um documento
    /// </summary>
    /// <param name="id">ID do documento</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Confirmação de exclusão</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteDocumentCommand { DocumentId = id };
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}