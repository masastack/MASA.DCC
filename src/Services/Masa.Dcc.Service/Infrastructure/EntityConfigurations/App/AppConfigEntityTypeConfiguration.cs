using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Masa.Dcc.Service.Admin.Infrastructure.EntityConfigurations.App
{
    public class AppConfigEntityTypeConfiguration : IEntityTypeConfiguration<AppConfigObject>
    {
        public void Configure(EntityTypeBuilder<AppConfigObject> builder)
        {
        }
    }
}
