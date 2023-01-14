// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Infrastructure.Middleware
{
    public class DisabledCommandMiddleware<TEvent> : Middleware<TEvent>
        where TEvent : notnull, IEvent
    {
        readonly IUserContext _userContext;
        readonly IConfiguration _configuration;

        public DisabledCommandMiddleware(IUserContext userContext, IConfiguration configuration)
        {
            _userContext = userContext;
            _configuration = configuration;
        }

        public override async Task HandleAsync(TEvent @event, EventHandlerDelegate next)
        {
            var isDemo = _configuration.GetValue<bool>("IsDemo");
            var user = _userContext.GetUser<MasaUser>();

            if (isDemo && user?.Account == "guest" && @event is ICommand)
            {
                throw new UserFriendlyException("演示账号禁止操作");
            }
            await next();
        }
    }
}
