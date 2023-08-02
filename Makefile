include .env
export

run-product-dapr:
	cd product-api && \
	spin build && \
	dapr run \
    --app-id productapi \
    --app-port 5001 \
    --resources-path ../components \
    --config ../components/daprConfig.yaml \
    -- dotnet run --project product-api/ProductApi.csproj && \
	cd -
.PHONY: run-product-dapr

run-counter-dapr:
	dapr run \
    --app-id counterapi \
    --app-port 5002 \
    --dapr-http-port 54321 \
    --resources-path components \
    --config components/daprConfig.yaml \
    -- dotnet run --project counter-api/counter-api.csproj \
	cd -
.PHONY: run-counter-dapr

run-barista-dapr:
	cd barista-api && \
	spin build && \
	dapr run \
    --app-id baristaapi \
    --app-port 5003 \
    --resources-path ../components \
    --config ../components/daprConfig.yaml \
    -- dotnet run --project barista-api/BaristaApi.csproj && \
	cd -
.PHONY: run-barista-dapr

run-kitchen-dapr:
	cd kitchen-api && \
	spin build && \
	dapr run \
    --app-id kitchenaapi \
    --app-port 5004 \
    --resources-path ../components \
    --config ../components/daprConfig.yaml \
    -- dotnet run --project kitchen-api/KitchenApi.csproj && \
	cd -
.PHONY: run-kitchen-dapr