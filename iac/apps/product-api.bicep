import radius as radius

@description('The Radius application ID.')
param appId string

// @description('The service name.')
// param serviceName string = 'product-api'

@description('The name of the API HTTP route.')
param apiRouteName string

// @description('The name of the Radius gateway.')
// param gatewayName string

@description('The Dapr application ID.')
var daprAppId = 'product-api'

@description('The name of the Dapr pub/sub component.')
param daprBaristaPubSubBrokerName string

@description('The name of the Dapr pub/sub component.')
param daprKitchenPubSubBrokerName string

@description('The name of the Dapr state store component.')
param daprStateStoreName string

@description('The image version.')
param imageVersion string = 'latest'

resource apiRoute 'Applications.Core/httpRoutes@2023-10-01-preview' existing = {
  name: apiRouteName
}

// resource gateway 'Applications.Core/gateways@2023-10-01-preview' existing = {
//   name: gatewayName
// }

resource stateStore 'Applications.Dapr/stateStores@2023-10-01-preview' existing = {
  name: daprStateStoreName
}

resource baristapubsubBroker 'Applications.Dapr/pubsubBrokers@2023-10-01-preview' existing = {
  name: daprBaristaPubSubBrokerName
}

resource kitchenpubsubBroker 'Applications.Dapr/pubsubBrokers@2023-10-01-preview' existing = {
  name: daprKitchenPubSubBrokerName
}

resource productApi 'Applications.Core/containers@2023-10-01-preview' = {
  name: 'product-api'
  properties: {
    application: appId
    container: {
      image: 'ghcr.io/thangchung/coffeeshop-dapr-workflow/product-api:${imageVersion}'
      env: {
        ASPNETCORE_ENVIRONMENT: 'Development'
        ASPNETCORE_URLS: 'http://0.0.0.0:80'
      }
      ports: {
        http: {
          containerPort: 80
          provides: apiRoute.id
        }
      }
    }
    extensions: [
      {
        kind: 'daprSidecar'
        appId: 'product-api'
        appPort: 80
      }
    ]
    // connections: {
    //   baristapubsubBroker: {
    //     source: baristapubsubBroker.id
    //   }
    //   kitchenpubsubBroker: {
    //     source: kitchenpubsubBroker.id
    //   }
    //   daprStateStore: {
    //     source: stateStore.id
    //   }
    // }
  }
}

output appId string = daprAppId
