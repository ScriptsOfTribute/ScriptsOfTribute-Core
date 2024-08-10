import json
import sys

END_OF_TRANSMISSION = "EOT"
FINISHED_TOKEN = "FINISHED"

def get_game_state():
    data = ''
    while (data_fraction := input()) != END_OF_TRANSMISSION:
        data += data_fraction
    if data.startswith(FINISHED_TOKEN):
        # Game is over
        _, winner, reason, context = data.split(' ', 3)
        return (winner, reason, context), True
    return json.loads(data), False

def get_patrons_to_pick():
    data = input()
    patrons, round_nr = data.split()
    patrons = patrons.split(',')

    return patrons, round_nr

def debug(msg):
    print(msg, file=sys.stderr)