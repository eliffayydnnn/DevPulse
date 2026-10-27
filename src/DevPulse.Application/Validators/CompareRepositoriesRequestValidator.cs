using FluentValidation;
using DevPulse.Application.DTOs;

namespace DevPulse.Application.Validators;

public class CompareRepositoriesRequestValidator : AbstractValidator<CompareRepositoriesRequest>
{
    public CompareRepositoriesRequestValidator()
    {
        RuleFor(x => x.Repositories)
            .NotNull().WithMessage("Repositories list is required.")
            .Must(list => list.Count >= 2 && list.Count <= 4)
            .WithMessage("Please select between 2 and 4 repositories to compare.");

        RuleForEach(x => x.Repositories)
            .NotEmpty().WithMessage("Repository query in comparison list cannot be empty.");
    }
}
