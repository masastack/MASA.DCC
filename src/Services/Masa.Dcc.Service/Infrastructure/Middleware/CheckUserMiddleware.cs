// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Infrastructure.Middleware
{
    public class CheckUserMiddleware
    {
        private readonly RequestDelegate _next;

        public CheckUserMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IUserContext userContext, IUserSetter userSetter, IMasaStackConfig masaConfig)
        {
            var identityUser = userContext.GetUser();

            IDisposable? userSetterHandle = null;
            if (identityUser == null)
            {
                string system = "system";
                var auditUser = new IdentityUser() { Id = masaConfig.GetDefaultUserId().ToString(), UserName = system };
                userSetterHandle = userSetter!.Change(auditUser);
            }

            await _next(context);
            userSetterHandle?.Dispose();
        }
    }
}
