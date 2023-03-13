// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Web.Admin.Rcl.Pages
{
    public partial class Label
    {
        [Inject]
        public LabelCaller LabelCaller { get; set; } = default!;

        [Inject]
        public IPopupService PopupService { get; set; } = default!;

        private List<LabelDto> _labels = new();
        private string _typeName = "";
        private readonly DataModal<UpdateLabelModel> _labelModal = new();

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _labels = await GetListAsync();

                StateHasChanged();
            }
        }

        private async Task<List<LabelDto>> GetListAsync()
        {
            var labels = await LabelCaller.GetListAsync();

            foreach (var label in labels)
            {
                var user = await AuthClient.UserService.GetByIdAsync(label.Modifier) ?? new();
                label.ModifierName = user.StaffDislpayName;
            }

            return labels.OrderByDescending(label => label.ModificationTime).ToList();
        }

        private async Task SearchAsync(KeyboardEventArgs args)
        {
            if (args.Key == "Enter")
            {
                if (!string.IsNullOrEmpty(_typeName))
                {
                    _labels = _labels.Where(l => l.TypeName.Trim().Contains(_typeName.Trim())).ToList();
                }
                else
                {
                    _labels = await GetListAsync();
                }
            }
        }

        private async Task ShowLabelModal(LabelDto? labelDto = null)
        {
            if (labelDto == null)
            {
                _labelModal.Data.LabelValues = new() { new(0) };
                _labelModal.Show();
            }
            else
            {
                var labels = await LabelCaller.GetLabelsByTypeCodeAsync(labelDto.TypeCode);
                for (int i = 0; i < labels.Count; i++)
                {
                    var label = labels[i];
                    _labelModal.Data.LabelValues.Add(new LabelValueModel(label.Name, label.Code, i));
                }
                _labelModal.Show(new UpdateLabelModel
                {
                    TypeCode = labelDto.TypeCode,
                    TypeName = labelDto.TypeName,
                    Description = labelDto.Description,
                    LabelValues = _labelModal.Data.LabelValues
                });
            }
        }

        private void AddLabelValue(int index)
        {
            var label = _labelModal.Data.LabelValues.FirstOrDefault(e => e.Index == index);
            if (label != null)
            {
                var newIndex = _labelModal.Data.LabelValues.IndexOf(label) + 1;
                _labelModal.Data.LabelValues.Insert(newIndex, new LabelValueModel(_labelModal.Data.LabelValues.Count));
            }
        }

        private void RemoveLabelValue(int index)
        {
            if (_labelModal.Data.LabelValues.Count > 1)
            {
                var label = _labelModal.Data.LabelValues.FirstOrDefault(e => e.Index == index);
                if (label != null)
                {
                    _labelModal.Data.LabelValues.Remove(label);
                }
            }
        }

        private async Task SubmitLabelAsync(FormContext context)
        {
            if (_labelModal.Data.LabelValues.GroupBy(l => l.Code).Any(l => l.Count() > 1) || _labelModal.Data.LabelValues.GroupBy(l => l.Name).Any(l => l.Count() > 1))
            {
                await PopupService.EnqueueSnackbarAsync(T("The label value Code and label value Name cannot be duplicate"), AlertTypes.Error);
                return;
            }

            if (context.Validate())
            {
                var dto = _labelModal.Data.Adapt<UpdateLabelDto>();
                if (_labelModal.HasValue)
                {
                    await LabelCaller.UpdateAsync(dto);
                    await PopupService.EnqueueSnackbarAsync(T("Edit succeeded"), AlertTypes.Success);
                }
                else
                {
                    await LabelCaller.AddAsync(dto);
                    await PopupService.EnqueueSnackbarAsync(T("Add succeeded"), AlertTypes.Success);
                }

                _labels = await GetListAsync();
                LabelModalValueChanged(false);
            }
        }

        private void LabelModalValueChanged(bool value)
        {
            _labelModal.Visible = value;
            if (!value)
            {
                _labelModal.Hide();
                _labelModal.Data.LabelValues.Clear();
            }
        }

        private async Task RemoveLabelAsync(UpdateLabelModel label)
        {
            var result = await PopupService.ConfirmAsync(
                T("Delete label"),
                T("Are you sure you want to delete the label \"{typeName}\"?").Replace("{typeName}", label.TypeName),
                AlertTypes.Error);

            if (result)
            {
                await LabelCaller.RemoveAsync(label.TypeCode);
                await PopupService.EnqueueSnackbarAsync(T("Delete succeeded"), AlertTypes.Success);
                LabelModalValueChanged(false);
                _labels = await GetListAsync();
            }
        }
    }
}
