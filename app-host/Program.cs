using System.Collections.Immutable;

using Aspire.Hosting;
using Aspire.Hosting.Dapr;

var builder = DistributedApplication.CreateBuilder(args);

var stateStore = builder.AddDaprStateStore("statestore", 
    new DaprComponentOptions {
        LocalPath = "../components/statestore.yaml"
    });
var baristapubsub = builder.AddDaprPubSub("baristapubsub", 
    new DaprComponentOptions{
        LocalPath = "../components/barista_pubsub.yaml"
    });
var kitchenpubsub = builder.AddDaprPubSub("kitchenpubsub", 
    new DaprComponentOptions{
        LocalPath = "../components/kitchen_pubsub.yaml"
    });

builder.AddProject<Projects.product_api>("productapi")
    .WithEnvironment("ASPNETCORE_URLS", "http://+:5001")
    .WithDaprSidecar(new DaprSidecarOptions{
        AppId = "productapp",
        AppPort = 5001,
        DaprHttpPort = 3500,
        PlacementHostAddress = "localhost:50005",
        ResourcesPaths = ImmutableHashSet.Create("../components"),
        Config = "../components/daprConfig.yaml",
        LogLevel = "debug"
    })
    .WithReference(stateStore)
    .WithReference(baristapubsub)
    .WithReference(kitchenpubsub);

builder.AddProject<Projects.counter_api>("counterapi")
    .WithEnvironment("ASPNETCORE_URLS", "http://+:5002")
    .WithEnvironment("ProductCatalogAppDaprName", "productapp")
    .WithEnvironment("DAPR_GRPC_PORT", "4001")
    .WithDaprSidecar(new DaprSidecarOptions{
        AppId = "counterapp",
        AppPort = 5002,
        AppProtocol = "http",
        DaprGrpcPort = 4001,
        DaprHttpPort = 3501,
        DaprInternalGrpcPort = 4001,
        PlacementHostAddress = "localhost:50005",
        ResourcesPaths = ImmutableHashSet.Create("../components"),
        Config = "../components/daprConfig.yaml",
        LogLevel = "debug"
    })
    .WithReference(stateStore)
    .WithReference(baristapubsub)
    .WithReference(kitchenpubsub);

builder.AddProject<Projects.barista_api>("baristaapi")
    .WithEnvironment("ASPNETCORE_URLS", "http://+:5003")
    .WithDaprSidecar(new DaprSidecarOptions{
        AppId = "baristaapp",
        AppPort = 5003,
        DaprHttpPort = 3502,
        PlacementHostAddress = "localhost:50005",
        ResourcesPaths = ImmutableHashSet.Create("../components"),
        Config = "../components/daprConfig.yaml",
        LogLevel = "debug"
    })
    .WithReference(stateStore)
    .WithReference(baristapubsub)
    .WithReference(kitchenpubsub);

builder.AddProject<Projects.kitchen_api>("kitchenapi")
    .WithEnvironment("ASPNETCORE_URLS", "http://+:5004")
    .WithDaprSidecar(new DaprSidecarOptions{
        AppId = "kitchenapp",
        AppPort = 5004,
        DaprHttpPort = 3504,
        PlacementHostAddress = "localhost:50005",
        ResourcesPaths = ImmutableHashSet.Create("../components"),
        Config = "../components/daprConfig.yaml",
        LogLevel = "debug"
    })
    .WithReference(stateStore)
    .WithReference(baristapubsub)
    .WithReference(kitchenpubsub);

builder.Build().Run();
