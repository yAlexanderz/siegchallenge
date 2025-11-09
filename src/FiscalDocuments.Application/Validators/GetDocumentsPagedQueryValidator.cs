using FluentValidation;
using FiscalDocuments.Application.Queries;

namespace FiscalDocuments.Application.Validators;

public class GetDocumentsPagedQueryValidator : AbstractValidator<GetDocumentsPagedQuery>
{
    public GetDocumentsPagedQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Número da página deve ser maior que 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Tamanho da página deve ser maior que 0")
            .LessThanOrEqualTo(100).WithMessage("Tamanho da página não pode exceder 100");

        RuleFor(x => x.Cnpj)
            .Matches(@"^\d{14}$").When(x => !string.IsNullOrEmpty(x.Cnpj))
            .WithMessage("CNPJ deve conter 14 dígitos");

        RuleFor(x => x.UF)
            .Length(2).When(x => !string.IsNullOrEmpty(x.UF))
            .WithMessage("UF deve conter 2 caracteres");
    }
}