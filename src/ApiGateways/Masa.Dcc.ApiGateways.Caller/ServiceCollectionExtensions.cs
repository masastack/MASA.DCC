// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDccApiGateways(this IServiceCollection services, Action<DccApiGatewayOptions>? configure = null)
    {
        var options = new DccApiGatewayOptions("http://localhost:6196/");

        configure?.Invoke(options);
        services.AddSingleton(options);
        services.AddAutoRegistrationCaller(Assembly.Load("Masa.Dcc.ApiGateways.Caller"));

        return services;
    }
}
