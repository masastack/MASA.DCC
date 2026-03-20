// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Infrastructure.EFCore;

public class PmDbConText : MasaDbContext<PmDbConText>
{
    public PmDbConText(MasaDbContextOptions<PmDbConText> options) : base(options)
    {
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    public DbSet<MASA.PM.Infrastructure.Domain.Shared.Entities.Environment> Environments { get; }

    public DbSet<EnvironmentCluster> EnvironmentClusters { get; set; } = default!;

    public DbSet<Cluster> Clusters { get; set; } = default!;

    public DbSet<EnvironmentClusterProject> EnvironmentClusterProjects { get; set; } = default!;

    public DbSet<Project> Projects { get; set; } = default!;

    public DbSet<EnvironmentClusterProjectApp> EnvironmentClusterProjectApps { get; set; } = default!;

    public DbSet<App> Apps { get; set; } = default!;

    public DbSet<EnvironmentProjectTeam> EnvironmentProjectTeams { get; set; } = default!;
}
