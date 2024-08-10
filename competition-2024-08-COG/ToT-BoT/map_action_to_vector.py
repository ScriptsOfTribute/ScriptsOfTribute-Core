import os
import pickle
import re
import json
import numpy as np


#Set up other constants
example_actions = ['ACTIVATE_AGENT BLACKFEATHER_KNAVE',
    'ACTIVATE_AGENT BLACKFEATHER_KNIGHT',
    'ACTIVATE_AGENT HEL_SHIRA_HERALD',
    'ACTIVATE_AGENT HIRELING',
    'ACTIVATE_AGENT JEERING_SHADOW',
    'ACTIVATE_AGENT KARTH_MANHUNTER',
    'ACTIVATE_AGENT KNIGHTS_OF_SAINT_PELIN',
    'ACTIVATE_AGENT NO_SHIRA_POET',
    'ACTIVATE_AGENT OATHMAN',
    'ACTIVATE_AGENT PROWLING_SHADOW',
    'ACTIVATE_AGENT SERPENTGUARD_RIDER',
    'ACTIVATE_AGENT SHIELD_BEARER',
    'ACTIVATE_AGENT SNAKESKIN_FREEBOOTER',
    'ACTIVATE_AGENT STORM_SHARK_WAVECALLER',
    'ACTIVATE_AGENT STUBBORN_SHADOW',
    'ATTACK BANGKORAI_SENTRIES',
    'ATTACK BLACKFEATHER_BRIGAND',
    'ATTACK BLACKFEATHER_KNAVE',
    'ATTACK BLACKFEATHER_KNIGHT',
    'ATTACK CLANWITCH',
    'ATTACK ELDER_WITCH',
    'ATTACK HEL_SHIRA_HERALD',
    'ATTACK JEERING_SHADOW',
    'ATTACK KARTH_MANHUNTER',
    'ATTACK KNIGHTS_OF_SAINT_PELIN',
    'ATTACK NO_SHIRA_POET',
    'ATTACK OATHMAN',
    'ATTACK PROWLING_SHADOW',
    'ATTACK SERPENTGUARD_RIDER',
    'ATTACK SHIELD_BEARER',
    'ATTACK SNAKESKIN_FREEBOOTER',
    'ATTACK STORM_SHARK_WAVECALLER',
    'ATTACK STUBBORN_SHADOW',
    'BUY_CARD AMBUSH',
    'BUY_CARD ANSEIS_VICTORY',
    'BUY_CARD ANSEI_ASSAULT',
    'BUY_CARD ARCHERS_VOLLEY',
    'BUY_CARD BAG_OF_TRICKS',
    'BUY_CARD BANGKORAI_SENTRIES',
    'BUY_CARD BARTERER',
    'BUY_CARD BATTLE_MEDITATION',
    'BUY_CARD BLACKFEATHER_BRIGAND',
    'BUY_CARD BLACKFEATHER_KNAVE',
    'BUY_CARD BLACKFEATHER_KNIGHT',
    'BUY_CARD BLACKMAIL',
    'BUY_CARD BLACK_SACRAMENT',
    'BUY_CARD BLOODY_OFFERING',
    'BUY_CARD BLOOD_SACRIFICE',
    'BUY_CARD BONFIRE',
    'BUY_CARD BRIARHEART_RITUAL',
    'BUY_CARD CLANWITCH',
    'BUY_CARD CONQUEST',
    'BUY_CARD CURRENCY_EXCHANGE',
    'BUY_CARD CUSTOMS_SEIZURE',
    'BUY_CARD EBONY_MINE',
    'BUY_CARD ELDER_WITCH',
    'BUY_CARD GHOSTSCALE_SEA_SERPENT',
    'BUY_CARD GRAND_LARCENY',
    'BUY_CARD GRAND_ORATORY',
    'BUY_CARD HAGRAVEN',
    'BUY_CARD HAGRAVEN_MATRON',
    'BUY_CARD HARVEST_SEASON',
    'BUY_CARD HEL_SHIRA_HERALD',
    'BUY_CARD HIRAS_END',
    'BUY_CARD HIRELING',
    'BUY_CARD HLAALU_COUNCILOR',
    'BUY_CARD HLAALU_KINSMAN',
    'BUY_CARD HOSTILE_TAKEOVER',
    'BUY_CARD HOUSE_EMBASSY',
    'BUY_CARD HOUSE_MARKETPLACE',
    'BUY_CARD IMPERIAL_PLUNDER',
    'BUY_CARD IMPERIAL_SPOILS', 
    'BUY_CARD IMPRISONMENT',
    'BUY_CARD JARRING_LULLABY', 
    'BUY_CARD JEERING_SHADOW',
    'BUY_CARD KARTH_MANHUNTER',
    'BUY_CARD KING_ORGNUMS_COMMAND',
    'BUY_CARD KNIGHTS_OF_SAINT_PELIN',
    'BUY_CARD KNIGHT_COMMANDER',
    'BUY_CARD KWAMA_EGG_MINE',
    'BUY_CARD LAW_OF_SOVEREIGN_ROOST',
    'BUY_CARD LEGIONS_ARRIVAL',
    'BUY_CARD LUXURY_EXPORTS',
    'BUY_CARD MAORMER_BOARDING_PARTY',
    'BUY_CARD MAORMER_CUTTER',
    'BUY_CARD MARCH_ON_HATTU',
    'BUY_CARD MIDNIGHT_RAID',
    'BUY_CARD MOONLIT_ILLUSION',
    'BUY_CARD MURDER_OF_CROWS',
    'BUY_CARD NO_SHIRA_POET',
    'BUY_CARD OATHMAN',
    'BUY_CARD PILFER',
    'BUY_CARD PLUNDER',
    'BUY_CARD POOL_OF_SHADOW',
    'BUY_CARD POUNCE_AND_PROFIT',
    'BUY_CARD PROWLING_SHADOW',
    'BUY_CARD PYANDONEAN_WAR_FLEET',
    'BUY_CARD RAGPICKER',
    'BUY_CARD RALLY',
    'BUY_CARD REINFORCEMENTS',
    'BUY_CARD RINGS_GUILE',
    'BUY_CARD SCRATCH',
    'BUY_CARD SEA_RAIDERS_GLORY',
    'BUY_CARD SEA_SERPENT_COLOSSUS',
    'BUY_CARD SERPENTGUARD_RIDER',
    'BUY_CARD SERPENTPROW_SCHOONER',
    'BUY_CARD SHADOWS_SLUMBER',
    'BUY_CARD SHEHAI_SUMMONING',
    'BUY_CARD SHIELD_BEARER',
    'BUY_CARD SIEGE_WEAPON_VOLLEY',
    'BUY_CARD SLIGHT_OF_HAND',
    'BUY_CARD SNAKESKIN_FREEBOOTER',
    'BUY_CARD SQUAWKING_ORATORY',
    'BUY_CARD STORM_SHARK_WAVECALLER',
    'BUY_CARD STUBBORN_SHADOW',
    'BUY_CARD SUMMERSET_SACKING',
    'BUY_CARD THE_ARMORY',
    'BUY_CARD THE_PORTCULLIS',
    'BUY_CARD TITHE',
    'BUY_CARD TOLL_OF_FLESH',
    'BUY_CARD TOLL_OF_SILVER',
    'BUY_CARD TWILIGHT_REVELRY',
    'BUY_CARD WARRIOR_WAVE',
    "CHOICE ARCHERS'_VOLLEY",
    'CHOICE BANGKORAI_SENTRIES',
    'CHOICE BEWILDERMENT',
    'CHOICE BEWILDERMENT BEWILDERMENT',
    'CHOICE BEWILDERMENT GOLD',
    'CHOICE BEWILDERMENT TOLL_OF_SILVER',
    'CHOICE BEWILDERMENT WRIT_OF_COIN',
    'CHOICE BLACKFEATHER_BRIGAND',
    'CHOICE BLACKFEATHER_KNAVE',
    'CHOICE BLACKFEATHER_KNIGHT',
    'CHOICE Battle Meditation',
    'CHOICE CONQUEST',
    'CHOICE CURRENCY_EXCHANGE',
    'CHOICE CUSTOMS_SEIZURE',
    'CHOICE Conquest',
    'CHOICE FORTIFY',
    'CHOICE FORTIFY GOLD',
    'CHOICE FORTIFY GOLD WRIT_OF_COIN',
    'CHOICE FORTIFY GOODS_SHIPMENT',
    'CHOICE FORTIFY WRIT_OF_COIN',
    'CHOICE GHOSTSCALE_SEA_SERPENT',
    'CHOICE GOLD',
    'CHOICE GOLD BEWILDERMENT',
    'CHOICE GOLD BLACKFEATHER_KNIGHT',
    'CHOICE GOLD FORTIFY',
    'CHOICE GOLD GOLD',
    'CHOICE GOLD GOLD SUMMERSET_SACKING',
    'CHOICE GOLD GOLD WRIT_OF_COIN',
    'CHOICE GOLD GOODS_SHIPMENT',
    'CHOICE GOLD PECK',
    'CHOICE GOLD TOLL_OF_FLESH SEA_ELF_RAID',
    'CHOICE GOLD TOLL_OF_FLESH WRIT_OF_COIN',
    'CHOICE GOLD TOLL_OF_SILVER',
    'CHOICE GOLD WAR_SONG',
    'CHOICE GOLD WAY_OF_THE_SWORD',
    'CHOICE GOLD WRIT_OF_COIN',
    'CHOICE GOLD WRIT_OF_COIN GOLD',
    'CHOICE GOLD WRIT_OF_COIN SEA_ELF_RAID',
    'CHOICE GOLD WRIT_OF_COIN SUMMERSET_SACKING',
    'CHOICE GOLD WRIT_OF_COIN WRIT_OF_COIN',
    'CHOICE GOODS_SHIPMENT',
    'CHOICE GOODS_SHIPMENT FORTIFY',
    'CHOICE GOODS_SHIPMENT GOLD',
    'CHOICE GOODS_SHIPMENT WAR_SONG',
    'CHOICE GOODS_SHIPMENT WAY_OF_THE_SWORD',
    'CHOICE GOODS_SHIPMENT WRIT_OF_COIN',
    'CHOICE GRAND_LARCENY',
    'CHOICE Grand Oratory',
    'CHOICE HEL_SHIRA_HERALD',
    "CHOICE HIRA'S_END",
    'CHOICE HIRELING',
    'CHOICE HOSTILE_TAKEOVER',
    "CHOICE Hira's End",
    'CHOICE JEERING_SHADOW',
    'CHOICE KNIGHTS_OF_SAINT_PELIN',
    'CHOICE KNIGHT_COMMANDER',
    "CHOICE LEGION'S_ARRIVAL",
    'CHOICE LUXURY_EXPORTS',
    'CHOICE MAORMER_BOARDING_PARTY',
    'CHOICE MARCH_ON_HATTU',
    'CHOICE MIDNIGHT_RAID',
    'CHOICE MURDER_OF_CROWS',
    'CHOICE March on Hattu',
    'CHOICE OATHMAN',
    'CHOICE PECK',
    'CHOICE PECK GOLD',
    'CHOICE PILFER',
    'CHOICE PLUNDER',
    'CHOICE PLUNDER BEWILDERMENT',
    'CHOICE PLUNDER GOLD',
    'CHOICE PLUNDER WRIT_OF_COIN',
    'CHOICE POOL_OF_SHADOW',
    'CHOICE POUNCE_AND_PROFIT',
    'CHOICE PROWLING_SHADOW',
    'CHOICE PYANDONEAN_WAR_FLEET',
    'CHOICE RALLY',
    'CHOICE REINFORCEMENTS',
    'CHOICE SCRATCH',
    'CHOICE SEA_ELF_RAID',
    'CHOICE SEA_SERPENT_COLOSSUS',
    'CHOICE SERPENTGUARD_RIDER',
    'CHOICE SERPENTPROW_SCHOONER',
    'CHOICE SHEHAI_SUMMONING',
    'CHOICE SHIELD_BEARER',
    'CHOICE SIEGE_WEAPON_VOLLEY',
    'CHOICE SLIGHT_OF_HAND',
    'CHOICE SNAKESKIN_FREEBOOTER',
    'CHOICE SQUAWKING_ORATORY',
    'CHOICE STORM_SHARK_WAVECALLER',
    'CHOICE STUBBORN_SHADOW',
    'CHOICE SUMMERSET_SACKING',
    'CHOICE SUMMERSET_SACKING FORTIFY GOLD',
    'CHOICE SUMMERSET_SACKING FORTIFY WRIT_OF_COIN',
    'CHOICE SUMMERSET_SACKING GOLD WRIT_OF_COIN',
    'CHOICE SUMMERSET_SACKING SUMMERSET_SACKING FORTIFY',
    'CHOICE SUMMERSET_SACKING SUMMERSET_SACKING GOLD',
    'CHOICE SUMMERSET_SACKING SUMMERSET_SACKING SUMMERSET_SACKING',
    'CHOICE SUMMERSET_SACKING SUMMERSET_SACKING WRIT_OF_COIN',
    'CHOICE SWIPE',
    'CHOICE Shehai Summoning',
    'CHOICE THE_ARMORY',
    'CHOICE THE_PORTCULLIS',
    'CHOICE TOLL_OF_FLESH',
    'CHOICE TOLL_OF_FLESH WRIT_OF_COIN SEA_ELF_RAID',
    'CHOICE TOLL_OF_SILVER',
    'CHOICE TOLL_OF_SILVER WAR_SONG',
    'CHOICE TOLL_OF_SILVER WAY_OF_THE_SWORD',
    'CHOICE TOLL_OF_SILVER WRIT_OF_COIN',
    'CHOICE WARRIOR_WAVE',
    'CHOICE WAR_SONG',
    'CHOICE WAR_SONG FORTIFY',
    'CHOICE WAR_SONG GOLD',
    'CHOICE WAR_SONG WRIT_OF_COIN',
    'CHOICE WAY_OF_THE_SWORD',
    'CHOICE WAY_OF_THE_SWORD BLACKFEATHER_KNIGHT',
    'CHOICE WAY_OF_THE_SWORD FORTIFY',
    'CHOICE WAY_OF_THE_SWORD GOLD',
    'CHOICE WAY_OF_THE_SWORD WAR_SONG',
    'CHOICE WAY_OF_THE_SWORD WRIT_OF_COIN',
    'CHOICE WRIT_OF_COIN',
    'CHOICE WRIT_OF_COIN BEWILDERMENT',
    'CHOICE WRIT_OF_COIN BLACKFEATHER_KNIGHT',
    'CHOICE WRIT_OF_COIN FORTIFY',
    'CHOICE WRIT_OF_COIN GOLD',
    'CHOICE WRIT_OF_COIN GOLD SEA_ELF_RAID',
    'CHOICE WRIT_OF_COIN GOLD SUMMERSET_SACKING',
    'CHOICE WRIT_OF_COIN GOLD TOLL_OF_FLESH',
    'CHOICE WRIT_OF_COIN GOLD WRIT_OF_COIN',
    'CHOICE WRIT_OF_COIN PECK',
    'CHOICE WRIT_OF_COIN TOLL_OF_FLESH SEA_ELF_RAID',
    'CHOICE WRIT_OF_COIN TOLL_OF_FLESH WRIT_OF_COIN',
    'CHOICE WRIT_OF_COIN TOLL_OF_SILVER',
    'CHOICE WRIT_OF_COIN WAR_SONG',
    'CHOICE WRIT_OF_COIN WAY_OF_THE_SWORD',
    'CHOICE WRIT_OF_COIN WRIT_OF_COIN',
    'CHOICE WRIT_OF_COIN WRIT_OF_COIN SEA_ELF_RAID',
    'CHOICE WRIT_OF_COIN WRIT_OF_COIN SUMMERSET_SACKING',
    'CHOICE Warrior Wave',
    'CHOICE Way of the Sword',
    'END_TURN',
    'PATRON ANSEI',
    'PATRON DUKE_OF_CROWS',
    'PATRON HLAALU',
    'PATRON ORGNUM',
    'PATRON PELIN',
    'PATRON RAJHIN',
    'PATRON RED_EAGLE',
    'PATRON TREASURY',
    'PLAY_CARD ARCHERS_VOLLEY',
    'PLAY_CARD BANGKORAI_SENTRIES',
    'PLAY_CARD BEWILDERMENT',
    'PLAY_CARD BLACKFEATHER_BRIGAND',
    'PLAY_CARD BLACKFEATHER_KNAVE',
    'PLAY_CARD BLACKFEATHER_KNIGHT',
    'PLAY_CARD CONQUEST',
    'PLAY_CARD CURRENCY_EXCHANGE',
    'PLAY_CARD CUSTOMS_SEIZURE',
    'PLAY_CARD FORTIFY',
    'PLAY_CARD GHOSTSCALE_SEA_SERPENT',
    'PLAY_CARD GOLD',
    'PLAY_CARD GOODS_SHIPMENT',
    'PLAY_CARD GRAND_LARCENY',
    'PLAY_CARD HEL_SHIRA_HERALD',
    'PLAY_CARD HIRAS_END',
    'PLAY_CARD HIRELING',
    'PLAY_CARD HLAALU_COUNCILOR',
    'PLAY_CARD HOSTILE_TAKEOVER',
    'PLAY_CARD JEERING_SHADOW',
    'PLAY_CARD KNIGHTS_OF_SAINT_PELIN',
    'PLAY_CARD KNIGHT_COMMANDER',
    'PLAY_CARD LEGIONS_ARRIVAL',
    'PLAY_CARD LUXURY_EXPORTS',
    'PLAY_CARD MAORMER_BOARDING_PARTY',
    'PLAY_CARD MARCH_ON_HATTU',
    'PLAY_CARD MIDNIGHT_RAID',
    'PLAY_CARD MURDER_OF_CROWS',
    'PLAY_CARD NO_SHIRA_POET',
    'PLAY_CARD OATHMAN',
    'PLAY_CARD PECK',
    'PLAY_CARD PILFER',
    'PLAY_CARD PLUNDER',
    'PLAY_CARD POOL_OF_SHADOW',
    'PLAY_CARD POUNCE_AND_PROFIT',
    'PLAY_CARD PROWLING_SHADOW',
    'PLAY_CARD PYANDONEAN_WAR_FLEET',
    'PLAY_CARD RALLY',
    'PLAY_CARD REINFORCEMENTS',
    'PLAY_CARD SCRATCH',
    'PLAY_CARD SEA_ELF_RAID',
    'PLAY_CARD SEA_SERPENT_COLOSSUS',
    'PLAY_CARD SERPENTGUARD_RIDER',
    'PLAY_CARD SERPENTPROW_SCHOONER',
    'PLAY_CARD SHEHAI_SUMMONING',
    'PLAY_CARD SIEGE_WEAPON_VOLLEY',
    'PLAY_CARD SLIGHT_OF_HAND',
    'PLAY_CARD SNAKESKIN_FREEBOOTER',
    'PLAY_CARD SQUAWKING_ORATORY',
    'PLAY_CARD STORM_SHARK_WAVECALLER',
    'PLAY_CARD STUBBORN_SHADOW',
    'PLAY_CARD SUMMERSET_SACKING',
    'PLAY_CARD SWIPE',
    'PLAY_CARD THE_ARMORY',
    'PLAY_CARD THE_PORTCULLIS',
    'PLAY_CARD TOLL_OF_FLESH',
    'PLAY_CARD TOLL_OF_SILVER',
    'PLAY_CARD WARRIOR_WAVE',
    'PLAY_CARD WAR_SONG',
    'PLAY_CARD WAY_OF_THE_SWORD',
    'PLAY_CARD WRIT_OF_COIN']

patron_key = {'Treasury' : 0, 'Ansei' : 1, 'Crows' : 2, 'Rajhin' : 3, 'Psijic' : 4, 'Orgnum' : 5, 'Hlaalu' : 6, 'Pelin' : 7, 'Red Eagle' : 8}
gamestate_patron_key = {'TREASURY' : 0, 'ANSEI' : 1, 'DUKE_OF_CROWS' : 2, 'RAJHIN' : 3, 'PSIJIC' : 4, 'ORGNUM' : 5, 'HLAALU' : 6, 'PELIN' : 7, 'RED_EAGLE' : 8}
card_type_key = {'ACTION' : 1, 'AGENT' : 2, 'CONTRACT_ACTION' : 3, 'CONTRACT_AGENT' : 4, 'STARTER' : 1, 'CURSE' : 1}
effect_slot_key = {
    'Coin' : 0,
    'Power' : 1,
    'Prestige' : 2,
    'OppLosePrestige' : 3,
                            'Remove' : 4,
                            'Acquire' : 5,
                            'Destroy' : 6,
    'Draw' : 7,
    'Discard' : 8,
                            'Return' : 9,
    'Toss' : 10,
                            'KnockOut' : 11,
    'Patron' : 12,
    'Create' : 13,
    'Heal' : 14
    }
patron_conversion_key = {
        'TREASURY' : 'Treasury', 
        'ANSEI' : 'Ansei', 
        'DUKE_OF_CROWS' : 'Crows', 
        'RAJHIN' : 'Rajhin', 
        'PSIJIC' : 'Psijic', 
        'ORGNUM' : 'Orgnum', 
        'HLAALU' : 'Hlaalu', 
        'PELIN' : 'Pelin', 
        'RED_EAGLE' : 'Red Eagle'}

#Takes text effects and cleans them up for use in vectorization
def parse_effects(text):
    # Use regular expression to find all occurrences of the pattern 'EFFECT_NAME number'
    pattern = r"(\w+)\s+(\d+)"
    matches = re.finditer(pattern, text)
    
    # Convert each match into a tuple and collect them into a list
    result = [(match.group(1), int(match.group(2))) for match in matches]
    return result

#Searches cards.json for the card object that corresponds to a certain card's name.
## Note: cards.json's information is sometimes presented differently than whatever JSON is sent via the external language adpater.
## For example: 'GAIN_COIN 3' effect in the language adapter becomes 'Coin 3' in cards.json
def find_card_by_name(card_name, list_of_card_objects):
    card_name = convert_gamestate_cardname_to_cardsjson_cardname(card_name)
    for card_object in list_of_card_objects:
        
        if card_object.get('Name').replace('_',' ').title() == card_name.replace('_',' ').title():
            return card_object

#Used to find the combo state
#The combo state is a dictionary that contains the different decks (suits?) as the keys, and the # of cards of that deck that have been played this turn
#Important for keeping track of combo effects
def get_actions_after_last_end_turn(gamestate):
    if not 'CompletedActions' in gamestate:
        return 'GameHasntStartedSignal'
    
    actions = gamestate['CompletedActions']
    # Return the actions if it's the first turn (no 'END_TURN' in log)
    if 'END_TURN' not in actions:
        return actions
    # Find the index of the most recent 'END_TURN' action
    last_end_turn_index = len(actions) - 1 - actions[::-1].index('END_TURN')
    
    # Create a new list with actions after the most recent 'END_TURN' action
    actions_after_last_end_turn = actions[last_end_turn_index + 1:]
    
    return actions_after_last_end_turn

#Used to find the combo state
def find_deck(card_string):
    # Regular expression to match the deck information
    match = re.search(r'Deck: (\w+)', card_string)
    if match:
        return match.group(1)
    else:
        return None

#A few conversion functions between adapter and cards.json
def convert_gamestate_patron_to_cardsjson_patron(gamestate_patron_name):
    return patron_conversion_key[gamestate_patron_name]

def convert_gamestate_cardname_to_cardsjson_cardname(gamestate_cardname):
    if gamestate_cardname == 'ANSEIS_VICTORY':
        return "Ansei's Victory"
    elif gamestate_cardname == 'HIRAS_END':
        return "Hira's End"
    elif gamestate_cardname == 'LEGIONS_ARRIVAL':
        return "Legion's Arrival"
    elif gamestate_cardname == 'RINGS_GUILE':
        return "Ring's Guile"
    elif gamestate_cardname == 'ARCHERS_VOLLEY':
        return "Archers' Volley"
    elif gamestate_cardname == 'CLANWITCH':
        return "Clan-Witch"
    elif gamestate_cardname == 'KARTH_MANHUNTER':
        return "Karth Man-Hunter"
    elif gamestate_cardname == "KING_ORGNUMS_COMMAND":
        return "King Orgnum's Command"
    elif gamestate_cardname == "SEA_RAIDERS_GLORY":
        return "Sea Raider's Glory"
    elif gamestate_cardname == "SHADOWS_SLUMBER":
        return "Shadow's Slumber"
    else:
        return gamestate_cardname

#Takes gamestate, returns dictionary like {<PATRON> : <# of cards of that patron played this turn>}
def find_combo_state(gamestate):
    combo_state = {'Treasury' : 0, 'Ansei' : 0, 'Crows' : 0, 'Rajhin' : 0, 'Psijic' : 0, 'Orgnum' : 0, 'Hlaalu' : 0, 'Pelin' : 0, 'Red Eagle' : 0}
    
    if get_actions_after_last_end_turn(gamestate) == 'GameHasntStartedSignal':
        return combo_state
    for action in get_actions_after_last_end_turn(gamestate):
        if action[0:9] == 'PLAY_CARD':
            deck = convert_gamestate_patron_to_cardsjson_patron(find_deck(action))
            combo_state[deck] += 1
    return combo_state

#Turns an action into a vector of dimension 24
def vectorize_play_card_action(card_name, combo_state):
    vector = np.zeros(24)
    
    card = find_card_by_name(card_name, cardsjson)
    
    vector[patron_key[card['Deck']]] = 1
    
    effectsvec = np.zeros(15)
    
    combo_1_effects = card['Activation']
    if combo_state >= 0 and combo_1_effects != None:
        for effect in parse_effects(combo_1_effects):
            effectsvec[effect_slot_key[effect[0]]] = effect[1]
        
    combo_2_effects = card['Combo 2']
    if combo_state >= 1 and combo_2_effects != None:
        for effect in parse_effects(combo_2_effects):
            effectsvec[effect_slot_key[effect[0]]] += effect[1]
            
    combo_3_effects = card['Combo 3']
    if combo_state >= 2 and combo_3_effects != None:
        for effect in parse_effects(combo_3_effects):
            effectsvec[effect_slot_key[effect[0]]] += effect[1]
            
    combo_4_effects = card['Combo 4']
    if combo_state >= 3 and combo_4_effects != None:
        for effect in parse_effects(combo_4_effects):
            effectsvec[effect_slot_key[effect[0]]] += effect[1]
            
    vector[9:] = effectsvec
    
    return vector

def map_action_to_vector(action, gamestate):
    
    vec = np.zeros(105)
    ## Return heuristic signal if it's a'CHOICE' action for choosing cards
    if action[0:6] =='CHOICE':
        #'HEURISTIC SIGNAL: This choice involves choosing a specific card. It must be evaluated by the heuristic function!'
        return None
        
    
    ## Currently we treat "OR" effects like "AND" effects, meaning that the bot thinks that a choice card will just give us both choices instead of only one.
    ## This is fixable only if we let the bot create an entirely new action, one for each option
    ## "PLAY_CARD 
    if action[0:9] == 'PLAY_CARD':
        card_name = action[10:]
        card = find_card_by_name(card_name, cardsjson)
        card_patron = card['Deck']
        combo_state = find_combo_state(gamestate)
        card_patron_combo_state = combo_state[card_patron]
        vec[:24] = vectorize_play_card_action(card_name, card_patron_combo_state)
        return vec
    
    ## Issue with combo effects: we don't consider the combo {2,3,4} effects in this code,
    ## So the bot can't buy a card specifically for its combo effects.
    if action[:8] == 'BUY_CARD':
        card_name = action[9:]
        vec[24:48] = vectorize_play_card_action(card_name, 0)
        return vec
    
    ## Code for Agents
    if action[:14] == 'ACTIVATE_AGENT':
        agent_name = action[15:]
        agent = find_card_by_name(agent_name, cardsjson)
        agent_patron = agent['Deck']
        combo_state = find_combo_state(gamestate)
        agent_patron_combo_state = combo_state[agent_patron]
        vec[48:72] = vectorize_play_card_action(agent_name, agent_patron_combo_state)
        return vec
    
    ## Opponent's Agents
    if action[:6] == 'ATTACK':
        opp_agent_name = action[7:]
        vec[72:96] = vectorize_play_card_action(opp_agent_name, 0)
        return vec
    
    ## Activating patrons
    ## So far we are just having each patron be its own part of the vector
    ## This is because some patrons have effects that are hard to quantify
    ## For example, Ansei gives passive income when favored, which can't really be vectorized.
    ## But we could try adding a "passive income" section to the mini-vector eventually. 
    ## It's something to experiement with.
    if action[:6] == 'PATRON':
        patronvec = np.zeros(9)
        patron_index = gamestate_patron_key[action[7:]]
        patronvec[patron_index] = 1
        vec[96:105] = patronvec
        return vec
    
    else:
        return None
        #return "I haven't coded this part yet"


## Set up cards.json for use in vectorization
## TODO for Dr. Dockhorn: download cards.json and change filepath accordingly
## Cards.json is SoT's file that describes every card in the game. We use it to get the effects of cards when we only have access to card names.
## Note that, frustratingly, cards.json has a different formatting for card names than the external language adapter does.
## Therefore, there are a few functions in here that help convert between adapter to JSON and vice versa.

json_file_path = 'cards.json'
with open(json_file_path, 'r') as file:
    cardsjson = json.load(file)


if __name__ == "__main__":
    ## Load gamestates
    gamestate_filename = 'past_gamestates.pkl'
    gamestate_filename = "C:/Users/lashm/OneDrive/Desktop/ActionsGamestates/past_gamestates.pkl"
    assert os.path.exists(gamestate_filename), 'File not found'
    with open(gamestate_filename, 'rb') as file:
        gamestates = []
        while 1:
            try:
                gamestates.append(pickle.load(file))
            except EOFError:
                break

    for game in gamestates:
        for action in example_actions:
            print('-' * 30)
            print(action)
            print(map_action_to_vector(action, game))


