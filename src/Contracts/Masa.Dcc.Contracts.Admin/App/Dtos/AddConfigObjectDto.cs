using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masa.Dcc.Contracts.Admin.App.Dtos
{
    public class AddConfigObjectDto
    {
        private string _name = "";

        [Required]
        [RegularExpression(@"^[\u4E00-\u9FA5A-Za-z0-9`~!$%^&*()_\-+=<>?:{}|,.\/;""·~！￥%……&*（）——\-+={}|《》？：“”【】、；]+$", ErrorMessage = "Special symbols are not allowed")]
        [StringLength(50, MinimumLength = 2)]
        public string Name { get => _name; set => _name = value.Trim(); }

        [Required]
        [Range(1, int.MaxValue)]
        public int FormatLabelId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int TypeLabelId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int PublicConfigId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int EnvironmentClusterId { get; set; }

        public AddConfigObjectDto()
        {
        }

        public AddConfigObjectDto(string name, int formatLabelId, int typeLabelId, int publicConfigId, int environmentClusterId)
        {
            _name = name;
            FormatLabelId = formatLabelId;
            TypeLabelId = typeLabelId;
            PublicConfigId = publicConfigId;
            EnvironmentClusterId = environmentClusterId;
        }
    }
}
