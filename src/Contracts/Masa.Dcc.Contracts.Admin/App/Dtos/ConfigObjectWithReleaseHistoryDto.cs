using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masa.Dcc.Contracts.Admin.App.Dtos
{
    public class ConfigObjectWithReleaseHistoryDto : ConfigObjectDto
    {
        public List<ConfigObjectReleaseDto> ConfigObjectReleases { get; set; } = new();
    }
}
