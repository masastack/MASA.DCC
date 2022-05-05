using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masa.Dcc.Contracts.Admin.App.Dtos
{
    public class RollbackConfigObjectReleaseDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int ConfigObjectId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int RollbackToReleaseId { get; set; }
    }
}
