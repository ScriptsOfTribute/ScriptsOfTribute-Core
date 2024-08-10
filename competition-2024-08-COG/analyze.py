from contextlib import ExitStack
from glob import glob
from itertools import chain, zip_longest
from re import compile

summary = compile(r"^.*?'(?P<player1>.*?)' '(?P<player2>.*?)'.*?(?:(?P<fail>ConnectionRefusedError)|draws: (?P<draw>\d).*?P1 wins: (?P<win1>\d).*?other factors: (?P<errs>\d)).*?$")

def analyze(chunks, stats):
  for chunk in filter(None, chunks):
    for line in filter(None, chunk):
      match = summary.match(line)
      if match is not None:
        analyze_line(match.groupdict(), stats)
    yield

def analyze_line(match, stats):
  player1 = match['player1']
  player2 = match['player2']

  pairA = (player1, player2)
  pairB = (player2, player1)

  if pairA not in stats: stats[pairA] = stats_new()
  if pairB not in stats: stats[pairB] = stats_new()

  # Map ToT-BoT fails into errors.
  # if match['fail']:
  #   if player1 == 'ToT-BoT' or player2 == 'ToT-BoT':
  #     match['draw'] = '0'
  #     match['win1'] = '2' if player1 == 'ToT-BoT' else '1'
  #     match['errs'] = '1'

  if match['draw'] == '0':
    if match['win1'] == '1':
      stats[pairA]['wins'] += 1
      stats[pairB]['errs' if match['errs'] == '1' else 'lost'] += 1
    else:
      stats[pairA]['errs' if match['errs'] == '1' else 'lost'] += 1
      stats[pairB]['wins'] += 1

def graph(paths, chunk, limit=None):
  header = True
  stats = {}

  interleave = lambda xs: chain.from_iterable(zip_longest(*xs))
  chunkify = lambda xs, n: zip_longest(*([iter(xs)] * n))

  with ExitStack() as stack:
    files = [stack.enter_context(open(path, buffering=1)) for path in paths]
    chunks = interleave(chunkify(file, chunk) for file in files)
    for _ in analyze(chunks, stats):
      players = {}
      for (player1, player2), results in stats.items():
        if player1 != player2:
          players[player1] = stats_combine(players.get(player1), results)
      if header:
        header = False
        print(';'.join(sorted(players.keys())).replace('_', '\\\\_'))
      graph_print(players)
      if limit is not None:
        limit -= 1
        if limit <= 0:
          break

  return stats

def graph_print(players):
  def single(pair):
    _, stats = pair
    alls = stats['errs'] + stats['lost'] + stats['wins']
    wins = stats['wins'] / alls * 100
    return f'{wins:6.2f}'
  print(';'.join(map(single, sorted(players.items()))))

def score(stats):
  players = {}

  for (player1, player2), results in sorted(stats.items()):
    if player1 != player2:
      players[player1] = stats_combine(players.get(player1), results)
      score_print(f'{player1:>9} {player2:>9}', results)

  order = lambda pair: score_count(pair[1])[2]
  for player1, results in sorted(players.items(), key=order, reverse=True):
    score_print(f'{player1:>9}', results)

def score_count(stats):
  alls = stats['errs'] + stats['lost'] + stats['wins']
  errs = stats['errs'] / alls * 100
  wins = stats['wins'] / alls * 100
  return alls, errs, wins

def score_print(title, stats, extra = ''):
  alls, errs, wins = score_count(stats)
  print(f'{title} wins={wins:6.2f}% errs={errs:6.2f}% alls={alls // 2}{extra}')

def stats_combine(statsA, statsB):
  if statsA is None:
    statsA = {'errs': 0, 'lost': 0, 'wins': 0}

  if statsB is not None:
    statsA['errs'] += statsB['errs']
    statsA['lost'] += statsB['lost']
    statsA['wins'] += statsB['wins']

  return statsA

def stats_new():
  return stats_combine(None, None)

if __name__ == '__main__':
  files = sorted(glob('out-*.txt'))
  score(graph(files, 5 ** 2, 500))
