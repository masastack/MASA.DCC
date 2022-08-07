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
        private DataModal<UpdateLabelDto> _labelModal = new();
        private List<LabelValueModel> _labelValues = new();

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

            return labels;
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
                _labelValues = new() { new(0) };
                _labelModal.Show();
            }
            else
            {
                var labels = await LabelCaller.GetLabelsByTypeCodeAsync(labelDto.TypeCode);
                for (int i = 0; i < labels.Count; i++)
                {
                    var label = labels[i];
                    _labelValues.Add(new LabelValueModel(label.Name, label.Code, i));
                }
                _labelModal.Show(labelDto.Adapt<UpdateLabelDto>());
            }
        }

        private void AddLabelValue(int index)
        {
            var label = _labelValues.FirstOrDefault(e => e.Index == index);
            if (label != null)
            {
                var newIndex = _labelValues.IndexOf(label) + 1;
                _labelValues.Insert(newIndex, new LabelValueModel(_labelValues.Count));
            }
        }

        private void RemoveLabelValue(int index)
        {
            if (_labelValues.Count > 1)
            {
                var label = _labelValues.FirstOrDefault(e => e.Index == index);
                if (label != null)
                {
                    _labelValues.Remove(label);
                }
            }
        }

        private async Task SubmitLabelAsync(EditContext context)
        {
            _labelModal.Data.LabelValues = _labelValues
                .Where(l => !string.IsNullOrWhiteSpace(l.Name) && !string.IsNullOrWhiteSpace(l.Code))
                .Select(l => new LabelValueDto { Code = l.Code, Name = l.Name })
                .ToList();

            if (context.Validate())
            {
                if (!_labelModal.Data.LabelValues.Any())
                {
                    await PopupService.ToastErrorAsync("标签值不允许为空");
                    return;
                }

                if (_labelModal.HasValue)
                {
                    await LabelCaller.UpdateAsync(_labelModal.Data);
                }
                else
                {
                    await LabelCaller.AddAsync(_labelModal.Data);
                }

                await PopupService.ToastSuccessAsync("操作成功");
                _labels = await GetListAsync();
                LabelModalValueChanged(false);
            }
        }

        private void LabelModalValueChanged(bool value)
        {
            if (!value)
            {
                _labelModal.Hide();
                _labelValues.Clear();
            }
        }

        private async Task RemoveLabelAsync(string typeCode)
        {
            await LabelCaller.RemoveAsync(typeCode);

            await PopupService.ToastSuccessAsync("操作成功");

            LabelModalValueChanged(false);
            _labels = await GetListAsync();
        }
    }
}
