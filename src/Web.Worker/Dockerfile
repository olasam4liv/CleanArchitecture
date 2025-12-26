FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY ["Directory.Packages.props", "Directory.Packages.props"]
COPY ["Directory.Build.props", "Directory.Build.props"]
COPY ["src/SharedKernel/SharedKernel.csproj", "src/SharedKernel/"]
COPY ["src/Domain/Domain.csproj", "src/Domain/"]
COPY ["src/Application/Application.csproj", "src/Application/"]
COPY ["src/Web.Worker/Web.Worker.csproj", "src/Web.Worker/"]

RUN dotnet restore "src/Web.Worker/Web.Worker.csproj"

COPY ["src/SharedKernel", "src/SharedKernel"]
COPY ["src/Domain", "src/Domain"]
COPY ["src/Application", "src/Application"]
COPY ["src/Web.Worker", "src/Web.Worker"]

WORKDIR "/source/src/Web.Worker"
RUN dotnet build "Web.Worker.csproj" -c Release -o /app/build /p:TreatWarningsAsErrors=false /p:CodeAnalysisTreatWarningsAsErrors=false

FROM build AS publish
RUN dotnet publish "Web.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false /p:TreatWarningsAsErrors=false /p:CodeAnalysisTreatWarningsAsErrors=false

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Web.Worker.dll"]
