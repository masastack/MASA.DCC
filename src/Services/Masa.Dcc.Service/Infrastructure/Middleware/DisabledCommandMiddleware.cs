// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Infrastructure.Middleware
{
    public class DisabledCommandMiddleware<TEvent> : EventMiddleware<TEvent>
        where TEvent : notnull, IEvent
    {
        readonly IUserContext _userContext;
        readonly IMasaStackConfig _masaStackConfig;
        readonly I18n<DefaultResource> _i18N;

        public DisabledCommandMiddleware(IUserContext userContext, IMasaStackConfig masaStackConfig, I18n<DefaultResource> i18N)
        {
            _userContext = userContext;
            _masaStackConfig = masaStackConfig;
            _i18N = i18N;
        }

        public override async Task HandleAsync(TEvent @event, EventHandlerDelegate next)
        {
            var user = _userContext.GetUser<MasaUser>();
            var attribute = Attribute.GetCustomAttribute(typeof(TEvent), typeof(ByPassDisabledCommandAttribute));

            if (_masaStackConfig.IsDemo && attribute == null && user?.Account?.ToLower() == "guest" && @event is ICommand)
            {
                throw new UserFriendlyException(_i18N.T("Demo Account Prohibited Operations"));
            }

            await next();
        }
    }
}
