import radius as radius

@description('The Radius application ID.')
param appId string

resource productApiRoute 'Applications.Core/httpRoutes@2023-10-01-preview' = {
  name: 'product-api-route'
  properties: {
    application: appId
  }
}


output productApiRouteName string = productApiRoute.name
