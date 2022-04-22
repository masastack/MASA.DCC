namespace Masa.Dcc.Service.Admin.Domain.Aggregates.App
{
    [Table("AppSecrets")]
    public class AppSecret : BaseEntity<int, Guid>
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int AppId { get; }

        [Required]
        [Range(1, int.MaxValue)]
        public int EnvironmentId { get; }

        [Required]
        [Range(1, int.MaxValue)]
        public SecretType Type { get; set; }

        [Required]
        public Guid EncryptionSecret { get; }

        [Required]
        public Guid Secret { get; }
    }
}
