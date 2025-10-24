// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Contracts.Admin.Validators.Label;

public class UpdateLabelDtoValidator : MasaAbstractValidator<UpdateLabelDto>
{
    public UpdateLabelDtoValidator()
    {
        RuleFor(m => m.TypeCode).Required().WithMessage("TypeCode is required").
            MaximumLength(50).MinimumLength(2)
            .Matches(@"^[\u4E00-\u9FA5A-Za-z0-9_.-]+$").WithMessage("Label Id is required");
        RuleFor(m => m.TypeName).Required().WithMessage("Please enter [Chinese、Number、 English、and - _ . symbols] ")
            .MaximumLength(50).MinimumLength(2)
            .Matches(@"^[\u4E00-\u9FA5A-Za-z0-9_.-]+$").WithMessage("Please enter [Chinese、Number、 English、and - _ . symbols] ");
        RuleFor(m => m.Description).MaximumLength(255).WithMessage("Description length range is [0-255]");
    }
}

public class LabelValueDtoValidator : MasaAbstractValidator<LabelValueDto>
{
    public LabelValueDtoValidator() { 
        RuleFor(m=>m.Name).Required()
            .MaximumLength(50).MinimumLength(2).WithMessage("Name length range is [2-50] ")
            .Matches(@"^[\u4E00-\u9FA5A-Za-z0-9_.-]+$").WithMessage("Special symbols are not allowed");

        RuleFor(m => m.Code).Required()
            .MaximumLength(50).MinimumLength(2).WithMessage("Code length range is [2-50] ")
            .Matches(@"^[\u4E00-\u9FA5A-Za-z0-9_.-]+$").WithMessage("Special symbols are not allowed");
    }
}
