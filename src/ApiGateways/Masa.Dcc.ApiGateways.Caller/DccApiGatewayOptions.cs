// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.ApiGateways.Caller
{
    public class DccApiGatewayOptions
    {
        public string DccServiceAddress { get; set; }

        public DccApiGatewayOptions(string url)
        {
            DccServiceAddress = url;
        }
    }
}
