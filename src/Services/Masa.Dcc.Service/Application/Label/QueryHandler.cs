// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Application.Label;

public class QueryHandler
{
    private readonly ILabelRepository _labelRepository;

    public QueryHandler(ILabelRepository labelRepository)
    {
        _labelRepository = labelRepository;
    }

    [EventHandler]
    public async Task GetLabelsAsync(LabelsQuery labelsQuery)
    {
        IEnumerable<Dcc.Infrastructure.Domain.Label.Aggregates.Label> labels = new List<Dcc.Infrastructure.Domain.Label.Aggregates.Label>();

        if (string.IsNullOrEmpty(labelsQuery.TypeCode))
            labels = await _labelRepository.GetListAsync();
        else
            labels = await _labelRepository.GetListAsync(label => label.TypeCode == labelsQuery.TypeCode);

        labelsQuery.Result = labels.Adapt<List<LabelDto>>();
    }
}
