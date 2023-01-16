// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Application.App.Commands
{
    //public record AddBizConfigCommand(AddObjectConfigDto AddBizConfigDto) : Command
    //{
    //    public BizConfigDto BizConfigDto { get; set; } = new();
    //}

    //TODO:This is an urgent special treatment,to deal with guest account.
    //It will be filtered with properties in the future
    public record AddBizConfigCommand(AddObjectConfigDto AddBizConfigDto) : Query<string?>
    {
        public BizConfigDto BizConfigDto { get; set; } = new();

        public override string? Result { get; set; }
    }
}
