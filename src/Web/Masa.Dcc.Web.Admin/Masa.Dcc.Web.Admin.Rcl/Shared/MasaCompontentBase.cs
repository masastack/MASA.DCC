// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Web.Admin.Rcl.Shared;

public abstract class MasaCompontentBase : ComponentBase
{
    private I18n? _languageProvider;

    [Inject]
    public IAuthClient AuthClient { get; set; } = default!;

    [CascadingParameter]
    public I18n I18n
    {
        get
        {
            return _languageProvider ?? throw new Exception("please Inject I18n!");
        }
        set
        {
            _languageProvider = value;
        }
    }

    public string T(string key)
    {
        return I18n.T(key);
    }

    public async Task<UserPortraitModel> GetUserAsync(Guid userId)
    {
        var users = await AuthClient.UserService.GetUserPortraitsAsync(userId);

        return users.Any() ? users[0] : new();
    }
}

