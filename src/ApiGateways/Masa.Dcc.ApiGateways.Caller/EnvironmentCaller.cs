using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masa.Dcc.Caller
{
    public class EnvironmentCaller : HttpClientCallerBase
    {
        private readonly string _prefix = "/api/v1/env";

        public EnvironmentCaller(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Name = nameof(EnvironmentCaller);
        }

        protected override string BaseAddress { get; set; } = AppSettings.Get("ServiceBaseUrl");

        public async Task<EnvironmentDetailModel> GetAsync(int Id)
        {
            var result = await CallerProvider.GetAsync<EnvironmentDetailModel>($"{_prefix}/{Id}");

            return result ?? new();
        }

        public async Task<List<EnvironmentModel>> GetListAsync()
        {
            var result = await CallerProvider.GetAsync<List<EnvironmentModel>>($"{_prefix}");

            return result ?? new();
        }
    }
}
