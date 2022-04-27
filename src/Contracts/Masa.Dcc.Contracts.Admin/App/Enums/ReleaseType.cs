using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masa.Dcc.Contracts.Admin.App.Enums
{
    public enum ReleaseType
    {
        [Description("Main release")]
        MainRelease = 1,

        [Description("Gray release")]
        GrayRelease,
    }
}
