import json
import sys
import random
import socket
import pickle 
import numpy as np
import struct
import math


from map_gamestate_to_vector import map_gamestate_to_vector
from map_action_to_vector import map_action_to_vector
from card_rating_function import rate_card, convert_gamestate_cardname_to_cardsjson_cardname

END_OF_TRANSMISSION = "EOT"
FINISHED_TOKEN = "FINISHED"

PATRON_MAPPING = {
    "ALMALEXIA": 0,
    "ANSEI": 1,
    "DUKE_OF_CROWS": 2,
    "HLAALU": 3,
    "HERMAEUS_MORA": 4,
    "PSIJIC_LOREMASTER_CELARUS": 5,
    "RAJHIN": 6,
    "RED_EAGLE": 7,
    "ALESSIA": 8,
    "PELIN": 9,
    "ORGNUM": 10,
    "DRUID_KING": 11,
    "TREASURY": 12,
}

# This script is not really an agent itself, it just represents a bridge between the game and the agent running separate programs
# The sot_rl_environment.py script will run Scripts of Tribute using this bridge and a random agent to play against
# This bridge will pass information from the gamestates to the sot_rl_environment.py and receive actions from it
# These actions will be forwarded to Scripts of Tribute

def update_win_loss(winner, file_path):
    # Initialize default counts
    wins = 0
    losses = 0
    
    # Attempt to read the file
    try:
        with open(file_path, 'r') as file:
            lines = file.readlines()
            if len(lines) >= 2:
                wins = int(lines[0].split(': ')[1])
                losses = int(lines[1].split(': ')[1])
            else:
                print("File format is incorrect, initializing counts to zero.")
    except FileNotFoundError:
        print(f"File not found: {file_path}, initializing new file with counts.")
    except ValueError as e:
        print(f"Error parsing file: {e}, initializing counts to zero.")
    except IndexError as e:
        print(f"File content is incorrect: {e}, initializing counts to zero.")

    # Update the counts based on the winner
    if winner == 'PLAYER1':
        wins += 1
    elif winner == 'PLAYER2':
        losses += 1
    else:
        raise ValueError("Winner must be either 'PLAYER1' or 'PLAYER2'")

    # Write the updated counts back to the file
    with open(file_path, 'w') as file:
        file.write(f'WINS: {wins}\n')
        file.write(f'LOSSES: {losses}\n')

# Example usage:
# update_win_loss('PLAYER1', 'C:\\Users\\lashm\\OneDrive\\Desktop\\Competition Submission\\SoT_RL_WORKING\\ScriptsOfTribute-Core-master\\Bots\\ExternalLanguageBotsUtils\\Python\\Win_Loss_Record.txt')
# update_win_loss('PLAYER2', 'C:\\Users\\lashm\\OneDrive\\Desktop\\Competition Submission\\SoT_RL_WORKING\\ScriptsOfTribute-Core-master\\Bots\\ExternalLanguageBotsUtils\\Python\\Win_Loss_Record.txt')



def get_game_state():
    """ Function for retrieving the game state from the Scripts of Tribute
    """
    data = ''
    while (data_fraction := input()) != END_OF_TRANSMISSION:
        data += data_fraction
    #debug(data)
    if FINISHED_TOKEN in data:
        # Game is over
        _, winner, reason, context = data[data.find(FINISHED_TOKEN):].split(' ', 3)
        debug(winner +" " +  reason + " " + context)
        # Note from the organizers: This part of the code has been changed to fix the paths.
        update_win_loss(winner, os.path.join(os.path.dirname(__file__), "Win_Loss_Record.txt"))

        return (winner, reason, context), True

    return json.loads(data), False

def get_patrons_to_pick():
    """ function for receiving the patrons from the Scripts of Tribute
    """
    data = input()
    patrons, round_nr = data.split()
    patrons = patrons.split(',')

    return patrons, round_nr

def encode_patrons(patrons):
    vec = np.zeros((13,), dtype=bool)
    for p in patrons:
        vec[PATRON_MAPPING[p]] = True
    return vec

def debug(msg):
    """
    Alternative Print function for debugging, because print is used to communicate with Scripts of Tribute
    """
    print(msg, file=sys.stderr)

# I aimed to provide you some little helper functions
# for sending and receiving information between the agent and the environment
def send_data_to_rl_environment(socket, data):
    # Serialize your data
    data = pickle.dumps(data)

    # Prefix the message with its length, packed into 4 bytes
    prefixed_data = struct.pack('!I', len(data)) + data

    # Send the prefixed message
    s.sendall(prefixed_data)

def receive_data_from_rl_environment(socket):
    # First, receive the length of the incoming pickle data (4 bytes for 'I' format)
    length_bytes = socket.recv(4)
    message_length = struct.unpack('!I', length_bytes)[0]

    # Now that you know the length, receive the rest of the message
    message = b''
    while len(message) < message_length:
        part = socket.recv(message_length - len(message))
        if not part:
            raise Exception("Connection closed or error")
        message += part

    # Unpickle the complete message
    return pickle.loads(message)

def evaluate_action_space(actions, gamestate, weight_vector):
    """
    Function for mapping the action space to a matrix
    """
    action_space = [] # List to be filled with vectors
    replace_with_heuristic = []
    for i, action in enumerate(actions):
        mapped_action = map_action_to_vector(action, gamestate)
        if mapped_action is None:
            mapped_action = np.zeros((105,))
            replace_with_heuristic.append(i)
        action_space.append(mapped_action)

    action_space = np.array(action_space) # Turns list of action vectors into matrix of action vectors
    
    
    # So now we have a matrix of actions where each row describes one action.
    # For the actions that have to be heuristic'd:
    # They are currently just inserted into the action matrix as normal but all their values are zero.
    weighted_action_space = weight_vector @ action_space.T # weighted_action_space is of shape (n,) where n is the # of legal moves. Each number is the total value of its associated action.


    for i in replace_with_heuristic:
        weighted_action_space[i] = 0
        # todo add heuristic card value of this action
        if actions[i].startswith("END_TURN"):
            continue # So END_TURN always has a value of zero --> The bot can't choose to end turn when other actions are available. Unless the other actions have negative values.
        else:
            cardname = actions[i].split("CHOICE ")[1].strip()
            if cardname == "": # If it's just "CHOICE" with no card attached
                continue
            
            if cardname.upper().strip() == cardname.strip():
                card_values = []
                for card in cardname.split(" "):
                    card_values.append(rate_card(
                        convert_gamestate_cardname_to_cardsjson_cardname(card), 
                        gamestate, 
                        gamestate['CurrentPlayer'])
                    )
                sorted_card_values = sorted(card_values, reverse=True) #Lines up the card heuristic values into a list from best to worst
                weighted_action_space[i] = sum(sorted_card_values[:gamestate['PendingChoice']['MaxChoices']])
                #takes the best n cards where n = max # of choices. Therefore, if a card lets us choose 1 2 OR 3 cards, we'll always choose the max # of cards.
                
                #So there's a slot in the weighted action space that corresponds to all "CHOICE" effects, and it only has the value of the best possible one according to the heuristic.
                continue

            weighted_action_space[i] = rate_card(
                convert_gamestate_cardname_to_cardsjson_cardname(cardname), 
                gamestate, 
                gamestate['CurrentPlayer']
            )
                
    return weighted_action_space


if __name__ == '__main__':
    # Create a socket object
    s = socket.socket(socket.AF_INET, socket.SOCK_STREAM) 

    # Define the server address and port
    server_address = ('localhost', 12345)

    # Connect to the server
    s.connect(server_address)

    # The following loop will send the patrons to the environment and receive the actions (patrons to pick) from the environment
    for _ in range(2):
        # choose random patrons, for better generalization
        patrons, round_nr = get_patrons_to_pick()
        print(random.choice(patrons))
        # the chose commented here, was for letting the neural network choose which patrons to pick
        #debug(encode_patrons(patrons))
        #s.sendall(pickle.dumps(("Patrons", encode_patrons(patrons), round_nr)))
        #preference_vec = pickle.loads(s.recv(1024))
        #filtered_preference_vec = [preference_vec[PATRON_MAPPING[p]] for p in patrons]
        #action = patrons[np.argmax(filtered_preference_vec)]

        #debug(patrons)
        #debug(patrons[action])
        #debug("Action: " +  str(action))
        #print(action) 

    # while the game goes on, we will retrieve the game state from Scripts of Tribute and send it to sol_rl_environment.py
    # Next, we listen to sol_rl_enviroment.py for picking an action and sending it to Scripts of Tribute
    # no further logic will be stored in here
    import os
    while True:
        data, finished = get_game_state()
        if finished:
            debug(data)
            break
        
        gamestate = map_gamestate_to_vector(data["State"])
        #debug(str(len(gamestate)) + str(gamestate))
        
        prestige = data['State']['CurrentPlayer']['Prestige']
        
        normalized_reward = lambda prestige, k=1/5, P_target=80: (math.exp(k * prestige) - 1) / (math.exp(k * P_target) - 1)

        
        reward = normalized_reward(prestige)
        #debug(f'Prestige: {prestige} | Reward: {reward}')
        send_data_to_rl_environment(s, ("Gamestate", gamestate, reward))

        weight_vector = receive_data_from_rl_environment(s)
        
        #debug(data["Actions"])

        # if the game is not finished yet, we want to evaluate the actions given the provided weight vector
        action_preferences = evaluate_action_space(data["Actions"], data["State"], weight_vector)
        #debug( "action preferences: " + str(action_preferences))
        action = np.argmax(action_preferences)
        print(action)

        # random actions for testing purposes
        #action = random.choice(range(len(data["Actions"])))
        #print(action)

    #debug("Reward: " +  str(data[0]=="Player1"))
    send_data_to_rl_environment(s, ("Reward", int(data[0]=="PLAYER1")))
    s.close()
