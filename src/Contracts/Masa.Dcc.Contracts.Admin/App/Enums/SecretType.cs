using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masa.Dcc.Contracts.Admin.App.Enums
{
    public enum SecretType
    {
        [Description("read only")]
        ReadOnly = 1,

        [Description("read and write")]
        ReadAndWrite
    }
}
