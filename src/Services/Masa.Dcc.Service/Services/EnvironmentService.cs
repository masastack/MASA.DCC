// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Services;

[Authorize]
public class EnvironmentService : ServiceBase
{
    public EnvironmentService(IPmClient pmClient)
    {
        RouteOptions.DisableAutoMapRoute = true;
        App.MapGet("api/v1/env", GetListAsync).RequireAuthorization();
        App.MapGet("api/v1/env/{Id}", GetAsync).RequireAuthorization();
    }

    public async Task<List<EnvironmentModel>> GetListAsync(IPmClient pmClient)
    {
        var result = await pmClient.EnvironmentService.GetListAsync();

        return result;
    }

    public async Task<EnvironmentDetailModel> GetAsync(IPmClient pmClient,int Id)
    {
        var result = await pmClient.EnvironmentService.GetAsync(Id);

        return result;
    }
}
