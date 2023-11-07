import radius as radius

@description('Specifies the environment for resources.')
param environment string

@description('The ID of your Radius Application. Automatically injected by the rad CLI.')
param application string

var imageVersion = '0.1.0'

// HTTP Routes
module httpRoutes 'iac/infra/http-routes.bicep' = {
  name: '${deployment().name}-http-routes'
  params: {
    appId: application
  }
}

module productApi 'iac/apps/product-api.bicep' = {
  name: '${deployment().name}-product-api'
  params: {
    appId: application
    apiRouteName: httpRoutes.outputs.productApiRouteName
    daprStateStoreName: stateStore.name
    daprBaristaPubSubBrokerName: baristapubsubBroker.name
    daprKitchenPubSubBrokerName: kitchenpubsubBroker.name
    imageVersion: imageVersion
  }
}

// resource demo 'Applications.Core/containers@2023-10-01-preview' = {
//   name: 'demo'
//   properties: {
//     application: application
//     container: {
//       image: 'radius.azurecr.io/samples/demo:latest'
//       ports: {
//         web: {
//           containerPort: 3000
//         }
//       }
//     }
//     extensions: [
//       {
//         kind: 'daprSidecar'
//         appId: 'demo'
//         appPort: 3000
//       }
//     ]
//   }
// }

// The Dapr state store that is connected to the backend container
resource stateStore 'Applications.Dapr/stateStores@2023-10-01-preview' = {
  name: 'statestore'
  properties: {
    // Provision Redis Dapr state store automatically via the default Radius Recipe
    environment: environment
    application: application
  }
}

resource baristapubsubBroker 'Applications.Dapr/pubsubBrokers@2023-10-01-preview' = {
  name: 'baristapubsub'
  properties: {
    // Provision Redis Dapr state store automatically via the default Radius Recipe
    environment: environment
    application: application
  }
}

resource kitchenpubsubBroker 'Applications.Dapr/pubsubBrokers@2023-10-01-preview' = {
  name: 'kitchenpubsub'
  properties: {
    // Provision Redis Dapr state store automatically via the default Radius Recipe
    environment: environment
    application: application
  }
}
