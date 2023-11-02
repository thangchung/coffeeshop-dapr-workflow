include .env
export

.PHONY: publish-all-dockers
publish-all-dockers: publish-product-docker publish-counter-docker publish-barista-docker publish-kitchen-docker

.PHONY: publish-product-docker
publish-product-docker:
	dotnet publish ./product-api/product-api.csproj --os linux --arch x64 /t:PublishContainer -c Release
	docker tag product-api:latest ghcr.io/thangchung/coffeeshop-dapr-workflow/product-api:${IMAGE_TAG}
	docker rmi product-api:latest

.PHONY: publish-counter-docker
publish-counter-docker:
	dotnet publish ./counter-api/counter-api.csproj --os linux --arch x64 /t:PublishContainer -c Release
	docker tag counter-api:latest ghcr.io/thangchung/coffeeshop-dapr-workflow/counter-api:${IMAGE_TAG}
	docker rmi counter-api:latest

.PHONY: publish-barista-docker
publish-barista-docker:
	dotnet publish ./barista-api/barista-api.csproj --os linux --arch x64 /t:PublishContainer -c Release
	docker tag barista-api:latest ghcr.io/thangchung/coffeeshop-dapr-workflow/barista-api:${IMAGE_TAG}
	docker rmi barista-api:latest

.PHONY: publish-kitchen-docker
publish-kitchen-docker:
	dotnet publish ./kitchen-api/kitchen-api.csproj --os linux --arch x64 /t:PublishContainer -c Release
	docker tag kitchen-api:latest ghcr.io/thangchung/coffeeshop-dapr-workflow/kitchen-api:${IMAGE_TAG}
	docker rmi kitchen-api:latest