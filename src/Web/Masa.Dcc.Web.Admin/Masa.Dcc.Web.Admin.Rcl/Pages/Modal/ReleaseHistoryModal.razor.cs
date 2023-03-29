// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Web.Admin.Rcl.Pages.Modal;

public partial class ReleaseHistoryModal
{
    private readonly List<DataTableHeader<ConfigObjectPropertyModel>> _allConfigheaders = new()
    {
        new DataTableHeader<ConfigObjectPropertyModel>
        {
            Text = "Key", Value = nameof(ConfigObjectPropertyModel.Key)
        },
        new DataTableHeader<ConfigObjectPropertyModel>
        {
            Text = "Value", Value = nameof(ConfigObjectPropertyModel.Value)
        }
    };

    private List<ConfigObjectPropertyModel> _changedProperties = new();
    private List<ConfigObjectReleaseModel> _configObjectReleases = new();
    private ConfigObjectReleaseDto? _currentConfigObjectRelease;
    private string _formatLabelCode = default!;
    private Action _handleRollbackOnClickAfter = () => { };
    private ConfigObjectReleaseModel _prevReleaseHistory = new();
    private ConfigObjectWithReleaseHistoryDto _releaseHistory = new();
    private string _releaseTabText = "All configuration";
    private ConfigObjectReleaseDto? _rollbackConfigObjectRelease;
    private ConfigObjectReleaseModel _selectReleaseHistory = new();
    private bool _showRollbackModal;

    [Parameter]
    public bool Value { get; set; }

    [Parameter]
    public EventCallback OnSubmitAfter { get; set; }

    [Inject]
    public IPopupService PopupService { get; set; } = default!;

    [Inject]
    public ConfigObjectCaller ConfigObjectCaller { get; set; } = default!;

    private List<DataTableHeader<ConfigObjectPropertyModel>> ChangedConfigheaders
    {
        get
        {
            var headers = new List<DataTableHeader<ConfigObjectPropertyModel>>
            {
                new() { Text = T("State"), Value = nameof(ConfigObjectPropertyModel.IsPublished) },
                new() { Text = T("Key"), Value = nameof(ConfigObjectPropertyModel.Key) },
                new() { Text = T("New value"), Value = nameof(ConfigObjectPropertyModel.Value) },
                new() { Text = T("Old value"), Value = nameof(ConfigObjectPropertyModel.TempValue) }
            };

            return headers;
        }
    }

    public async Task InitDataAsync(ConfigObjectModel configObject)
    {
        _releaseHistory = await ConfigObjectCaller.GetReleaseHistoryAsync(configObject.Id);
        _configObjectReleases = _releaseHistory.ConfigObjectReleases
            .OrderByDescending(release => release.Id)
            .Adapt<List<ConfigObjectReleaseModel>>();

        if (configObject.FormatLabelCode.Trim().ToLower() == "properties")
        {
            _configObjectReleases.ForEach(release =>
            {
                release.ConfigObjectProperties =
                    JsonSerializer.Deserialize<List<ConfigObjectPropertyModel>>(release.Content) ??
                    new List<ConfigObjectPropertyModel>();
            });
        }

        if (_configObjectReleases.Count < 1)
        {
            await PopupService.EnqueueSnackbarAsync(T("No publishing history"), AlertTypes.Error);
        }
        else
        {
            await OnTimelineItemClickAsync(_configObjectReleases.First());
            Value = true;
            StateHasChanged();
        }
    }

    private async Task OnTimelineItemClickAsync(ConfigObjectReleaseModel configObjectRelease)
    {
        _releaseTabText = "All configuration";
        _selectReleaseHistory = configObjectRelease;

        var creatorInfo = await GetUserAsync(_selectReleaseHistory.Creator);
        _selectReleaseHistory.CreatorName = creatorInfo.StaffDislpayName;

        var index = _configObjectReleases.IndexOf(configObjectRelease);
        _prevReleaseHistory = _configObjectReleases.Skip(index + 1).FirstOrDefault() ?? new ConfigObjectReleaseModel();
        configObjectRelease.IsActive = true;
        _configObjectReleases.ForEach(release =>
        {
            if (release.Id != configObjectRelease.Id)
            {
                release.IsActive = false;
            }
        });

        if (_releaseHistory.FormatLabelCode.Trim().ToLower() == "properties")
        {
            ReleaseHistoryTabIndexChanged(_releaseTabText);
        }
    }

    private void ReleaseHistoryTabIndexChanged(string tabText)
    {
        _releaseTabText = tabText;
        if (_releaseTabText == T("Changed configuration"))
        {
            if (_prevReleaseHistory.ConfigObjectProperties.Count == 0)
            {
                _changedProperties = _selectReleaseHistory.ConfigObjectProperties.Select(release =>
                    new ConfigObjectPropertyModel
                    {
                        Key = release.Key,
                        Value = release.Value,
                        TempValue = "",
                        IsAdded = true
                    }).ToList();
            }
            else
            {
                var current = _selectReleaseHistory.ConfigObjectProperties;
                var prev = _prevReleaseHistory.ConfigObjectProperties;

                var added = current.ExceptBy(prev.Select(content => content.Key), content => content.Key)
                    .Select(content => new ConfigObjectPropertyModel
                    {
                        IsAdded = true,
                        Key = content.Key,
                        Value = content.Value
                    })
                    .ToList();
                var deleted = prev.ExceptBy(current.Select(content => content.Key), content => content.Key)
                    .Select(content => new ConfigObjectPropertyModel
                    {
                        IsDeleted = true,
                        Key = content.Key,
                        TempValue = content.Value
                    })
                    .ToList();
                var intersectAndEdited = current
                    .IntersectBy(prev.Select(content => content.Key), content => content.Key)
                    .ToList();
                var intersect = current.IntersectBy(
                    prev.Select(content => new { content.Key, content.Value }),
                    content => new { content.Key, content.Value });
                intersectAndEdited.RemoveAll(content => intersect.Select(content => content.Key).Contains(content.Key));
                var edited = intersectAndEdited
                    .Select(content => new ConfigObjectPropertyModel
                    {
                        IsEdited = true,
                        Key = content.Key,
                        Value = current.FirstOrDefault(current => current.Key == content.Key)?.Value ?? "",
                        TempValue = prev.FirstOrDefault(rollback => rollback.Key == content.Key)?.Value ?? ""
                    }).ToList();

                _changedProperties = added
                    .UnionBy(deleted, prop => prop.Key)
                    .UnionBy(edited, prop => prop.Key).ToList();
            }
        }
    }

    private async Task RollbackToAsync()
    {
        var current = _configObjectReleases.First();
        if (_selectReleaseHistory.IsInvalid)
        {
            await PopupService.EnqueueSnackbarAsync(T("This version is obsolete and cannot be rolled back"), AlertTypes.Error);
            return;
        }

        if (current.ToReleaseId == _selectReleaseHistory.Id || _selectReleaseHistory.Id == current.Id ||
            current.Version == _selectReleaseHistory.Version)
        {
            await PopupService.EnqueueSnackbarAsync(
                T("This version is the same as the current version and cannot be rolled back"), AlertTypes.Error);
            return;
        }

        _releaseHistory.ConfigObjectReleases.Clear();
        _releaseHistory.ConfigObjectReleases.Add(current);
        _releaseHistory.ConfigObjectReleases.Add(_selectReleaseHistory);
        ShowRollbackModal();
        _handleRollbackOnClickAfter = async () =>
        {
            _releaseHistory = await ConfigObjectCaller.GetReleaseHistoryAsync(current.ConfigObjectId);
            _configObjectReleases = _releaseHistory.ConfigObjectReleases.OrderByDescending(release => release.Id)
                .Adapt<List<ConfigObjectReleaseModel>>();
            if (_releaseHistory.FormatLabelCode.Trim().ToLower() == "properties")
            {
                _configObjectReleases.ForEach(release =>
                {
                    release.ConfigObjectProperties =
                        JsonSerializer.Deserialize<List<ConfigObjectPropertyModel>>(release.Content) ??
                        new List<ConfigObjectPropertyModel>();
                });
            }

            if (OnSubmitAfter.HasDelegate)
            {
                await OnSubmitAfter.InvokeAsync();
            }

            Value = false;
            StateHasChanged();
        };
    }

    private void ShowRollbackModal()
    {
        _formatLabelCode = _releaseHistory.FormatLabelCode;
        _currentConfigObjectRelease = _releaseHistory.ConfigObjectReleases.FirstOrDefault();
        _rollbackConfigObjectRelease = _releaseHistory.ConfigObjectReleases.LastOrDefault();
        _showRollbackModal = true;
    }

    private async Task RollbackAsync(ConfigObjectWithReleaseHistoryDto releaseHistory)
    {
        await ConfigObjectCaller.RollbackAsync(new RollbackConfigObjectReleaseDto
        {
            ConfigObjectId = releaseHistory.Id,
            RollbackToReleaseId = releaseHistory.ConfigObjectReleases.Last().Id
        });
        _showRollbackModal = false;
        await PopupService.EnqueueSnackbarAsync(T("Rollback succeeded"), AlertTypes.Success);
    }
}
