// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using Masa.BuildingBlocks.Globalization.I18n;

namespace Masa.Dcc.Service.Admin.Domain.Label.Services
{
    public class LabelDomainService : DomainService
    {
        private readonly ILabelRepository _labelRepository;
        private readonly IDistributedCacheClient _distributedCacheClient;
        private readonly II18n<DefaultResource> _il8n;

        public LabelDomainService(IDomainEventBus eventBus,
            ILabelRepository labelRepository,
            IDistributedCacheClient distributedCacheClient,
             II18n<DefaultResource> i18N) : base(eventBus)
        {
            _labelRepository = labelRepository;
            _distributedCacheClient = distributedCacheClient;
            _il8n = i18N;
        }

        public async Task AddLabelAsync(UpdateLabelDto labelDto)
        {
            if (!labelDto.LabelValues.Any())
            {
                throw new UserFriendlyException("Label values can not be null");
            }
            else
            {
                var label = await _labelRepository.FindAsync(x => x.TypeCode == labelDto.TypeCode);
                if (label is not null)
                {
                    throw new UserFriendlyException(_il8n.T("Duplicate label type code"));
                }

                List<Aggregates.Label> labels = new();
                foreach (var item in labelDto.LabelValues)
                {
                    labels.Add(new Aggregates.Label(item.Code,
                        item.Name,
                        labelDto.TypeCode,
                        labelDto.TypeName,
                        labelDto.Description));
                }

                await _labelRepository.AddRangeAsync(labels);
                await _distributedCacheClient.SetAsync(labelDto.TypeCode, labels.Adapt<List<LabelModel>>());
            }
        }

        public async Task UpdateLabelAsync(UpdateLabelDto labelDto)
        {
            var labelEntities = await _labelRepository.GetListAsync(l => l.TypeCode.Equals(labelDto.TypeCode));
            if (labelEntities.Any())
            {
                await _labelRepository.RemoveRangeAsync(labelEntities);

                List<Aggregates.Label> labels = new();
                var labelEntity = labelEntities.First();
                foreach (var item in labelDto.LabelValues)
                {
                    labels.Add(new Aggregates.Label(item.Code,
                        item.Name,
                        labelDto.TypeCode,
                        labelDto.TypeName,
                        labelEntity.Creator,
                        labelEntity.CreationTime,
                        labelDto.Description));
                }

                await _labelRepository.AddRangeAsync(labels);
                await _distributedCacheClient.SetAsync(labelDto.TypeCode, labels.Adapt<List<LabelModel>>());
            }
        }

        public async Task RemoveLaeblAsync(string typeCode)
        {
            var labels = await _labelRepository.GetListAsync(label => label.TypeCode == typeCode) ?? throw new UserFriendlyException("Label does not exist");
            await _labelRepository.RemoveRangeAsync(labels);

            await _distributedCacheClient.RemoveAsync<List<LabelModel>>(typeCode);
        }
    }
}
