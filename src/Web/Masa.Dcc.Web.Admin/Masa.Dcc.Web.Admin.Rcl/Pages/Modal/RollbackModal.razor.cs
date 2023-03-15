// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Web.Admin.Rcl.Pages.Modal
{
    public partial class RollbackModal
    {
        private bool _show;
        [Parameter]
        public bool Value
        {
            get => _show;
            set
            {
                _show = value;
                if (_show && FormatLabelCode.ToLower() == "properties")
                {
                    var current = JsonSerializer.Deserialize<List<ConfigObjectPropertyModel>>(CurrentConfigObjectRelease.Content) ?? new();
                    var rollback = JsonSerializer.Deserialize<List<ConfigObjectPropertyModel>>(RollbackConfigObjectRelease.Content) ?? new();

                    var added = rollback.ExceptBy(current.Select(content => content.Key), content => content.Key)
                        .Select(content => new ConfigObjectPropertyModel
                        { IsAdded = true, Key = content.Key, TempValue = content.Value });
                    var deleted = current.ExceptBy(rollback.Select(content => content.Key), content => content.Key)
                        .Select(content => new ConfigObjectPropertyModel
                        { IsDeleted = true, Key = content.Key, Value = content.Value });
                    var intersectAndEdited = current.IntersectBy(rollback.Select(content => content.Key), content => content.Key)
                        .ToList();
                    var intersect = current.IntersectBy(
                        rollback.Select(content => new { content.Key, content.Value }), content => new { content.Key, content.Value });
                    intersectAndEdited.RemoveAll(content => intersect.Select(content => content.Key).Contains(content.Key));
                    var edited = intersectAndEdited
                        .Select(content => new ConfigObjectPropertyModel
                        {
                            IsEdited = true,
                            Key = content.Key,
                            Value = current.FirstOrDefault(current => current.Key == content.Key)?.Value ?? "",
                            TempValue = rollback.FirstOrDefault(rollback => rollback.Key == content.Key)?.Value ?? ""
                        });

                    _properties = added.Union(deleted).Union(edited).ToList();
                }
            }
        }

        [Parameter]
        public EventCallback<bool> ValueChanged { get; set; }
        
        [Parameter]
        public string ModalKey { get; set; }
        
        [Parameter]
        public EventCallback OnClick { get; set; }

        [Parameter]
        public EventCallback OnClickAfter { get; set; }

        [Parameter]
        public string FormatLabelCode { get; set; } = null!;

        [Parameter]
        public ConfigObjectReleaseDto CurrentConfigObjectRelease { get; set; } = null!;

        [Parameter]
        public ConfigObjectReleaseDto RollbackConfigObjectRelease { get; set; } = null!;

        private List<ConfigObjectPropertyModel> _properties = new();
        private readonly List<DataTableHeader<ConfigObjectPropertyModel>> _headers = new()
        {
            new (){ Text= "State", Value= nameof(ConfigObjectPropertyModel.IsPublished)},
            new (){ Text= "Key", Value= nameof(ConfigObjectPropertyModel.Key)},
            new (){ Text= "Value before rollback", Value= nameof(ConfigObjectPropertyModel.Value)},
            new (){ Text= "Value after rollback", Value= nameof(ConfigObjectPropertyModel.TempValue)}
        };
        
        public RollbackModal()
        {
            ModalKey = $"rollbackModal-{this._show}";
        }

        private async Task HandOnClickAsync()
        {
            if (OnClick.HasDelegate)
            {
                await OnClick.InvokeAsync();
            }
            if (OnClickAfter.HasDelegate)
            {
                await OnClickAfter.InvokeAsync();
            }
        }
    }
}
