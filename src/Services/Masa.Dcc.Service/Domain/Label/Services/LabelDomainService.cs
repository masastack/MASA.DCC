// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using Masa.Utils.Caching.Core.Interfaces;

namespace Masa.Dcc.Service.Admin.Domain.Label.Services
{
    public class LabelDomainService : DomainService
    {
        private readonly ILabelRepository _labelRepository;
        private readonly IDistributedCacheClient _distributedCacheClient;

        public LabelDomainService(IDomainEventBus eventBus,
            ILabelRepository labelRepository,
            IDistributedCacheClient memoryCacheClient) : base(eventBus)
        {
            _labelRepository = labelRepository;
            _distributedCacheClient = memoryCacheClient;
        }

        public async Task AddLabelAsync(UpdateLabelDto labelDto)
        {
            if (!labelDto.LabelValues.Any())
            {
                throw new UserFriendlyException("");
            }
            else
            {
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
                await _distributedCacheClient.SetAsync(labelDto.TypeCode, labels.Adapt<List<LabelDto>>());
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
                await _distributedCacheClient.SetAsync(labelDto.TypeCode, labels.Adapt<List<LabelDto>>());
            }
        }
    }
}
