// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace Masa.Dcc.Caller;

public abstract class DccHttpClientCallerBase : HttpClientCallerBase
{
    protected DccHttpClientCallerBase(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override IHttpClientBuilder UseHttpClient()
    {
        string baseApi = AppSettings.Get("ServiceBaseUrl");
        return base.CallerOptions.UseHttpClient(delegate (MasaHttpClientBuilder opt)
        {
            opt.Name = Name;
            opt.Configure = delegate (HttpClient client)
            {
                client.BaseAddress = new Uri(baseApi);
            };
        });
    }
}
