// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Contracts.Admin.Infrastructure.Dtos
{
    public class PaginatedListDto<T>
    {
        public int Page { get; set; }

        public int PageSize { get; set; }

        public long Total { get; set; }

        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(Total / (double)PageSize);

        public List<T> Result { get; set; } = new();

        public PaginatedListDto()
        {
        }

        public PaginatedListDto(int page, int pageSize, long total, List<T> result)
        {
            Page = page;
            PageSize = pageSize;
            Total = total;
            Result = result;
        }
    }
}
