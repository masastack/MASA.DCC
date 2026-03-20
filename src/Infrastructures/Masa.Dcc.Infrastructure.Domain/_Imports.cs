// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

global using System.Linq.Expressions;
global using System.Text.Encodings.Web;
global using System.Text.Json;
global using Mapster;
global using Masa.BuildingBlocks.Caching;
global using Masa.BuildingBlocks.Configuration;
global using Masa.BuildingBlocks.Data.UoW;
global using Masa.BuildingBlocks.Ddd.Domain.Events;
global using Masa.BuildingBlocks.Ddd.Domain.Services;
global using Masa.BuildingBlocks.Globalization.I18n;
global using Masa.BuildingBlocks.ReadWriteSplitting.Cqrs.Commands;
global using Masa.BuildingBlocks.ReadWriteSplitting.Cqrs.Queries;
global using Masa.BuildingBlocks.StackSdks.Config;
global using Masa.BuildingBlocks.StackSdks.Dcc.Contracts.Model;
global using Masa.BuildingBlocks.StackSdks.Middleware;
global using Masa.BuildingBlocks.StackSdks.Pm;
global using Masa.BuildingBlocks.StackSdks.Pm.Model;
global using Masa.Contrib.Dispatcher.Events;
global using Masa.Dcc.Contracts.Admin.App.Dtos;
global using Masa.Dcc.Contracts.Admin.App.Enums;
global using Masa.Dcc.Contracts.Admin.Label.Dtos;
global using Masa.Dcc.Infrastructure.Domain.App.Queries;
global using Masa.Dcc.Infrastructure.Domain.Commands;
global using Masa.Dcc.Infrastructure.Domain.Queries;
global using Masa.Dcc.Infrastructure.Domain.Services;
global using Masa.Dcc.Infrastructure.Domain.Shared;
global using Masa.Dcc.Infrastructure.EFCore;
global using Masa.Dcc.Infrastructure.Repository.App;
global using Masa.Dcc.Infrastructure.Repository.Label;
global using Masa.Utils.Security.Cryptography;
global using Microsoft.EntityFrameworkCore;
