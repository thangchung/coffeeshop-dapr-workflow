# Coffeeshop Dapr Workflow Demo

Opinionated coffeeshop application builds with Dapr workflow

![coffeeshop-wf](assets/coffeeshop-wf.svg)

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
make publish-all-dockers
```

## Refs

- https://github.com/davidfowl/TodoApi/blob/davidfowl/net8/README.md
- https://github.com/thangchung/dapr-labs