AGENTS[0]="CamelCaseBot"
AGENTS[1]="BestMCTS3"
AGENTS[2]="BestMCTS5"

for agent in "${AGENTS[@]}"; do
  rm -rf $agent{/{bin,obj,Bot.csproj},.dll}
done

rm -rf ../GameRunner/{bin,obj,cards.json} Bots.dll cards.json *.txt
