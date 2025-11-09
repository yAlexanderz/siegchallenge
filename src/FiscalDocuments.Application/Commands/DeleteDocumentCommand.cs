using MediatR;

namespace FiscalDocuments.Application.Commands;

public class DeleteDocumentCommand : IRequest<Unit>
{
    public Guid DocumentId { get; set; }
}