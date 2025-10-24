// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Contracts.Admin.Validators.App;

public class AddConfigObjectDtoValidator : MasaAbstractValidator<AddConfigObjectDto>
{
    public AddConfigObjectDtoValidator()
    {
        RuleFor(m => m.Name).Required().MaximumLength(255).MinimumLength(2).Matches(@"^[\u4E00-\u9FA5A-Za-z0-9`~!@#%^&*()_\-+=<>?:""{}|,.\/;'\\[\]·~！￥%……&*（）——《》？：“”【】、；‘’，。]+$").WithMessage("Special symbols are not allowed");
        RuleFor(m => m.FormatLabelCode).Required();
        RuleFor(m => m.Type).Required().IsInEnum();
        RuleFor(m => m.ObjectId).Required();
    }
}
