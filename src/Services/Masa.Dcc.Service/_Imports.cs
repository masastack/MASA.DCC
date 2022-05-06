// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

global using FluentValidation;
global using FluentValidation.AspNetCore;
global using Mapster;
global using Masa.BuildingBlocks.BasicAbility.Pm;
global using Masa.BuildingBlocks.BasicAbility.Pm.Model;
global using Masa.BuildingBlocks.Data.UoW;
global using Masa.BuildingBlocks.Ddd.Domain.Entities.Auditing;
global using Masa.BuildingBlocks.Ddd.Domain.Events;
global using Masa.BuildingBlocks.Ddd.Domain.Repositories;
global using Masa.BuildingBlocks.Dispatcher.Events;
global using Masa.Contrib.BasicAbility.Pm;
global using Masa.Contrib.Data.UoW.EF;
global using Masa.Contrib.Ddd.Domain;
global using Masa.Contrib.Ddd.Domain.Events;
global using Masa.Contrib.Ddd.Domain.Repository.EF;
global using Masa.Contrib.Dispatcher.Events;
global using Masa.Contrib.Dispatcher.IntegrationEvents.EventLogs.EF;
global using Masa.Contrib.ReadWriteSpliting.Cqrs.Commands;
global using Masa.Contrib.ReadWriteSpliting.Cqrs.Queries;
global using Masa.Contrib.Service.MinimalAPIs;
global using Masa.Dcc.Contracts.Admin.App.Dtos;
global using Masa.Dcc.Contracts.Admin.App.Enums;
global using Masa.Dcc.Service.Admin.Application.App.Commands;
global using Masa.Dcc.Service.Admin.Application.App.Queries;
global using Masa.Dcc.Service.Admin.Domain.App.Aggregates;
global using Masa.Dcc.Service.Admin.Domain.App.Repositories;
global using Masa.Dcc.Service.Admin.Domain.Label.Repositories;
global using Masa.Dcc.Service.Infrastructure;
global using Masa.Dcc.Service.Infrastructure.Middleware;
global using Masa.Utils.Data.EntityFrameworkCore;
global using Masa.Utils.Data.EntityFrameworkCore.SqlServer;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.EntityFrameworkCore;
global using System.ComponentModel.DataAnnotations;
global using System.ComponentModel.DataAnnotations.Schema;
global using Masa.Dcc.Service.Admin.Domain.App.Services;
global using Masa.Utils.Caching.DistributedMemory;
global using Masa.Utils.Caching.DistributedMemory.Interfaces;





