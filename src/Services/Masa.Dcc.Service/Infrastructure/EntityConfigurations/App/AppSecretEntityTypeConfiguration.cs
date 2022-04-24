using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Masa.Dcc.Service.Admin.Infrastructure.EntityConfigurations.App
{
    public class AppSecretEntityTypeConfiguration : IEntityTypeConfiguration<AppSecret>
    {
        public void Configure(EntityTypeBuilder<AppSecret> builder)
        {
        }
    }
}
