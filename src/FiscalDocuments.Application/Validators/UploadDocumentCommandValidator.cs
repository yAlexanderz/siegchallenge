using FluentValidation;
using FiscalDocuments.Application.Commands;

namespace FiscalDocuments.Application.Validators;

public class UploadDocumentCommandValidator : AbstractValidator<UploadDocumentCommand>
{
    public UploadDocumentCommandValidator()
    {
        RuleFor(x => x.XmlContent)
            .NotEmpty().WithMessage("Conteúdo XML é obrigatório")
            .Must(BeValidXml).WithMessage("XML inválido");

        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("Nome do arquivo é obrigatório")
            .Must(x => x.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Arquivo deve ser XML");
    }

    private bool BeValidXml(string xml)
    {
        try
        {
            System.Xml.Linq.XDocument.Parse(xml);
            return true;
        }
        catch
        {
            return false;
        }
    }
}