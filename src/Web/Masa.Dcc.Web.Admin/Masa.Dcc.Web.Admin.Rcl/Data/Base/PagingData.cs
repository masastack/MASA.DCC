﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace MASA.PM.UI.Admin.Data.Base;

public class PagingData<TEntity> where TEntity : class
{
    public int PageIndex { get; private set; }

    public int PageSize { get; private set; }

    public long Count { get; private set; }

    public int PageCount => (int)Math.Ceiling(Count / (decimal)PageSize);

    public IEnumerable<TEntity> Items { get; private set; }

    public PagingData(int pageIndex, int pageSize, long count, IEnumerable<TEntity> items)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
        Count = count;
        Items = items;
    }
}

