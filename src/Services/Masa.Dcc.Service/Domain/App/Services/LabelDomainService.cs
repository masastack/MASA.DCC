namespace Masa.Dcc.Service.Admin.Domain.App.Services
{
    public class LabelDomainService : DomainService
    {
        private readonly ILabelRepository _labelRepository;

        public LabelDomainService(IDomainEventBus eventBus, ILabelRepository orderRepository) : base(eventBus)
        {
            _labelRepository = orderRepository;
        }

        public async Task<Label.Aggregates.Label> GetAsync(int Id)
        {
            return await _labelRepository.FindAsync(label => label.Id == Id) ?? throw new UserFriendlyException("label not exist");
        }

        public async Task<List<Label.Aggregates.Label>> GetListAsync()
        {
            var result = await _labelRepository.GetListAsync();

            return result.ToList();
        }
    }
}