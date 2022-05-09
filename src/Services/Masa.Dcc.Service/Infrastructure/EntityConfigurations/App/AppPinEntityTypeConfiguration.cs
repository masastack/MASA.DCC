// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Masa.Dcc.Service.Admin.Infrastructure.EntityConfigurations.App
{
    public class AppPinEntityTypeConfiguration : IEntityTypeConfiguration<AppPin>
    {
        public void Configure(EntityTypeBuilder<AppPin> builder)
        {
        }
    }
}
