// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Infrastructure.EFCore.PostgreSql.EntityConfigurations.App;

internal class ConfigObjectEntityTypeConfiguration : IEntityTypeConfiguration<ConfigObject>
{
    public void Configure(EntityTypeBuilder<ConfigObject> builder)
    {
        builder.Property(m => m.Content).HasColumnType("text");
        builder.Property(m => m.TempContent).HasColumnType("text");
    }
}
