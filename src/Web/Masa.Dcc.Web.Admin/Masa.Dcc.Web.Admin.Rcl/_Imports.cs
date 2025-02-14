// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

global using System.Text;
global using System.Text.Json;
global using System.Text.RegularExpressions;
global using System.Xml.Linq;
global using FluentValidation;
global using Mapster;
global using Masa.Blazor;
global using Masa.BuildingBlocks.Authentication.Identity;
global using Masa.BuildingBlocks.StackSdks.Auth.Contracts;
global using Masa.BuildingBlocks.StackSdks.Auth.Contracts.Model;
global using Masa.BuildingBlocks.StackSdks.Pm.Model;
global using Masa.Dcc.ApiGateways.Caller;
global using Masa.Dcc.Caller;
global using Masa.Dcc.Contracts.Admin;
global using Masa.Dcc.Contracts.Admin.App.Dtos;
global using Masa.Dcc.Contracts.Admin.App.Enums;
global using Masa.Dcc.Contracts.Admin.Label.Dtos;
global using Masa.Dcc.Web.Admin.Rcl.Model;
global using Masa.Dcc.Web.Admin.Rcl.Pages.Modal;
global using Masa.Stack.Components;
global using Masa.Stack.Components.Configs;
global using Masa.Stack.Components.Extensions;
global using Microsoft.AspNetCore.Components;
global using Microsoft.AspNetCore.Components.Routing;
global using Microsoft.JSInterop;
global using ProjectModel = Masa.BuildingBlocks.StackSdks.Pm.Model.ProjectModel;
