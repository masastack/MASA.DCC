// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Caller;

public abstract class DccHttpClientCallerBase : HttpClientCallerBase
{
    private readonly TokenProvider _tokenProvider;

    protected DccHttpClientCallerBase(
        IServiceProvider serviceProvider,
        TokenProvider tokenProvider,
        DccApiGatewayOptions options) : base(serviceProvider)
    {
        _tokenProvider = tokenProvider;
        BaseAddress = options.DccServiceAddress;
    }

    protected override string BaseAddress { get; set; }

    protected override async Task ConfigHttpRequestMessageAsync(HttpRequestMessage requestMessage)
    {
        if (!string.IsNullOrWhiteSpace(_tokenProvider.AccessToken))
        {
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tokenProvider.AccessToken);
        }

        await base.ConfigHttpRequestMessageAsync(requestMessage);
    }
}
