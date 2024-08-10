AGENTS[0]="BestMCTS3"
AGENTS[1]="HQL_BOT"
AGENTS[2]="Sakkirin"
AGENTS[3]="SOISMCTS"
AGENTS[4]="ToT-BoT"

for i in {1..1000}; do
  seed=$RANDOM # Same for a whole epoch.
  for j in {1..10}; do
    for playerA in "${AGENTS[@]}"; do
      for playerB in "${AGENTS[@]}"; do
        if [ $playerA = $playerB ]; then continue; fi
        echo -n "$(date +%FT%T) '$playerA' '$playerB' "

        # Replace ToT-BoT with the real command.
        agentA="$playerA"
        agentB="$playerB"
        if [ "$agentA" = "ToT-BoT" ]; then agentA="cmd:python3 ToT-BoT/rl-bridge.py"; fi
        if [ "$agentB" = "ToT-BoT" ]; then agentB="cmd:python3 ToT-BoT/rl-bridge.py"; fi

        dotnet run --configuration Release --project ../GameRunner --seed $seed "$agentA" "$agentB" | tr '\n' ' '
        echo
      done
    done
  done
done
