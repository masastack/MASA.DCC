using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masa.Dcc.Contracts.Admin.App.Dtos
{
    public class PublishReleaseDto
    {
        public string Content { get; set; } = null!;

        public string FormatLabelName { get; set; } = null!;

        public ConfigObjectType ConfigObjectType { get; set; }
    }
}
