// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Contracts.Admin.App.Dtos
{
    /// <summary>
    /// Config object search condition. All conditions are optional and combined with AND.
    /// </summary>
    public class SearchConfigObjectDto
    {
        /// <summary>Environment id.</summary>
        public int? EnvironmentId { get; set; }

        /// <summary>Cluster id.</summary>
        public int? ClusterId { get; set; }

        /// <summary>
        /// Project id. Matches the project's biz config objects and the config objects of all apps in the project.
        /// </summary>
        public int? ProjectId { get; set; }

        /// <summary>App id. When used together with <see cref="ProjectId"/>, the app must belong to the project.</summary>
        public int? AppId { get; set; }

        /// <summary>Config object name (fuzzy, case-insensitive).</summary>
        public string? ConfigObjectName { get; set; }

        /// <summary>Config object content (fuzzy, case-insensitive). Encrypted content is matched after decryption.</summary>
        public string? Content { get; set; }

        /// <summary>Page index, starts from 1.</summary>
        public int Page { get; set; } = 1;

        /// <summary>Page size, 1-200.</summary>
        public int PageSize { get; set; } = 20;
    }
}
