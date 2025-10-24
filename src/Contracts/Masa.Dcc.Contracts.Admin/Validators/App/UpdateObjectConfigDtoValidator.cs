// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Contracts.Admin.Validators.App;

public class UpdateObjectConfigDtoValidator : MasaAbstractValidator<UpdateObjectConfigDto>
{
    public UpdateObjectConfigDtoValidator() { 
        RuleFor(m=>m.Id).Required().GreaterThan(0);
        RuleFor(m => m.Name).MinimumLength(2).MaximumLength(50)
            .Matches(@"^[\u4E00-\u9FA5A-Za-z0-9_-.]+$").WithMessage("Please enter [Chinese、Number、 English、and - _ . symbols] ");
        RuleFor(m=>m.Description).MaximumLength(255);
    }
}
