namespace Masa.Dcc.Service.Admin.Application.App
{
    public class QueryHandler
    {
        private readonly IPublicConfigRepository _publicConfigRepository;

        public QueryHandler(IPublicConfigRepository publicConfigRepository)
        {
            _publicConfigRepository = publicConfigRepository;
        }

        public async Task GetPublicConfigsAsync(PublicConfigsQuery query)
        {
            var result = await _publicConfigRepository.GetListAsync();
            query.Result = result.Select(p => new PublicConfigDto
            {
                Id = p.Id,
                Name = p.Name,
                Identity = p.Identity,
                Description = p.Description,
                CreationTime = p.CreationTime,
                Creator = p.Creator,
                Modifier = p.Modifier,
                ModificationTime = p.ModificationTime
            }).ToList();
        }
    }
}
