// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Services
{
    public class LabelService : ServiceBase
    {
        public LabelService(IServiceCollection services) : base(services)
        {
            App.MapGet("api/v1/{typeCode}/labels", GetLabelsByTypeCodeAsync);
        }

        public async Task<List<LabelDto>> GetLabelsByTypeCodeAsync(IEventBus eventBus, string typeCode)
        {
            var query = new LabelsQuery(typeCode);
            await eventBus.PublishAsync(query);

            return query.Result;
        }
    }
}
