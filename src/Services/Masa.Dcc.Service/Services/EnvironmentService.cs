// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Services;

public class EnvironmentService : ServiceBase
{
    private readonly IPmClient _pmClient;

    public EnvironmentService(IPmClient pmClient)
    {
        _pmClient = pmClient;
        App.MapGet("api/v1/env", GetListAsync);
        App.MapGet("api/v1/env/{Id}", GetAsync);
    }

    public async Task<List<EnvironmentModel>> GetListAsync()
    {
        var result = await _pmClient.EnvironmentService.GetListAsync();

        return result;
    }

    public async Task<EnvironmentDetailModel> GetAsync(int Id)
    {
        var result = await _pmClient.EnvironmentService.GetAsync(Id);

        return result;
    }
}
