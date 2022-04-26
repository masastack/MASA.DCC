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
