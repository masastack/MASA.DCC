// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Contracts.Admin.Validators.App;

internal class AddObjectConfigDtoValidator : MasaAbstractValidator<AddObjectConfigDto>
{
    public AddObjectConfigDtoValidator()
    {
        RuleFor(dto => dto.Name).Required()
            .MaximumLength(255).MinimumLength(2)
            .Matches(@"^[\u4E00-\u9FA5A-Za-z0-9_-.]+$").WithMessage("Please enter [Chinese, English、and - _ . symbols] ");
        RuleFor(dto => dto.Identity).Required()
            .MaximumLength(255).MinimumLength(2)
            .Matches(@"^[\u4E00-\u9FA5A-Za-z0-9_-.]+$").WithMessage("Please enter [Chinese, English、and - _ . symbols] ");

        RuleFor(dto => dto.Description)
            .MaximumLength(255);
    }
}
