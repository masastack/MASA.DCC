// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Services;

public static class DccHelper
{
    public static async Task<List<LatestReleaseConfigModel>> FillUserNameAsync(this IAuthClient authClient,
        List<LatestReleaseConfigModel> data)
    {
        var ids = data.Select(t => t.LastPublisherId).Distinct();
        var users = await authClient.UserService.GetListByIdsAsync(ids.ToArray());
        foreach (var t in data)
        {
            var u = users.FirstOrDefault(x => x.Id == t.LastPublisherId);
            t.LastPublisher = u?.StaffDisplayName ?? u?.DisplayName;
        }

        return data;
    }

    public static async Task FillCreatorAndModifierNameAsync<T>(this IAuthClient authClient, List<T> data) where T : BaseDto
    {
        if (data.Count == 0)
            return;

        var ids = data.SelectMany(t => new[] { t.Creator, t.Modifier })
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToArray();
        if (ids.Length == 0)
            return;

        var users = await authClient.UserService.GetListByIdsAsync(ids);
        string? GetName(Guid id)
        {
            var user = users.FirstOrDefault(x => x.Id == id);
            return user?.StaffDisplayName ?? user?.DisplayName;
        }

        foreach (var t in data)
        {
            t.CreatorName = GetName(t.Creator);
            t.ModifierName = GetName(t.Modifier);
        }
    }

    public static async Task<List<ConfigObjectDto>> FillUserNameAsync(this IAuthClient authClient,
        List<ConfigObjectDto> data)
    {
        var ids = data.Where(x => x.LatestRelease != null).Select(t => t.LatestRelease!.LastPublisherId).Distinct();
        var users = await authClient.UserService.GetListByIdsAsync(ids.ToArray());
        foreach (var t in data)
        {
            if (t.LatestRelease != null)
            {
                var u = users.FirstOrDefault(x => x.Id == t.LatestRelease?.LastPublisherId);
                t.LatestRelease.LastPublisher = u?.StaffDisplayName ?? u?.DisplayName;
            }
        }

        return data;
    }
}
