using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masa.Dcc.Contracts.Admin.App.Dtos
{
    public class ConfigObjectDto
    {
        public string Name { get; set; } = "";

        public string FormatName { get; set; } = "";

        public string TypeName { get; set; } = "";

        public int RelationConfigObjectId { get; set; }

        public ConfigObjectMainDto ConfigObjectMain { get; set; } = new();
    }
}
