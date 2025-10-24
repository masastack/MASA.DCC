// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Contracts.Admin.App.Dtos
{
    public class AddConfigObjectDto
    {
        private string _name = "";

        public string Name { get => _name; set => _name = value.Trim(); }

        public string FormatLabelCode { get; set; } = "JSON";

        public ConfigObjectType Type { get; set; }

        public bool Encryption { get; set; }

        /// <summary>
        /// appid or publicid or bizid
        /// </summary>        
        public int ObjectId { get; set; }

        public int EnvironmentClusterId { get; set; }

        public string Content { get; set; } = default!;

        public string TempContent { get; set; } = default!;

        public int RelationConfigObjectId { get; set; }

        public bool FromRelation { get; set; }
    }
}
