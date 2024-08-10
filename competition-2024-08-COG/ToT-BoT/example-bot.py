import json
import sys
import random
import socket
import pickle 
import os

END_OF_TRANSMISSION = "EOT"
FINISHED_TOKEN = "FINISHED"

def get_game_state():
    data = ''
    while (data_fraction := input()) != END_OF_TRANSMISSION:
        data += data_fraction
    #debug(data)
    if data.startswith(FINISHED_TOKEN):
        # Game is over
        _, winner, reason, context = data.split(' ', 3)
        debug(winner)
        debug(reason)
        debug(context)
        return (winner, reason, context), True
    return json.loads(data), False

def get_patrons_to_pick():
    data = input()
    patrons, round_nr = data.split()
    patrons = patrons.split(',')

    return patrons, round_nr
    
def debug(msg):
    print(msg, file=sys.stderr)

if __name__ == '__main__':

    action_filename = 'actions.pkl'
    gamestate_filename = 'gamestate.pkl'

    # Check if the file exists
    if os.path.exists(action_filename):
        # Open the file in binary read mode and load the data using pickle
        with open(action_filename, 'rb') as file:
            past_actions = pickle.load(file)
    else:
        #Initialize past_actions as an empty list if the file does not exist
        past_actions = set()

    
    for _ in range(2):
        patrons, round_nr = get_patrons_to_pick()
        #debug(f'Received: {patrons} in round {round_nr}')
        print(random.choice(patrons))
        
    with open(gamestate_filename, 'a+b') as gs_file:
        
        while True:
            data, finished = get_game_state()
            past_actions.update(data["Actions"])

            with open(action_filename, 'wb') as file:
                pickle.dump(past_actions, file)
            
            #if "State" in data:
            #    pickle.dump(data["State"], gs_file)


            if finished:
                #debug(data)
                break

            #data["State"]

            action = random.choice(range(len(data["Actions"])))
            print(action)
