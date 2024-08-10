AGENTS[0]="BestMCTS3"
AGENTS[1]="HQL_BOT"
AGENTS[2]="Sakkirin"
AGENTS[3]="SOISMCTS"
AGENTS[4]="ToT-BoT"

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
  # No compilation needed.
  if [ $agent == "ToT-BoT" ]; then continue; fi

  echo $CSPROJ > $agent/Bot.csproj
  dotnet build --configuration Release $agent
  cp $agent/bin/Release/netstandard2.1/Bot.dll $agent.dll
done

dotnet build --configuration Release ../GameRunner
cp ../GameRunner/bin/Release/net7.0/Bots.dll .
cp ../GameRunner/bin/Release/net7.0/cards.json .

# Start ToT-BoT server.
kill $(lsof -t -i:12345)
nohup python3 ToT-BoT/RLTraining/sot_rl_environment.py > ToT-BoT-output-4.log 2>&1 &
disown
