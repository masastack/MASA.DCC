// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Web.Admin.Rcl.Pages;

public partial class ConfigItems
{
    private bool _configNameCopyClicked;
    private readonly string _checkSvg = "M21,7L9,19L3.5,13.5L4.91,12.09L9,16.17L19.59,5.59L21,7Z";

    private readonly string _copySvg =
        "M19,21H8V7H19M19,5H8A2,2 0 0,0 6,7V21A2,2 0 0,0 8,23H19A2,2 0 0,0 21,21V7A2,2 0 0,0 19,5M16,1H4A2,2 0 0,0 2,3V17H4V3H16V1Z";


    [Inject] public IJSRuntime Js { get; set; } = default!;

    [Parameter] public ConfigObjectType ConfigObjectType { get; set; }
    [Parameter] public ConfigObjectModel ConfigObject { get; set; } = default!;
    [Parameter] public EventCallback<ConfigObjectModel> ConfigObjectChanged { get; set; } = default!;
    [Parameter] public EventCallback<ConfigObjectModel> RevokeClick { get; set; }
    [Parameter] public EventCallback<ConfigObjectModel> RemoveClick { get; set; }
    [Parameter] public EventCallback<ConfigObjectModel> CancelEditConfigClick { get; set; }
    [Parameter] public EventCallback<ConfigObjectModel> UpdateConfigClick { get; set; }
    [Parameter] public EventCallback<ConfigObjectModel> CloneConfigClick { get; set; }
    [Parameter] public EventCallback<ConfigObjectModel> ReleaseConfigClick { get; set; }
    [Parameter] public EventCallback<ConfigObjectModel> RollbackClick { get; set; }
    [Parameter] public EventCallback<ConfigObjectModel> ReleaseHistoryClick { get; set; }

    private string FormatLabelCode
    {
        get
        {
            return ConfigObject.FormatLabelCode.Trim().ToLower();
        }
    }

    private bool DisabledButton
    {
        get
        {
            switch (FormatLabelCode)
            {
                case "json":
                    return ConfigObject.IsEditing;
                case "properties":
                    return ConfigObject.ElevationTabPropertyContent.Disabled;
                default:
                    return false;
            }
        }
    }

    private Task RevokeAsync()
    {
        return RevokeClick.InvokeAsync(ConfigObject);
    }

    private Task RemoveAsync()
    {
        return RemoveClick.InvokeAsync(ConfigObject);
    }

    private Task CancelEditConfig()
    {
        return CancelEditConfigClick.InvokeAsync(ConfigObject);
    }

    private Task UpdateConfig()
    {
        return UpdateConfigClick.InvokeAsync(ConfigObject);
    }

    private Task CloneConfig()
    {
        return CloneConfigClick.InvokeAsync(ConfigObject);
    }

    private Task ReleaseConfig()
    {
        return ReleaseConfigClick.InvokeAsync(ConfigObject);
    }

    private Task Rollback()
    {
        return RollbackClick.InvokeAsync(ConfigObject);
    }

    private Task ReleaseHistory()
    {
        return ReleaseHistoryClick.InvokeAsync(ConfigObject);
    }

    private async Task Copy(string value)
    {
        _configNameCopyClicked = true;

        await Js.InvokeVoidAsync(JsInteropConstants.Copy, value);

        await Task.Delay(500);

        _configNameCopyClicked = false;
    }
}
