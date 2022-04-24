using Masa.Dcc.Service.Admin.Domain.Label.Aggregates;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Masa.Dcc.Service.Admin.Infrastructure.EntityConfigurations.App
{
    public class LabelEntityTypeConfiguration : IEntityTypeConfiguration<Label>
    {
        public void Configure(EntityTypeBuilder<Label> builder)
        {
        }
    }
}
