// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Caller;

public abstract class DccHttpClientCallerBase : HttpClientCallerBase
{
    protected DccHttpClientCallerBase(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override string BaseAddress { get; set; } = AppSettings.Get("ServiceBaseUrl");
}
