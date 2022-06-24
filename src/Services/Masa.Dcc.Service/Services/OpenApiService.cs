// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc;

namespace Masa.Dcc.Service.Admin.Services
{
    public class OpenApiService : ServiceBase
    {
        public OpenApiService(IServiceCollection services) : base(services)
        {
        }

        public async Task UpdateConfigObjectAsync(string environment, string cluster, string appId, string configObject,
            [FromBody] object value, string secret)
        {

        }
    }
}
