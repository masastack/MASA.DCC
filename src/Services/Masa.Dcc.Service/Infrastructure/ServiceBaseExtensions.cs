// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using Masa.BuildingBlocks.Data;
using Masa.Contrib.Service.MinimalAPIs;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Builder;

public static class ServiceBaseExtensions
{
    public static RouteHandlerBuilder MapGet(this ServiceBase service, string pattern, Delegate handler) => MapMethods(service, HttpMethods.Get, pattern, handler);

    public static RouteHandlerBuilder MapPost(this ServiceBase service, string pattern, Delegate handler) => MapMethods(service, HttpMethods.Post, pattern, handler);

    public static RouteHandlerBuilder MapPut(this ServiceBase service, string pattern, Delegate handler) => MapMethods(service, HttpMethods.Put, pattern, handler);

    public static RouteHandlerBuilder MapDelete(this ServiceBase service, string pattern, Delegate handler) => MapMethods(service, HttpMethods.Delete, pattern, handler);

    private static RouteHandlerBuilder MapMethods(this ServiceBase service, string method, string pattern, Delegate handler)
    {
        var builder = service.App.MapMethods(pattern, [method], handler);
        var options = MasaApp.GetRequiredService<IOptions<ServiceGlobalRouteOptions>>().Value;
        options.RouteHandlerBuilder?.Invoke(builder);
        return builder;
    }
}
