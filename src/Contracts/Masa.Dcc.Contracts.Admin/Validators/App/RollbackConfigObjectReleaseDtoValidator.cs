// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Contracts.Admin.Validators.App;

public class RollbackConfigObjectReleaseDtoValidator : MasaAbstractValidator<RollbackConfigObjectReleaseDto>
{
    public RollbackConfigObjectReleaseDtoValidator()
    {
        RuleFor(dto => dto.ConfigObjectId).Required().GreaterThan(0);
        RuleFor(dto => dto.RollbackToReleaseId).Required().GreaterThan(0);
    }
}
