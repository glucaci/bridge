﻿using Bridge.Workflow.Tests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Bridge.Storage.MongoDB.Tests;

[Collection("Mongo collection")]
public class MongoWhenScenario : WhenScenario
{        
    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddWorkflow(x => x.UseInMemoryBus().UseMongoDB(MongoDockerSetup.ConnectionString, nameof(MongoWhenScenario)));
    }
}