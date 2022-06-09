// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Application.Label
{
    public class CommandHandler
    {
        private readonly ILabelRepository _labelRepository;

        public CommandHandler(ILabelRepository labelRepository)
        {
            _labelRepository = labelRepository;
        }

        [EventHandler]
        public async Task AddLabelAsync(AddLabelCommand command)
        {
            var labelDto = command.LabelDto;
            List<Domain.Label.Aggregates.Label> labels = new();
            foreach (var item in labelDto.LabelValues)
            {
                labels.Add(new Domain.Label.Aggregates.Label(item.Code,
                    item.Name,
                    labelDto.TypeCode,
                    labelDto.TypeName,
                    labelDto.Description));
            }

            await _labelRepository.AddRangeAsync(labels);
        }

        [EventHandler]
        public async Task UpdateLabelAsync(UpdateLabelCommand command)
        {
            var labelDto = command.LabelDto;
            var labelEntities = await _labelRepository.GetListAsync(l => l.TypeCode.Equals(labelDto.TypeCode));
            if (labelEntities.Any())
            {
                await _labelRepository.RemoveRangeAsync(labelEntities);

                List<Domain.Label.Aggregates.Label> labels = new();
                var labelEntity = labelEntities.First();
                foreach (var item in labelDto.LabelValues)
                {
                    labels.Add(new Domain.Label.Aggregates.Label(item.Code,
                        item.Name,
                        labelDto.TypeCode,
                        labelDto.TypeName,
                        labelEntity.Creator,
                        labelEntity.CreationTime,
                        labelDto.Description));
                }

                await _labelRepository.AddRangeAsync(labels);
            }
        }
    }
}
