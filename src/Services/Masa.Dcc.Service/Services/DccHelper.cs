// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Services;

public static class DccHelper
{
    public static async Task<List<LatestReleaseConfigModel>> FillUserName(this IAuthClient authClient,
        List<LatestReleaseConfigModel> data)
    {
        var ids = data.Select(t => t.LastPublisherId);
        var users = await authClient.UserService.GetListByIdsAsync(ids.ToArray());
        foreach (var t in data)
        {
            var u = users.FirstOrDefault(x => x.Id == t.LastPublisherId);
            t.LastPublisher = u?.StaffDislpayName ?? u?.DisplayName;
        }

        return data;
    }
}
