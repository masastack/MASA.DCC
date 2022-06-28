// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Application.App
{
    public class CommandHandler
    {
        private readonly IPublicConfigRepository _publicConfigRepository;
        private readonly IConfigObjectRepository _configObjectRepository;
        private readonly ILabelRepository _labelRepository;
        private readonly IAppPinRepository _appPinRepository;
        private readonly IBizConfigRepository _bizConfigRepository;
        private readonly ConfigObjectDomainService _configObjectDomainService;

        public CommandHandler(
            IPublicConfigRepository publicConfigRepository,
            IConfigObjectRepository configObjectRepository,
            ILabelRepository labelRepository,
            IAppPinRepository appPinRepository,
            IBizConfigRepository bizConfigRepository,
            ConfigObjectDomainService configObjectDomainService)
        {
            _publicConfigRepository = publicConfigRepository;
            _configObjectRepository = configObjectRepository;
            _labelRepository = labelRepository;
            _appPinRepository = appPinRepository;
            _bizConfigRepository = bizConfigRepository;
            _configObjectDomainService = configObjectDomainService;
        }

        #region PublicConfig

        [EventHandler]
        public async Task AddPublicConfigAsync(AddPublicConfigCommand command)
        {
            var publicConfig = command.AddPublicConfigDto;

            var result = await _publicConfigRepository.AddAsync(
                new PublicConfig(publicConfig.Name, publicConfig.Identity, publicConfig.Description));

            command.PublicConfigDto = result.Adapt<PublicConfigDto>();
        }

        [EventHandler]
        public async Task UpdatePublicConfigAsync(UpdatePublicConfigCommand command)
        {
            var publicConfig = command.UpdatePublicConfigDto;
            var publicConfigEntity = await _publicConfigRepository.FindAsync(p => p.Id == publicConfig.Id)
                ?? throw new UserFriendlyException("Public config does not exist");

            publicConfigEntity.Update(publicConfig.Name, publicConfig.Description);
            await _publicConfigRepository.UpdateAsync(publicConfigEntity);
        }

        [EventHandler]
        public async Task RemovePublicConfigAsync(RemovePublicConfigCommand command)
        {
            var publicConfigEntity = await _publicConfigRepository.FindAsync(p => p.Id == command.PublicConfigId)
                ?? throw new UserFriendlyException("Public config does not exist");

            await _publicConfigRepository.RemoveAsync(publicConfigEntity);
        }

        #endregion

        #region BizConfig

        [EventHandler]
        public async Task AddBizConfigAsync(AddBizConfigCommand command)
        {
            var bizConfig = command.AddBizConfigDto;

            var result = await _bizConfigRepository.AddAsync(new BizConfig(bizConfig.Name, bizConfig.Identity));

            await _bizConfigRepository.UnitOfWork.SaveChangesAsync();

            command.BizConfigDto = result.Adapt<BizConfigDto>();
        }

        [EventHandler]
        public async Task UpdateBizConfigAsync(UpdateBizConfigCommand command)
        {
            var bizConfig = command.UpdateBizConfigDto;
            var bizConfigEntity = await _bizConfigRepository.FindAsync(p => p.Id == bizConfig.Id)
                ?? throw new UserFriendlyException("biz config does not exist");

            bizConfigEntity.Update(bizConfig.Name);
            var result = await _bizConfigRepository.UpdateAsync(bizConfigEntity);

            command.BizConfigDto = result.Adapt<BizConfigDto>();
        }

        #endregion

        #region ConfigObject

        [EventHandler]
        public async Task AddConfigObjectAsync(AddConfigObjectCommand command)
        {
            await _configObjectDomainService.AddConfigObjectAsync(command.ConfigObjectDtos);
        }

        [EventHandler]
        public async Task RemoveConfigObjectAsync(RemoveConfigObjectCommand command)
        {
            await _configObjectDomainService.RemoveConfigObjectAsync(command.ConfigObjectId);
        }

        [EventHandler]
        public async Task UpdateConfigObjectContentAsync(UpdateConfigObjectContentCommand command)
        {
            await _configObjectDomainService.UpdateConfigObjectContentAsync(command.ConfigObjectContent);
        }

        [EventHandler]
        public async Task RevokeConfigObjectAsync(RevokeConfigObjectCommand command)
        {
            var configObject = await _configObjectRepository.FindAsync(configObject => configObject.Id == command.ConfigObjectId);

            if (configObject == null)
            {
                throw new UserFriendlyException("Config object does not exist");
            }

            configObject.Revoke();

            await _configObjectRepository.UpdateAsync(configObject);
        }

        [EventHandler]
        public async Task CloneConfigObjectAsync(CloneConfigObjectCommand command)
        {
            await _configObjectDomainService.CloneConfigObjectAsync(command.CloneConfigObject);
        }

        #endregion

        #region Release

        [EventHandler]
        public async Task AddConfigObjectRelease(AddConfigObjectReleaseCommand command)
        {
            await _configObjectDomainService.AddConfigObjectReleaseAsync(command.ConfigObjectRelease);
        }

        [EventHandler]
        public async Task RollbackConfigObjectReleaseAsync(RollbackConfigObjectReleaseCommand command)
        {
            await _configObjectDomainService.RollbackConfigObjectReleaseAsync(command.RollbackConfigObjectRelease);
        }

        #endregion

        #region App

        [EventHandler]
        public async Task AddAppPinAsync(AddAppPinCommand command)
        {
            await _appPinRepository.AddAsync(new AppPin(command.AppId));
        }

        [EventHandler]
        public async Task RemoveAppPinAsync(RemoveAppPinCommand command)
        {
            var appPin = await _appPinRepository.FindAsync(appPin => appPin.AppId == command.AppId);

            if (appPin != null)
                await _appPinRepository.RemoveAsync(appPin);
        }

        #endregion
    }
}
