import pickle
import os
import numpy as np
import re
from itertools import product
import copy


#Important keys:
patron_key = {'TREASURY' : 0, 'ANSEI' : 1, 'DUKE_OF_CROWS' : 2, 'RAJHIN' : 3, 'PSIJIC' : 4, 'ORGNUM' : 5, 'HLAALU' : 6, 'PELIN' : 7, 'RED_EAGLE' : 8}
card_type_key = {'ACTION' : 1, 'AGENT' : 2, 'CONTRACT_ACTION' : 3, 'CONTRACT_AGENT' : 4, 'STARTER' : 1, 'CURSE' : 1}
effect_slot_key = {
    'GAIN_COIN' : 0,
    'GAIN_POWER' : 1,
    'GAIN_PRESTIGE' : 2,
    'OPP_LOSE_PRESTIGE' : 3,
    'REPLACE_TAVERN' : 4,
    'ACQUIRE_TAVERN' : 5,
    'DESTROY_CARD' : 6,
    'DRAW' : 7,
    'OPP_DISCARD' : 8,
    'RETURN_TOP' : 9,
    'TOSS' : 10,
    'KNOCKOUT' : 11,
    'PATRON_CALL' : 12,
    'CREATE_SUMMERSET_SACKING' : 13,
    'HEAL' : 14
    }

#Takes a dictionary card as input; outputs a list of multiple dictionaries, each containing a possible combination of the choosable effects for an OR card.
def split_or_conditions(input_list):
    # Create a list to hold sublists of options for each element
    options = []

    for item in input_list:
        if not isinstance(item, str) or item.strip() == "":
            options.append([item])
        elif 'OR' in item:
            # Split the item by 'OR' and trim any whitespace
            parts = [part.strip() for part in item.split(' OR ') if part.strip()]
            if not parts:
                options.append([item])  # If splitting results in an empty list, use the original item
            else:
                options.append(parts)
        else:
            options.append([item])

    # Generate all combinations of the options
    combinations = list(product(*options))

    # Convert each combination tuple into a list
    result_lists = [list(combo) for combo in combinations]

    return result_lists
def split_OR_cards(input_dict):
    effects_list = input_dict.get('effects', [])
    split_effects = split_or_conditions(effects_list)
    
    result_dicts = []
    for effects in split_effects:
        new_dict = copy.deepcopy(input_dict)  # Use deep copy to handle nested structures
        new_dict['effects'] = effects
        result_dicts.append(new_dict)
    
    return result_dicts

#Used in other functions to parse effects using effect_slot_key
def parse_effects(text):
    # Use regular expression to find all occurrences of the pattern 'EFFECT_NAME number'
    pattern = r"(\w+)\s+(\d+)"
    matches = re.finditer(pattern, text)
    
    # Convert each match into a tuple and collect them into a list
    result = [(match.group(1), int(match.group(2))) for match in matches]
    return result

#Takes dictionary card as an input; returns list of 61 integers describing a card.
def vectorize_card(card):
    vector = np.zeros(61)
    
    #vectorize deck that it comes from
    vector[0] = patron_key[card['deck']]
    
    #vectorize cost
    vector[1] = card['cost']
    
    #vectorize type (action, contract action, etc)
    vector[2] = card_type_key[card['type']]
    
    #vectorize HP
    vector[3] = card['HP']
    
    #vectorize taunts
    vector[4] = int(card['taunt'])
    
    #vectorize first level of effects
    effectsvec = np.zeros(56)
    
    combo_1_effects = card['effects'][0]
    for effect in parse_effects(combo_1_effects):
        effectsvec[effect_slot_key[effect[0]]] = effect[1]
    #vectorize second level
    combo_2_effects = card['effects'][1]
    for effect in parse_effects(combo_2_effects):
        effectsvec[int(effect_slot_key[effect[0]]) + 14] = effect[1]
    #vectorize third level
    combo_3_effects = card['effects'][2]
    for effect in parse_effects(combo_3_effects):
        effectsvec[int(effect_slot_key[effect[0]]) + 28] = effect[1]
    #vectorize fourth level
    combo_4_effects = card['effects'][3]
    for effect in parse_effects(combo_4_effects):
        effectsvec[int(effect_slot_key[effect[0]]) + 42] = effect[1]
        
    #Throw the effects vector into the rest of the vector
    vector[5:] = effectsvec
        
    return list(int(num) for num in vector)

#Used for hands, discard pile, etc. to fill out the rest of a vector (in list form) to a desired length.
def extend_vector(exlist, length):
    zeros_needed = length - len(exlist)
    if zeros_needed > 0:
        exlist.extend([0] * zeros_needed)
    return exlist
        
def vectorize_hand(hand):
    #make a list of variable length that contains all hand information
    hand_cards_list = []
    for card in hand:
        for combination in split_OR_cards(card):
            for attribute in (vectorize_card(combination)):
                hand_cards_list.append(attribute)
            
    #Fill the remaining space in the list with zeros until list is of length 620 (which is equivalent to ten (combinations of effects of) cards in hand)
    hand_cards_list = extend_vector(hand_cards_list, 610)
    
        
    return hand_cards_list

def map_gamestate_to_vector(gamestate):
    vec = np.zeros(14079)  # choose vector size as needed
    
    #TODO: add your logic for mapping gamestates to vectors here...
    
    #####-----Patrons and their favors-----#####
    #Map each patron to a specific integer to tell the bot which four patrons are in play.
    #My current approach also maps the Treasury patron, which is not important information.
    patron_list = []
    for patron in list(gamestate['PatronStates']):
        patron_list.append(patron_key[patron])
    vec[:5] = patron_list
    
    #Map each patron's favor to the next five slots in the representation. (-1, 0, 1) --> (favors opponent, favors nobody, favors agent)
    patron_favors = list(gamestate['PatronStates'].values())
    patron_favors_as_integers = []
    for favor in patron_favors:
        if favor == gamestate['CurrentPlayer']['Player']:
            patron_favors_as_integers.append(1)
        if favor == gamestate['EnemyPlayer']['Player']:
            patron_favors_as_integers.append(-1)
        if favor == 'NO_PLAYER_SELECTED':
            patron_favors_as_integers.append(0)
    vec[5:10] = patron_favors_as_integers
    
    #####-----Available cards in the Tavern-----#####
    tavern_cards_list = []
    for card in gamestate['TavernAvailableCards']:
        for attribute in vectorize_card(card):
            tavern_cards_list.append(attribute)
    tavern_cards_list = extend_vector(tavern_cards_list, 305)
    vec[10:315]= tavern_cards_list[:305]
    
    ####-----Housekeeping effects------#####
    
    #Board state type
    if gamestate['BoardState'] == 'NORMAL':
        vec[315] = 0
    if gamestate['BoardState'] == 'CHOICE_PENDING':
        vec[315] = 1
    if gamestate['BoardState'] == 'START_OF_TURN_CHOICE_PENDING':
        vec[315] = 2
    if gamestate['BoardState'] == 'PATRON_CHOICE_PENDING':
        vec[315] = 3

    #####-----Cards in active player's hand-----#####
    hand_cards_list = vectorize_hand(gamestate['CurrentPlayer']['Hand'])
    vec[316:926] = hand_cards_list[:610]
    
    #####-----Active player's cooldown pile-----#####
    ## Treats 'OR' cards as 'AND' cards to simplify code
    ## Caps cooldown pile at 30 cards;  others are ignored.

    cooldown_cards_list = []
    for card in gamestate['CurrentPlayer']['Cooldown']:
        for attribute in vectorize_card(card):
            cooldown_cards_list.append(attribute)
    cooldown_cards_list = extend_vector(cooldown_cards_list, 1830)
    vec[927:2757] = cooldown_cards_list[:1830]
    
    #####-----Active Player's played pile-----#####
    ## Treats 'OR' cards as 'AND' cards to simplify code
    ## Caps played pile at 30 cards; others are ignored.
    played_cards_list = []
    for card in gamestate['CurrentPlayer']['Played']:
        for attribute in vectorize_card(card):
            played_cards_list.append(attribute)
    played_cards_list = extend_vector(played_cards_list, 1830)
    vec[2757:4587] = played_cards_list[:1830]
    
    #####-----Active player's agents-----#####
    ## Caps active player's agents at 10; others are ignored.
    agents_list = []
    for agent in gamestate['CurrentPlayer']['Agents']:
        agents_list.append(int(agent['CurrentHP']))
        agents_list.append(int(agent['Activated']))
        for attribute in vectorize_card(agent['Card']):
            agents_list.append(attribute)
    agents_list = extend_vector(agents_list, 630)
    vec[4587:5217] = agents_list[:630]
    
    #####-----Active player's Draw Pile-----#####
    ## Treats 'OR' cards as 'AND' cards to simplify code
    ## Caps draw pile at 30 cards; others are ignored.
    draw_list = []
    for card in gamestate['CurrentPlayer']['DrawPile']:
        for attribute in vectorize_card(card):
            draw_list.append(attribute)
    draw_list = extend_vector(draw_list, 1830)
    vec[5217:7047] = draw_list[:1830]
        
    #####-----Active Player's known pile-----#####
    known_list = []
    for card in gamestate['CurrentPlayer']['KnownPile']:
        for attribute in vectorize_card(card):
            known_list.append(attribute)
    known_list = extend_vector(known_list, 305)
    vec[7047:7352] = known_list[:305]

    #####-----Opponent's Agents-----#####
    ## Caps opponent player's agents at 10; others are ignored.
    opp_agents_list = []
    for agent in gamestate['EnemyPlayer']['Agents']:
        opp_agents_list.append(int(agent['CurrentHP']))
        for attribute in vectorize_card(agent['Card']):
            opp_agents_list.append(attribute)
    opp_agents_list = extend_vector(opp_agents_list, 620)
    vec[7352:7972] = opp_agents_list[:620]
    
        
    #####-----Opp's hand and draw-----#####
    opp_hand_draw_list = []
    for card in gamestate['EnemyPlayer']['HandAndDraw']:
        for attribute in vectorize_card(card):
            opp_hand_draw_list.append(attribute)
    opp_hand_draw_list = extend_vector(opp_hand_draw_list, 2440)
    vec[7972:10412] = opp_hand_draw_list[:2440]
        
    #####-----Opp's cooldown pile-----#####
    ## Treats 'OR' cards as 'AND' cards to simplify code
    ## Caps cooldown pile at 30 cards;  others are ignored.
    opp_cooldown_cards_list = []
    for card in gamestate['EnemyPlayer']['Cooldown']:
        for attribute in vectorize_card(card):
            opp_cooldown_cards_list.append(attribute)
    opp_cooldown_cards_list = extend_vector(opp_cooldown_cards_list, 1830)
    vec[10412:12242] = opp_cooldown_cards_list[:1830]

    #####-----Opp's played pile-----#####
    ## Treats 'OR' cards as 'AND' cards to simplify code
    ## Caps cooldown pile at 30 cards;  others are ignored.
    opp_played_cards_list = []
    for card in gamestate['EnemyPlayer']['Played']:
        for attribute in vectorize_card(card):
            opp_played_cards_list.append(attribute)
    opp_played_cards_list = extend_vector(opp_played_cards_list, 1830)
    vec[12242:14072] = opp_played_cards_list[:1830]
    
    #####-----Misc-----#####
    vec[14072] = gamestate['EnemyPlayer']['Power']
    vec[14073] = gamestate['EnemyPlayer']['Coins']
    vec[14074] = gamestate['EnemyPlayer']['Prestige']
    
    vec[14075] = gamestate['CurrentPlayer']['Power']
    vec[14076] = gamestate['CurrentPlayer']['Coins']
    vec[14077] = gamestate['CurrentPlayer']['Prestige']
    vec[14078] = gamestate['CurrentPlayer']['PatronCalls']
    
    return vec


if __name__ == "__main__":

    # load actions from file
    # TODO: change filepath to the correct path, either use relative or absolute path
    gamestate_filename = 'past_gamestates.pkl'
    gamestate_filename = "C:/Users/lashm/OneDrive/Desktop/ActionsGamestates/past_gamestates.pkl"

    # Check if the file exists
    assert os.path.exists(gamestate_filename), 'File not found'

    # Open the file in binary read mode and load the data using pickle
    with open(gamestate_filename, 'rb') as file:
        gamestates = []
        while 1:
            try:
                gamestates.append(pickle.load(file))
            except EOFError:
                break
            
    # this should create a two-dimensional array with each row representing a gamestate as the vector
    #games = np.array([map_gamestate_to_vector(gamestate) for gamestate in gamestates])
    for game in gamestates:
        print(map_gamestate_to_vector(game))