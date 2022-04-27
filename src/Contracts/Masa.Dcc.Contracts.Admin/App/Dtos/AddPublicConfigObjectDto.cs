using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masa.Dcc.Contracts.Admin.App.Dtos
{
    public class AddPublicConfigObjectDto
    {
        public int PublicConfigId { get; set; }

        public int ConfigObjectId { get; set; }

        public int EnvironmentClusterId { get; set; }
    }
}
