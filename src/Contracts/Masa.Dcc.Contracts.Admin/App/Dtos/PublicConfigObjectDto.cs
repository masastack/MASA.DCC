// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Contracts.Admin.App.Dtos
{
    public class PublicConfigObjectDto
    {
        public int Id { get; set; }

        public int PublicConfigId { get; set; }

        public int ConfigObjectId { get; set; }

        public int EnvironmentClusterId { get; set; }
    }
}
