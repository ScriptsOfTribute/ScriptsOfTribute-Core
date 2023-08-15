AGENTS[0]="CamelCaseBot"
AGENTS[1]="BestMCTS3"
AGENTS[2]="BestMCTS5"

CSPROJ=$(cat <<-EOT
<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <OutputType>Library</OutputType>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>
        <LangVersion>10</LangVersion>
        <TargetFramework>netstandard2.1</TargetFramework>
        <RootNamespace>Bots</RootNamespace>
    </PropertyGroup>
    <ItemGroup>
      <ProjectReference Include="..\..\Engine\TalesOfTribute.csproj" />
    </ItemGroup>
</Project>
EOT
)

for agent in "${AGENTS[@]}"; do
    echo $CSPROJ > $agent/Bot.csproj
    dotnet build --configuration Release $agent
    cp $agent/bin/Release/netstandard2.1/Bot.dll $agent.dll
done

dotnet build --configuration Release ../GameRunner
cp ../GameRunner/bin/Release/net7.0/Bots.dll .
cp ../GameRunner/bin/Release/net7.0/cards.json .
