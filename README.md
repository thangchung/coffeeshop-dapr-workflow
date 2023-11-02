# Coffeeshop Dapr Workflow Demo
Opinionated coffeeshop application builds with Dapr workflow

![](assets/coffeeshop-wf.svg)

## Place Order Workflow

```mermaid
%%{
    init: {
        'theme':'neutral'
    }
}%%

graph TB
    A[Start]
    A1([NotifyActivity])
    B([AddOrderActivity])
    C{PlaceOrderResult}
    
    EXTERNAL_BARISTA_APP([External BaristaApp])
    EXTERNAL_KITCHEN_APP([External KitchenApp])
    D([WaitForExternalEventAsync])
    E([WaitForExternalEventAsync])
    F([Task.WhenAll])
    G[UpdateOrderActivity]
    G1([NotifyActivity])
    
    Exception([Catch TaskCanceledException or Exception])

    H[RefundMoneyActivity]
    I[End]

    A --> |"PlaceOrderCommand input"| A1
    A1 --> |"Notification - Order Received"| B
    B --> |"return"| C
    C --> |"Success"| D -->|"BaristaOrderUpdated"| F
    EXTERNAL_BARISTA_APP --> |"subcribe BaristaOrderUpdated event"| D
    C --> |"Success"| E -->|"KitchenOrderUpdated"| F
    EXTERNAL_KITCHEN_APP --> |"subcribe KitchenOrderUpdated event"| E
    F --> |"merging result"| G --> |"Notification - Order Completed"| G1
    C --> |"Failed, then compensate"| H

    D --> |"throw ex"| Exception --> |"compensate"| H --> I
    E --> |"throw ex"| Exception --> |"compensate"| H --> I
    G --> |"throw ex"| Exception --> |"compensate"| H --> I

    G1 --> I
    H --> I
```

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