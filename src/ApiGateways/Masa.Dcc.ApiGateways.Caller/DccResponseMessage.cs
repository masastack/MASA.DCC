﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.ApiGateways.Caller
{
    internal class DccResponseMessage : JsonResponseMessage
    {
        public DccResponseMessage(ILoggerFactory? loggerFactory = null) : base(default, loggerFactory)
        {
        }

        public override async Task ProcessCustomException(HttpResponseMessage response)
        {
            switch (response.StatusCode)
            {
                case (HttpStatusCode)293:
                    throw new UserFriendlyException(await response.Content.ReadAsStringAsync());
                default:
                    break;
            }
        }
    }
}
