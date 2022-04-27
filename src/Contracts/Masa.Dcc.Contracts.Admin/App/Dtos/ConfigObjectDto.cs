using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masa.Dcc.Contracts.Admin.App.Dtos
{
    public class ConfigObjectDto : BaseDto
    {
        public string Name { get; set; } = "";

        public string FormatName { get; set; } = "";

        public ConfigObjectType Type { get; set; }

        public int RelationConfigObjectId { get; set; }

        public string Content { get; set; } = "";

        public string TempContent { get; set; } = "";
    }
}
