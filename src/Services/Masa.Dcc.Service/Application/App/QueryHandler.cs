namespace Masa.Dcc.Service.Admin.Application.App
{
    public class QueryHandler
    {
        private readonly IPublicConfigRepository _publicConfigRepository;
        private readonly IPublicConfigObjectRepository _publicConfigObjectRepository;
        private readonly LabelDomainService _labelDomainService;

        public QueryHandler(
            IPublicConfigRepository publicConfigRepository,
            IPublicConfigObjectRepository publicConfigObjectRepository,
            LabelDomainService labelDomainService)
        {
            _publicConfigRepository = publicConfigRepository;
            _publicConfigObjectRepository = publicConfigObjectRepository;
            _labelDomainService = labelDomainService;
        }

        [EventHandler]
        public async Task GetPublicConfigsAsync(PublicConfigsQuery query)
        {
            IEnumerable<PublicConfig> result = await _publicConfigRepository.GetListAsync();
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

        [EventHandler]
        public async Task GetConfigObjectsAsync(ConfigObjectsQuery query)
        {
            var publicConfigObjects = await _publicConfigObjectRepository.GetListByEnvClusterIdAsync(query.EnvClusterId);
            var labels = await _labelDomainService.GetListAsync();
            query.Result = publicConfigObjects.Select(publicConfigObject => new ConfigObjectDto
            {
                Name = publicConfigObject.ConfigObject.Name,
                FormatName = labels.FirstOrDefault(label => label.Id == publicConfigObject.ConfigObject.FormatLabelId)?.Name ?? "",
                TypeName = labels.FirstOrDefault(label => label.Id == publicConfigObject.ConfigObject.TypeLabelId)?.Name ?? "",
                RelationConfigObjectId = publicConfigObject.ConfigObject.RelationConfigObjectId,
                ConfigObjectMain = publicConfigObject.ConfigObject.ConfigObjectMain == null
                ? new ConfigObjectMainDto()
                : new ConfigObjectMainDto
                {
                    Id = publicConfigObject.ConfigObject.ConfigObjectMain.Id,
                    ConfigObjectId = publicConfigObject.ConfigObject.ConfigObjectMain.ConfigObjectId,
                    Content = publicConfigObject.ConfigObject.ConfigObjectMain.Content,
                    TempContent = publicConfigObject.ConfigObject.ConfigObjectMain.TempContent,
                    Creator = publicConfigObject.ConfigObject.ConfigObjectMain.Creator,
                    ModificationTime = publicConfigObject.ConfigObject.ConfigObjectMain.ModificationTime,
                    CreationTime = publicConfigObject.ConfigObject.ConfigObjectMain.CreationTime,
                    Modifier = publicConfigObject.ConfigObject.ConfigObjectMain.Modifier
                }
            }).ToList();
        }
    }
}
