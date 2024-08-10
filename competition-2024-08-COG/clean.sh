AGENTS[0]="BestMCTS3"
AGENTS[1]="HQL_BOT"
AGENTS[2]="Sakkirin"
AGENTS[3]="SOISMCTS"
AGENTS[4]="ToT-BoT"

for agent in "${AGENTS[@]}"; do
  # No cleanup needed.
  if [ $agent == "ToT-BoT" ]; then
    rm -rf ToT-BoT/__pycache__ ToT-BoT-output.log
    continue;
  fi

  rm -rf $agent{/{bin,obj,Bot.csproj},.dll}
done

rm -rf ../GameRunner/{bin,obj,cards.json} Bots.dll cards.json *.txt
