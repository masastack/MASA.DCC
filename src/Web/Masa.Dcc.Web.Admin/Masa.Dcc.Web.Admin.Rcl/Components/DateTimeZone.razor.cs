// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using Masa.Stack.Components;

namespace Masa.Dcc.Web.Admin.Rcl.Components
{
    public partial class DateTimeZone
    {
        [Parameter]
        public DateTime Value { get; set; }

        [Parameter]
        public string? Style { get; set; }

        [Parameter]
        public string? Class { get; set; }

        [Parameter]
        public Func<DateTime, string>? Format { get; set; }

        [Inject]
        private I18n I18n { get; set; } = default!;

        [Inject]
        private JsInitVariables JsInitVariables { get; set; } = default!;

        private string TransformDateTime()
        {
            var dateTime = Value.Add(JsInitVariables.TimezoneOffset);

            if (Format != null)
            {
                return Format(dateTime);
            }
            else
            {
                return dateTime.ToString(I18n.T("$DateTimeFormat"));
            }
        }
    }
}
