// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Application.Label
{
    public class CommandHandler
    {
        private readonly ILabelRepository _labelRepository;
        private readonly LabelDomainService _labelDomainService;

        public CommandHandler(ILabelRepository labelRepository, LabelDomainService labelDomainService)
        {
            _labelRepository = labelRepository;
            _labelDomainService = labelDomainService;
        }

        [EventHandler]
        public async Task AddLabelAsync(AddLabelCommand command)
        {
            await _labelDomainService.AddLabelAsync(command.LabelDto);
        }

        [EventHandler]
        public async Task UpdateLabelAsync(UpdateLabelCommand command)
        {
            await _labelDomainService.UpdateLabelAsync(command.LabelDto);
        }

        [EventHandler]
        public async Task RemoveLabelAsync(RemoveLabelCommand command)
        {
            await _labelDomainService.RemoveLaeblAsync(command.TypeCode);
        }
    }
}
