using FluentValidation;
using DevPulse.Application.DTOs;

namespace DevPulse.Application.Validators;

public class SearchRepositoryRequestValidator : AbstractValidator<SearchRepositoryRequest>
{
    public SearchRepositoryRequestValidator()
    {
        RuleFor(x => x.Query)
            .NotEmpty().WithMessage("Repository query is required.")
            .MaximumLength(300).WithMessage("Query cannot exceed 300 characters.");
    }
}
