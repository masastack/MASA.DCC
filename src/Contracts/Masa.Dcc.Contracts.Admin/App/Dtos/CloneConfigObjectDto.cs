// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Contracts.Admin.App.Dtos
{
    public class CloneConfigObjectDto
    {
        /// <summary>
        /// AppId or BizId or PublicId
        /// </summary>
        public int ToObjectId { get; set; }

        public ConfigObjectType ConfigObjectType { get; set; }

        public List<AddConfigObjectDto> ConfigObjects { get; set; } = new();

        public List<AddConfigObjectDto> CoverConfigObjects { get; set; } = new();
    }
}
