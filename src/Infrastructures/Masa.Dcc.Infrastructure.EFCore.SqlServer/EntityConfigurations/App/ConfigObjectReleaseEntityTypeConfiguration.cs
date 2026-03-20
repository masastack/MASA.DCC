// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Infrastructure.EFCore.SqlServer.EntityConfigurations.App;

internal class ConfigObjectReleaseEntityTypeConfiguration : IEntityTypeConfiguration<ConfigObjectRelease>
{
    public void Configure(EntityTypeBuilder<ConfigObjectRelease> builder)
    {
        builder.Property(m => m.Type).HasColumnType("tinyint");
        builder.Property(m => m.Version).HasColumnType("varchar(20)");
        builder.Property(m => m.Name).HasColumnType("nvarchar(100)");
        builder.Property(m => m.Comment).HasColumnType("nvarchar(500)");
        builder.Property(m => m.Content).HasColumnType("ntext");
    }
}
