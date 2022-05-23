// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

global using System.Net.Http.Json;
global using System.Reflection;
global using System.Text;
global using System.Text.Json;
global using System.Text.RegularExpressions;
global using BlazorComponent;
global using BlazorComponent.I18n;
global using Mapster;
global using Masa.Blazor;
global using Masa.BuildingBlocks.BasicAbility.Pm.Model;
global using Masa.Dcc.ApiGateways.Caller;
global using Masa.Dcc.Caller;
global using Masa.Dcc.Contracts.Admin.App.Dtos;
global using Masa.Dcc.Contracts.Admin.App.Enums;
global using Masa.Dcc.Contracts.Admin.Label.Dtos;
global using Masa.Dcc.Web.Admin.Rcl.Global.Config;
global using Masa.Dcc.Web.Admin.Rcl.Global.Nav.Model;
global using Masa.Dcc.Web.Admin.Rcl.Model;
global using Masa.Dcc.Web.Admin.Rcl.Shared;
global using Microsoft.AspNetCore.Components;
global using Microsoft.AspNetCore.Components.Web;
global using Microsoft.AspNetCore.Http;
global using Microsoft.JSInterop;
global using Newtonsoft.Json.Linq;
