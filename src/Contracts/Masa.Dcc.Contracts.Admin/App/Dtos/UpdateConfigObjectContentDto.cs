﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masa.Dcc.Contracts.Admin.App.Dtos
{
    public class UpdateConfigObjectContentDto
    {
        public int ConfigObjectId { get; set; }

        public string Content { get; set; } = "";
    }
}
