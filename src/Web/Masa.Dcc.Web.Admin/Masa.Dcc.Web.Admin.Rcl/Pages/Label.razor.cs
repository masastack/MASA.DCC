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
        private Func<string, StringBoolean> _requiredRule = value => !string.IsNullOrEmpty(value) ? true : "Required";
        private Func<string, StringBoolean> _counterRule = value => (value.Length <= 50 && value.Length >= 2) ? true : "length range is [2-50]";
        private Func<string, StringBoolean> _strRule = value =>
        {
            Regex regex = new Regex(@"^[\u4E00-\u9FA5A-Za-z0-9_.-]+$");
            if (!regex.IsMatch(value))
            {
                return "Special symbols are not allowed";
            }
            else
            {
                return true;
            }
        };

        private IEnumerable<Func<string, StringBoolean>> LabelValueRules => new List<Func<string, StringBoolean>>
        {
            _requiredRule,
            _counterRule,
            _strRule
        };

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

            var users = await AuthClient.UserService.GetUserPortraitsAsync(labels.Select(l => l.Modifier).ToArray());
            foreach (var label in labels)
            {
                label.ModifierName = users.FirstOrDefault(user => user.Id == label.Modifier)?.Name ?? "";
            }

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

            foreach (var labelValue in _labelModal.Data.LabelValues)
            {
                foreach (var rule in LabelValueRules)
                {
                    var nameRule = rule.Invoke(labelValue.Name).Value;
                    if (nameRule is string)
                    {
                        return;
                    }

                    var codeRule = rule.Invoke(labelValue.Code).Value;
                    if (codeRule is string)
                    {
                        return;
                    }
                }

                if (_labelModal.Data.LabelValues.Count(l => l.Code == labelValue.Code) > 1
                    || _labelModal.Data.LabelValues.Count(l => l.Name == labelValue.Name) > 1)
                {
                    await PopupService.ToastErrorAsync("标签Code和标签Name不允许重复");
                    return;
                }
            }

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
            _labelModal.Visible = value;
            if (!value)
            {
                _labelModal.Hide();
                _labelValues.Clear();
            }
        }

        private async Task RemoveLabelAsync(string typeCode)
        {
            await PopupService.ConfirmAsync("删除标签", $"您确定要删除{typeCode}标签吗？", async args =>
            {
                await LabelCaller.RemoveAsync(typeCode);

                await PopupService.ToastSuccessAsync("操作成功");

                LabelModalValueChanged(false);
                _labels = await GetListAsync();
            });
        }
    }
}
