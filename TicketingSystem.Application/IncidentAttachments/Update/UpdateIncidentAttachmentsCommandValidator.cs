﻿using FluentValidation;
using TicketingSystem.Application.IncidentAttachments.Add;

namespace TicketingSystem.Application.IncidentAttachments.Update;

public class UpdateIncidentAttachmentsCommandValidator
    : AbstractValidator<UpdateIncidentAttachmentsCommand>
{
    private static readonly string[] AllowedExtensions =
        { ".jpg", ".jpeg", ".png", ".pdf", ".docx", ".xlsx" };
    private const long MaxFileSize = 5 * 1024 * 1024;

    public UpdateIncidentAttachmentsCommandValidator()
    {
        RuleFor(c => c.Files)
            .NotNull().WithMessage("At least one file must be provided.")
            .Must(files => files.Count > 0).WithMessage("At least one file must be provided.")
            .Must(files => files.All(f => f.Length > 0))
                .WithMessage("All provided files must have a size greater than 0.");

        RuleForEach(c => c.Files).ChildRules(file =>
        {
            file.RuleFor(f => f.Length)
                .LessThanOrEqualTo(MaxFileSize)
                .WithMessage(f => $"{f.FileName} exceeds {MaxFileSize / (1024 * 1024)} MB limit.");

            file.RuleFor(f => Path.GetExtension(f.FileName).ToLowerInvariant())
                .Must(ext => AllowedExtensions.Contains(ext))
                .WithMessage(f => $"{f.FileName} has an invalid extension.");
        });
    }
}
