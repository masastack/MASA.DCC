﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Infrastructure.Repository.App;

public interface IAppPinRepository : IRepository<AppPin>
{
    Task<List<AppPin>> GetListAsync(List<int> appIds);
}
