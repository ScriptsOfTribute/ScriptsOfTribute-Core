```sh
# Stock Ubuntu 24.04
source setup.sh
source prepare.sh
source run.sh | tee out-1.txt & # One per server.
source run.sh | tee out-2.txt & # One per server.
source run.sh | tee out-3.txt & # One per server.
source run.sh | tee out-4.txt & # One per server.
wait
source graph.sh
```
