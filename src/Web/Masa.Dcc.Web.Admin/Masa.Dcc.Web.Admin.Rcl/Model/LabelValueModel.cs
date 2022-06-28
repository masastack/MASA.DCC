// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Web.Admin.Rcl.Model
{
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
}
