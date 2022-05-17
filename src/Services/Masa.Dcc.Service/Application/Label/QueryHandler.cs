// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Application.Label
{
    public class QueryHandler
    {
        private readonly ILabelRepository _labelRepository;

        public QueryHandler(ILabelRepository labelRepository)
        {
            _labelRepository = labelRepository;
        }

        [EventHandler]
        public async Task GetLabelsByTypeCodeAsync(LabelsQuery labelsQuery)
        {
            var result = await _labelRepository.GetListAsync(label => label.TypeCode == labelsQuery.TypeCode);

            labelsQuery.Result = result.Adapt<List<LabelDto>>();
        }
    }
}
