// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using FluentValidation;

namespace Masa.Dcc.Web.Admin.Rcl.Model
{
    public class UpdateLabelModel
    {
        public string TypeCode { get; set; } = "";

        public string TypeName { get; set; } = "";

        public string Description { get; set; } = "";

        public List<LabelValueModel> LabelValues { get; set; } = new();
    }

    public class LabelValueModel : LabelValueDto
    {
        public bool Disabled { get; set; } = true;

        public int Index { get; set; }

        public LabelValueModel(int index)
        {
            Index = index;
            Disabled = false;
        }

        public LabelValueModel(string name, string code, int index)
        {
            Name = name;
            Code = code;
            Index = index;
        }
    }

    class LabelModelValidator : AbstractValidator<UpdateLabelModel>
    {
        public LabelModelValidator()
        {
            RuleFor(o => o.TypeCode)
                .NotEmpty()
                .Matches(@"^[\u4E00-\u9FA5A-Za-z0-9_-]+$").WithMessage("Please enter [Chinese, English、and - _ symbols]")
                .MinimumLength(2)
                .MaximumLength(50)
                .WithMessage("environment name length range is [2-50]");

            RuleFor(o => o.TypeName)
                .NotEmpty()
                .Matches(@"^[\u4E00-\u9FA5A-Za-z0-9_-]+$").WithMessage("Please enter [Chinese, English、and - _ symbols]")
                .MinimumLength(2)
                .MaximumLength(50)
                .WithMessage("environment name length range is [2-50]");

            RuleForEach(o => o.LabelValues).SetValidator(new EnvClusterModelValidator());
        }
    }

    class EnvClusterModelValidator : AbstractValidator<LabelValueModel>
    {
        public EnvClusterModelValidator()
        {
            RuleFor(o => o.Name)
                .NotEmpty()
                .Matches(@"^[\u4E00-\u9FA5A-Za-z0-9_-]+$").WithMessage("Please enter [Chinese, English、and - _ symbols]")
                .MinimumLength(2)
                .MaximumLength(50)
                .WithMessage("environment name length range is [2-50]");

            RuleFor(o => o.Code)
                .NotEmpty()
                .Matches(@"^[\u4E00-\u9FA5A-Za-z0-9_-]+$").WithMessage("Please enter [Chinese, English、and - _ symbols]")
                .MinimumLength(2)
                .MaximumLength(50)
                .WithMessage("environment name length range is [2-50]");
        }
    }
}
