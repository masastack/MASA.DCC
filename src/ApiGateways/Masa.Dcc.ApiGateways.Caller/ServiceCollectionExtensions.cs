// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDccApiGateways(this IServiceCollection services, Action<DccApiGatewayOptions>? configure = null)
    {
        var options = new DccApiGatewayOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);
        services.AddStackCaller(Assembly.Load("Masa.Dcc.ApiGateways.Caller"), jwtTokenValidatorOptions =>
        {
            jwtTokenValidatorOptions.AuthorityEndpoint = options.AuthorityEndpoint;
        }, clientRefreshTokenOptions =>
        {
            clientRefreshTokenOptions.ClientId = options.ClientId;
            clientRefreshTokenOptions.ClientSecret = options.ClientSecret;
        });
        return services;
    }
}
