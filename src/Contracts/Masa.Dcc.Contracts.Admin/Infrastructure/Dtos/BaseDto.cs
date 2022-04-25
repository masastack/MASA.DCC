using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masa.Dcc.Contracts.Admin.Infrastructure.Dtos
{
    public class BaseDto
    {
        public Guid Creator { get; set; }

        public DateTime CreationTime { get; set; }

        public Guid Modifier { get; set; }

        public DateTime ModificationTime { get; set; }
    }
}
