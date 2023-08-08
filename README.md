# Coffeeshop Dapr Workflow Demo
Opinionated coffeeshop application builds with Dapr workflow

![](assets/coffeeshop-wf.svg)

## Build Docker Images

```sh
dotnet publish ./product-api/product-api.csproj --os linux --arch x64 /t:PublishContainer -c Release
dotnet publish ./counter-api/counter-api.csproj --os linux --arch x64 /t:PublishContainer -c Release
dotnet publish ./barista-api/barista-api.csproj --os linux --arch x64 /t:PublishContainer -c Release
dotnet publish ./kitchen-api/kitchen-api.csproj --os linux --arch x64 /t:PublishContainer -c Release
```

```sh
docker tag product-api:latest ghcr.io/thangchung/coffeeshop-dapr-workflow/product-api:0.1.0
docker tag counter-api:latest ghcr.io/thangchung/coffeeshop-dapr-workflow/counter-api:0.1.0
docker tag barista-api:latest ghcr.io/thangchung/coffeeshop-dapr-workflow/barista-api:0.1.0
docker tag kitchen-api:latest ghcr.io/thangchung/coffeeshop-dapr-workflow/kitchen-api:0.1.0
```

## TODO:
- Consider to use https://mapperly.riok.app/docs/getting-started/first-mapper

## Refs
- https://github.com/davidfowl/TodoApi/blob/davidfowl/net8/README.md
- https://github.com/thangchung/dapr-labs