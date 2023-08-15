AGENTS[0]="BeamSearchBot"
AGENTS[1]="BestMCTS3"
AGENTS[2]="BestMCTS5"
AGENTS[3]="CamelCaseBot"
AGENTS[4]="DecisionTreeBot"
AGENTS[5]="MCTSBot"
AGENTS[6]="MaxPrestigeBot"
AGENTS[7]="RandomSimulationBot"

for i in {1..1000}; do
  seed=$RANDOM # Same for a whole epoch.
  for j in {1..10}; do
    for playerA in "${AGENTS[@]}"; do
      for playerB in "${AGENTS[@]}"; do
        echo -n "$(date +%FT%T) '$playerA' '$playerB' "
        dotnet run --configuration Release --project ../GameRunner --seed $seed $playerA $playerB | tr '\n' ' '
        echo
      done
    done
  done
done
