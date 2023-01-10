// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Infrastructure.Middleware
{
    public class DisabledCommandMiddleware<TEvent> : Middleware<TEvent>
        where TEvent : notnull, IEvent
    {
        readonly IUserContext _userContext;
        readonly IHostEnvironment _hostEnvironment;

        public DisabledCommandMiddleware(IUserContext userContext, IHostEnvironment hostEnvironment)
        {
            _userContext = userContext;
            _hostEnvironment = hostEnvironment;
        }

        public override async Task HandleAsync(TEvent @event, EventHandlerDelegate next)
        {
            var user = _userContext.GetUser<MasaUser>();
            if (_hostEnvironment.EnvironmentName.ToLower() == "isdemo" && user?.Account == "Guest" && @event is ICommand)
            {
                throw new UserFriendlyException("演示账号禁止操作");
            }
            await next();
        }
    }
}
