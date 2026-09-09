// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Infrastructure.Domain.Services;

public class InitConfigObjectDomainService : DomainService
{
    private readonly IConfigObjectReleaseRepository _configObjectReleaseRepository;
    private readonly IConfigObjectRepository _configObjectRepository;
    private readonly IPublicConfigRepository _publicConfigRepository;
    private readonly IConfigPublishStore _configPublishStore;
    private readonly IMasaStackConfig _masaStackConfig;
    private readonly IUnitOfWork _unitOfWork;

    public InitConfigObjectDomainService(
        IDomainEventBus eventBus,
        IConfigObjectReleaseRepository configObjectReleaseRepository,
        IConfigObjectRepository configObjectRepository,
        IPublicConfigRepository publicConfigRepository,
        IConfigPublishStore configPublishStore,
        IMasaStackConfig masaStackConfig,
        IUnitOfWork unitOfWork) : base(eventBus)
    {
        _configObjectReleaseRepository = configObjectReleaseRepository;
        _configObjectRepository = configObjectRepository;
        _publicConfigRepository = publicConfigRepository;
        _configPublishStore = configPublishStore;
        _masaStackConfig = masaStackConfig;
        _unitOfWork = unitOfWork;
    }

    private string EncryptContent(string content)
    {
        var secret = _masaStackConfig.DccSecret;
        var encryptContent = AesUtils.Encrypt(content, secret, FillType.Left);
        return encryptContent;
    }

    private async Task AddConfigObjectReleaseAsync(AddConfigObjectReleaseDto dto)
    {
        var configObject = (await _configObjectRepository.FindAsync(configObject => configObject.Id == dto.ConfigObjectId)) ?? throw new Exception("Config object does not exist");

        configObject.AddContent(configObject.Content, configObject.Content);
        await _configObjectRepository.UpdateAsync(configObject);

        var configObjectRelease = new ConfigObjectRelease(
               dto.ConfigObjectId,
               dto.Name,
               dto.Comment,
               configObject.Content);
        await _configObjectReleaseRepository.AddAsync(configObjectRelease);

        if (configObject.Encryption)
        {
            dto.Content = EncryptContent(dto.Content);
        }
        var releaseContent = new PublishReleaseModel
        {
            Content = dto.Content,
            FormatLabelCode = configObject.FormatLabelCode,
            Encryption = configObject.Encryption
        };
        await _configPublishStore.SetAsync(dto.EnvironmentName, dto.ClusterName, dto.Identity, configObject.Name, releaseContent);
    }

    public async Task InitConfigObjectAsync(
        string environmentName,
        string clusterName,
        int envClusterId,
        string appId,
        Dictionary<string, string> configObjects,
        ConfigObjectType configObjectType = ConfigObjectType.App,
        bool isEncryption = false)
    {
        foreach (var configObject in configObjects)
        {
            var configObjectName = configObject.Key;
            string content = configObject.Value;
            if (isEncryption)
                content = EncryptContent(content);

            var newConfigObject = new ConfigObject(
                configObjectName,
                "JSON",
                configObjectType,
                content,
                "{}",
                encryption: isEncryption);

            var publicConfig = await _publicConfigRepository.FindAsync(publicConfig => publicConfig.Identity == appId);
            if (publicConfig == null)
                throw new UserFriendlyException("只能初始化public配置");

            int objectId = 0;
            var existsConfigObject = await _configObjectRepository.FindAsync(configObject => configObject.Type == configObjectType && configObject.Name == configObjectName);
            if (existsConfigObject == null)
            {
                newConfigObject.SetConfigObjectType(ConfigObjectType.App);
                newConfigObject.SetPublicConfigObject(publicConfig.Id, envClusterId);
                await _configObjectRepository.AddAsync(newConfigObject);
                await _unitOfWork.SaveChangesAsync();
                objectId = newConfigObject.Id;
            }
            else
            {
                objectId = existsConfigObject.Id;
                var value = configObject.Value;
                if (isEncryption)
                {
                    value = EncryptContent(value);
                }
                existsConfigObject.UpdateContent(value);
                await _configObjectRepository.UpdateAsync(existsConfigObject);
            }

            var publishedConfig = await _configPublishStore.GetAsync(environmentName, clusterName, appId, configObjectName);
            if (publishedConfig != null)
            {
                continue;
            }

            var releaseModel = new AddConfigObjectReleaseDto
            {
                Type = ReleaseType.MainRelease,
                ConfigObjectId = objectId,
                Name = "通过Sdk发布",
                EnvironmentName = environmentName,
                ClusterName = clusterName,
                Identity = appId,
                Content = configObject.Value
            };
            await AddConfigObjectReleaseAsync(releaseModel);
        }
    }
}
