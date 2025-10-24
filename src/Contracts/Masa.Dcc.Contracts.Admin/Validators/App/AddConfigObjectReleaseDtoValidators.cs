// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Contracts.Admin.Validators.App;

public class AddConfigObjectReleaseDtoValidators : MasaAbstractValidator<AddConfigObjectReleaseDto>
{
    public AddConfigObjectReleaseDtoValidators()
    {
        RuleFor(dto => dto.Type).Required();
        RuleFor(dto => dto.ConfigObjectId).Required().WithMessage("Config object Id is required")
            .GreaterThan(0).WithMessage("Config object Id is required");
        RuleFor(dto => dto.Name).Required().WithMessage("Name is required")
            .MinimumLength(2).MaximumLength(50).WithMessage("Name length range is [2-50]");

        RuleFor(dto => dto.Comment).Required().WithMessage("Comment is required")
            .MinimumLength(2).MaximumLength(255).WithMessage("Comment length range is [0-255]");

        RuleFor(dto => dto.EnvironmentName).Required();
        RuleFor(dto => dto.ClusterName).Required();
        RuleFor(dto => dto.Identity).Required();
        RuleFor(dto => dto.Content).Required();
    }
}
