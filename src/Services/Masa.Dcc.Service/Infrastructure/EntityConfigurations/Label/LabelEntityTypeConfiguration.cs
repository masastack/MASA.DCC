// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Masa.Dcc.Service.Admin.Infrastructure.EntityConfigurations.Label
{
    public class LabelEntityTypeConfiguration : IEntityTypeConfiguration<Domain.Label.Aggregates.Label>
    {
        public void Configure(EntityTypeBuilder<Domain.Label.Aggregates.Label> builder)
        {
        }
    }
}
