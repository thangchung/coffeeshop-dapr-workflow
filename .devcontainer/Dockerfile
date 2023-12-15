FROM mcr.microsoft.com/dotnet/sdk:8.0.100-jammy

## https://github.com/dotnet/aspire/blob/main/docs/using-latest-daily.md
# RUN dotnet nuget add source --name dotnet8 https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet8/nuget/v3/index.json \
#     && dotnet workload update --skip-sign-check --source https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet8/nuget/v3/index.json \
#     && dotnet workload install aspire --skip-sign-check --source https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet8/nuget/v3/index.json \
#     && dotnet restore
