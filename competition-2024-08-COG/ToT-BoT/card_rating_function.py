import os
import pickle


##Set up card tierlist values and also dictionary of patron decks:
patron_dictionary = {
    "Currency Exchange": "Hlaalu",
    "Luxury Exports": "Hlaalu",
    "Oathman": "Hlaalu",
    "Ebony Mine": "Hlaalu",
    "Hlaalu Councilor": "Hlaalu",
    "Hlaalu Kinsman": "Hlaalu",
    "House Embassy": "Hlaalu",
    "House Marketplace": "Hlaalu",
    "Kwama Egg Mine": "Hlaalu",
    "Hireling": "Hlaalu",
    "Hostile Takeover": "Hlaalu",
    "Customs Seizure": "Hlaalu",
    "Goods Shipment": "Hlaalu",
    "Midnight Raid": "Red Eagle",
    "Blood Sacrifice": "Red Eagle",
    "Bloody Offering": "Red Eagle",
    "Bonfire": "Red Eagle",
    "Briarheart Ritual": "Red Eagle",
    "Clan-Witch": "Red Eagle",
    "Elder Witch": "Red Eagle",
    "Hagraven": "Red Eagle",
    "Hagraven Matron": "Red Eagle",
    "Imperial Plunder": "Red Eagle",
    "Imperial Spoils": "Red Eagle",
    "Karth Man-Hunter": "Red Eagle",
    "War Song": "Red Eagle",
    "Blackfeather Knave": "Crows",
    "Plunder": "Crows",
    "Toll of Flesh": "Crows",
    "Toll of Silver": "Crows",
    "Murder of Crows": "Crows",
    "Pilfer": "Crows",
    "Squawking Oratory": "Crows",
    "Law of Sovereign Roost": "Crows",
    "Pool of Shadow": "Crows",
    "Scratch": "Crows",
    "Blackfeather Brigand": "Crows",
    "Blackfeather Knight": "Crows",
    "Peck": "Crows",
    "Conquest": "Ansei",
    "Grand Oratory": "Ansei",
    "Hira's End": "Ansei",
    "Hel Shira Herald": "Ansei",
    "March on Hattu": "Ansei",
    "Shehai Summoning": "Ansei",
    "Warrior Wave": "Ansei",
    "Ansei Assault": "Ansei",
    "Ansei's Victory": "Ansei",
    "Battle Meditation": "Ansei",
    "No Shira Poet": "Ansei",
    "Way of the Sword": "Ansei",
    "Rally": "Pelin",
    "Siege Weapon Volley": "Pelin",
    "The Armory": "Pelin",
    "Banneret": "Pelin",
    "Knight Commander": "Pelin",
    "Reinforcements": "Pelin",
    "Archers' Volley": "Pelin",
    "Legion's Arrival": "Pelin",
    "Shield Bearer": "Pelin",
    "Bangkorai Sentries": "Pelin",
    "Knights of Saint Pelin": "Pelin",
    "The Portcullis": "Pelin",
    "Fortify": "Pelin",
    "Bag of Tricks": "Rajhin",
    "Bewilderment": "Rajhin",
    "Grand Larceny": "Rajhin",
    "Jarring Lullaby": "Rajhin",
    "Jeering Shadow": "Rajhin",
    "Moonlit Illusion": "Rajhin",
    "Pounce and Profit": "Rajhin",
    "Prowling Shadow": "Rajhin",
    "Ring's Guile": "Rajhin",
    "Shadow's Slumber": "Rajhin",
    "Slight of Hand": "Rajhin",
    "Stubborn Shadow": "Rajhin",
    "Swipe": "Rajhin",
    "Twilight Revelry": "Rajhin",
    "Ghostscale Sea Serpent": "Orgnum",
    "King Orgnum's Command": "Orgnum",
    "Maormer Boarding Party": "Orgnum",
    "Maormer Cutter": "Orgnum",
    "Pyandonean War Fleet": "Orgnum",
    "Sea Elf Raid": "Orgnum",
    "Sea Raider's Glory": "Orgnum",
    "Sea Serpent Colossus": "Orgnum",
    "Serpentguard Rider": "Orgnum",
    "Serpentprow Schooner": "Orgnum",
    "Snakeskin Freebooter": "Orgnum",
    "Storm Shark Wavecaller": "Orgnum",
    "Summerset Sacking": "Orgnum",
    "Ambush": "Treasury",
    "Barterer": "Treasury",
    "Black Sacrament": "Treasury",
    "Blackmail": "Treasury",
    "Gold": "Treasury",
    "Harvest Season": "Treasury",
    "Imprisonment": "Treasury",
    "Ragpicker": "Treasury",
    "Tithe": "Treasury",
    "Writ of Coin": "Treasury"
}

EARLY_GAME_TIERLIST = {
    
    'Ebony Mine': '50',
    'Kwama Egg Mine': '50',
    'Blood Sacrifice': '-1',
    'Bloody Offering': '-1',
    'Bonfire': '0',
    'Briarheart Ritual': '-1',
    'Clan-Witch': '-1',
    'Elder Witch': '-1',
    'Imperial Plunder': '15',
    'Imperial Spoils': '15',
    'Karth Man-Hunter': '-1',
    'Law of Sovereign Roost': '3',
    'Grand Oratory': '15',
    'Battle Meditation': '3',
    'Shield Bearer': '1',
    'Bag of Tricks': '3',
    'Moonlit Illusion': '0',
    "Ring's Guile": '1',
    
    
    
    'Ambush': '0',
    'Barterer': '15',
    'Black Sacrament': '0',
    'Blackmail': '1',
    'Harvest Season': '1',
    'Imprisonment': '15',
    'Ragpicker': '-1',
    'Tithe': '15',
    
    
    "Ghostscale Sea Serpent":3,
    "King Orgnum's Command":1,
    "Maormer Boarding Party":0,
    "Maormer Cutter":0,
    "Pyandonean War Fleet":1,
    "Sea Elf Raid":1,
    "Sea Raider's Glory":1,
    "Sea Serpent Colossus":1,
    "Serpentguard Rider":15,
    "Serpentprow Schooner":1,
    "Snakeskin Freebooter":15,
    "Storm Shark Wavecaller":3,
    "Summerset Sacking":1,
    
    "Currency Exchange": 50,
    "Luxury Exports": 50,
    "Oathman": 30,
    "Hlaalu Councilor": 30,
    "Hlaalu Kinsman": 30,
    "House Embassy": 30,
    "House Marketplace": 15,
    "Hireling": 3,
    "Hostile Takeover": 15,
    "Customs Seizure": 1,
    "Goods Shipment": 1,
    "Midnight Raid": 50,
    "Hagraven": 1,
    "Hagraven Matron": 1,
    "War Song": 1,
    "Blackfeather Knave": 50,
    "Plunder": 50,
    "Toll of Flesh": 50,
    "Toll of Silver": 50,
    "Murder of Crows": 50,
    "Pilfer": 30,
    "Squawking Oratory": 30,
    "Pool of Shadow": 15,
    "Scratch": 30,
    "Blackfeather Brigand": 3,
    "Blackfeather Knight": 30,
    "Peck": 3,
    "Conquest": 30,
    "Hira's End": 50,
    "Hel Shira Herald": 15,
    "March on Hattu": 30,
    "Shehai Summoning": 15,
    "Warrior Wave": 50,
    "Ansei Assault": 15,
    "Ansei's Victory": 15,
    "No Shira Poet": 3,
    "Way of the Sword": 1,
    "Rally": 30,
    "Siege Weapon Volley": 30,
    "The Armory": 30,
    "Banneret": 30,
    "Knight Commander": 50,
    "Reinforcements": 50,
    "Archers' Volley": 15,
    "Legion's Arrival": 30,
    "Bangkorai Sentries": 3,
    "Knights of Saint Pelin": 3,
    "The Portcullis": 3,
    "Fortify": 1,
    "Bewilderment": -3,
    "Grand Larceny": 30,
    "Jarring Lullaby": 15,
    "Jeering Shadow": 3,
    "Pounce and Profit": 50,
    "Prowling Shadow": 15,
    "Shadow's Slumber": 30,
    "Slight of Hand": 30,
    "Stubborn Shadow": 1,
    "Twilight Revelry": 15,
    "Swipe": 1,
    "Gold": 0,
    "Writ of Coin": 10
}
MID_GAME_TIERLIST = {
    
    'Ebony Mine': '15',
    'Kwama Egg Mine': '15',
    'Blood Sacrifice': '50',
    'Bloody Offering': '50',
    'Bonfire': '15',
    'Briarheart Ritual': '15',
    'Clan-Witch': '15',
    'Elder Witch': '15',
    'Imperial Plunder': '15',
    'Imperial Spoils': '15',
    'Karth Man-Hunter': '15',
    'Law of Sovereign Roost': '3',
    'Grand Oratory': '3',
    'Battle Meditation': '3',
    'Shield Bearer': '15',
    'Bag of Tricks': '3',
    'Moonlit Illusion': '3',
    "Ring's Guile": '1',
    
    
    
    'Ambush': '30',
    'Barterer': '15',
    'Black Sacrament': '15',
    'Blackmail': '3',
    'Harvest Season': '1',
    'Imprisonment': '30',
    'Ragpicker': '3',
    'Tithe': '50',

    
    
    
    "Ghostscale Sea Serpent":1,
    "King Orgnum's Command":15,
    "Maormer Boarding Party":1,
    "Maormer Cutter":3,
    "Pyandonean War Fleet":3,
    "Sea Elf Raid":0,
    "Sea Raider's Glory":1,
    "Sea Serpent Colossus":1,
    "Serpentguard Rider":15,
    "Serpentprow Schooner":3,
    "Snakeskin Freebooter":3,
    "Storm Shark Wavecaller":15,
    "Summerset Sacking":1,

    
    
    
    
    "Currency Exchange": 50,
    "Luxury Exports": 50,
    "Oathman": 30,
    "Hlaalu Councilor": 30,
    "Hlaalu Kinsman": 30,
    "House Embassy": 30,
    "House Marketplace": 30,
    "Hireling": 3,
    "Hostile Takeover": 3,
    "Customs Seizure": 1,
    "Goods Shipment": 1,
    "Midnight Raid": 50,
    "Hagraven": 15,
    "Hagraven Matron": 30,
    "War Song": 1,
    "Blackfeather Knave": 50,
    "Plunder": 50,
    "Toll of Flesh": 50,
    "Toll of Silver": 50,
    "Murder of Crows": 50,
    "Pilfer": 50,
    "Squawking Oratory": 50,
    "Pool of Shadow": 30,
    "Scratch": 30,
    "Blackfeather Brigand": 3,
    "Blackfeather Knight": 15,
    "Peck": 3,
    "Conquest": 30,
    "Hira's End": 50,
    "Hel Shira Herald": 30,
    "March on Hattu": 30,
    "Shehai Summoning": 15,
    "Warrior Wave": 30,
    "Ansei Assault": 30,
    "Ansei's Victory": 30,
    "No Shira Poet": 3,
    "Way of the Sword": 1,
    "Rally": 50,
    "Siege Weapon Volley": 50,
    "The Armory": 50,
    "Banneret": 50,
    "Knight Commander": 50,
    "Reinforcements": 30,
    "Archers' Volley": 30,
    "Legion's Arrival": 30,
    "Bangkorai Sentries": 15,
    "Knights of Saint Pelin": 30,
    "The Portcullis": 3,
    "Fortify": 1,
    "Bewilderment": -3,
    "Grand Larceny": 30,
    "Jarring Lullaby": 30,
    "Jeering Shadow": 3,
    "Pounce and Profit": 50,
    "Prowling Shadow": 3,
    "Shadow's Slumber": 30,
    "Slight of Hand": 15,
    "Stubborn Shadow": 3,
    "Twilight Revelry": 30,
    "Swipe": 1,
    "Gold": 0,
    "Writ of Coin": 15
}
LATE_GAME_TIERLIST = {
    
    'Ebony Mine': '0',
    'Kwama Egg Mine': '0',
    'Blood Sacrifice': '50',
    'Bloody Offering': '50',
    'Bonfire': '15',
    'Briarheart Ritual': '50',
    'Clan-Witch': '15',
    'Elder Witch': '15',
    'Imperial Plunder': '1',
    'Imperial Spoils': '1',
    'Karth Man-Hunter': '15',
    'Law of Sovereign Roost': '3',
    'Grand Oratory': '15',
    'Battle Meditation': '15',
    'Shield Bearer': '15',
    'Bag of Tricks': '3',
    'Moonlit Illusion': '3',
    "Ring's Guile": '3',
    
    
    
    'Ambush': '30',
    'Barterer': '1',
    'Black Sacrament': '15',
    'Blackmail': '3',
    'Harvest Season': '1',
    'Imprisonment': '30',
    'Ragpicker': '15',
    'Tithe': '50',

    
    
    
    "Ghostscale Sea Serpent":1,
    "King Orgnum's Command":15,
    "Maormer Boarding Party":1,
    "Maormer Cutter":3,
    "Pyandonean War Fleet":3,
    "Sea Elf Raid":0,
    "Sea Raider's Glory":3,
    "Sea Serpent Colossus":3,
    "Serpentguard Rider":3,
    "Serpentprow Schooner":3,
    "Snakeskin Freebooter":-1,
    "Storm Shark Wavecaller":1,
    "Summerset Sacking":1,
    
    
    "Currency Exchange": 30,
    "Luxury Exports": 30,
    "Oathman": 15,
    "Hlaalu Councilor": 15,
    "Hlaalu Kinsman": 15,
    "House Embassy": 15,
    "House Marketplace": 15,
    "Hireling": 1,
    "Hostile Takeover": 1,
    "Customs Seizure": 1,
    "Goods Shipment": 1,
    "Midnight Raid": 50,
    "Hagraven": 1,
    "Hagraven Matron": 3,
    "War Song": 1,
    "Blackfeather Knave": 30,
    "Plunder": 50,
    "Toll of Flesh": 30,
    "Toll of Silver": 30,
    "Murder of Crows": 30,
    "Pilfer": 30,
    "Squawking Oratory": 30,
    "Pool of Shadow": 15,
    "Scratch": 15,
    "Blackfeather Brigand": 1,
    "Blackfeather Knight": 3,
    "Peck": 1,
    "Conquest": 15,
    "Hira's End": 50,
    "Hel Shira Herald": 15,
    "March on Hattu": 15,
    "Shehai Summoning": 15,
    "Warrior Wave": 15,
    "Ansei Assault": 15,
    "Ansei's Victory": 15,
    "No Shira Poet": 1,
    "Way of the Sword": 1,
    "Rally": 30,
    "Siege Weapon Volley": 15,
    "The Armory": 30,
    "Banneret": 30,
    "Knight Commander": 30,
    "Reinforcements": 15,
    "Archers' Volley": 15,
    "Legion's Arrival": 15,
    "Bangkorai Sentries": 3,
    "Knights of Saint Pelin": 3,
    "The Portcullis": 1,
    "Fortify": 1,
    "Bewilderment": -3,
    "Grand Larceny": 15,
    "Jarring Lullaby": 15,
    "Jeering Shadow": 1,
    "Pounce and Profit": 15,
    "Prowling Shadow": 1,
    "Shadow's Slumber": 15,
    "Slight of Hand": 1,
    "Stubborn Shadow": 1,
    "Twilight Revelry": 15,
    "Swipe": 1,
    "Gold": 0,
    "Writ of Coin": 0
}


'''
early game

    'Ebony Mine': '50',
    'Kwama Egg Mine': '50',
    'Blood Sacrifice': '-1',
    'Bloody Offering': '-1',
    'Bonfire': '0',
    'Briarheart Ritual': '-1',
    'Clan-Witch': '-1',
    'Elder Witch': '-1',
    'Imperial Plunder': '15',
    'Imperial Spoils': '15',
    'Karth Man-Hunter': '-1',
    'Law of Sovereign Roost': '3',
    'Grand Oratory': '15',
    'Battle Meditation': '3',
    'Shield Bearer': '1',
    'Bag of Tricks': '3',
    'Moonlit Illusion': '0',
    "Ring's Guile": '1',
    
    
    
    'Ambush': '0',
    'Barterer': '15',
    'Black Sacrament': '0',
    'Blackmail': '1',
    'Harvest Season': '1',
    'Imprisonment': '15',
    'Ragpicker': '-1',
    'Tithe': '15'

midgame
    'Ebony Mine': '15',
    'Kwama Egg Mine': '15',
    'Blood Sacrifice': '50',
    'Bloody Offering': '50',
    'Bonfire': '15',
    'Briarheart Ritual': '15',
    'Clan-Witch': '15',
    'Elder Witch': '15',
    'Imperial Plunder': '15',
    'Imperial Spoils': '15',
    'Karth Man-Hunter': '15',
    'Law of Sovereign Roost': '3',
    'Grand Oratory': '3',
    'Battle Meditation': '3',
    'Shield Bearer': '15',
    'Bag of Tricks': '3',
    'Moonlit Illusion': '3',
    "Ring's Guile": '1',
    
    
    
    'Ambush': '30',
    'Barterer': '15',
    'Black Sacrament': '15',
    'Blackmail': '3',
    'Harvest Season': '1',
    'Imprisonment': '30',
    'Ragpicker': '3',
    'Tithe': '50'




endgame
    'Ebony Mine': '0',
    'Kwama Egg Mine': '0',
    'Blood Sacrifice': '50',
    'Bloody Offering': '50',
    'Bonfire': '15',
    'Briarheart Ritual': '50',
    'Clan-Witch': '15',
    'Elder Witch': '15',
    'Imperial Plunder': '1',
    'Imperial Spoils': '1',
    'Karth Man-Hunter': '15',
    'Law of Sovereign Roost': '3',
    'Grand Oratory': '15',
    'Battle Meditation': '15',
    'Shield Bearer': '15',
    'Bag of Tricks': '3',
    'Moonlit Illusion': '3',
    "Ring's Guile": '3',
    
    
    
    'Ambush': '30',
    'Barterer': '1',
    'Black Sacrament': '15',
    'Blackmail': '3',
    'Harvest Season': '1',
    'Imprisonment': '30',
    'Ragpicker': '15',
    'Tithe': '50'





'''
def count_cards_of_a_suit(suit, gamestate, player):
    count = 0
    suit = suit.upper()
    
    if player == 'CurrentPlayer':
        for card in gamestate['CurrentPlayer']['Cooldown']:
            if card['deck'] == suit:
                count += 1
        for card in gamestate['CurrentPlayer']['Played']:
            if card['deck'] == suit:
                count += 1
        for agent in gamestate['CurrentPlayer']['Agents']:
            if agent['Card']['deck'] == suit:
                count += 1
        for card in gamestate['CurrentPlayer']['Hand']:
            if card['deck'] == suit:
                count += 1
        for card in gamestate['CurrentPlayer']['DrawPile']:
            if card['deck'] == suit:
                count += 1
                
    if player == 'EnemyPlayer':
        for card in gamestate['EnemyPlayer']['Cooldown']:
            if card['deck'] == suit:
                count += 1
        for card in gamestate['EnemyPlayer']['Played']:
            if card['deck'] == suit:
                count += 1
        for agent in gamestate['EnemyPlayer']['Agents']:
            if agent['Card']['deck'] == suit:
                count += 1
        for card in gamestate['EnemyPlayer']['HandAndDraw']:
            if card['deck'] == suit:
                count += 1
    
    return count

def rate_card(card, gamestate, player):
    #Ascertain the game phase
    if gamestate['CurrentPlayer']['Prestige'] > 26 or gamestate['EnemyPlayer']['Prestige'] > 29:
        game_phase = 'LATE_GAME'
    if gamestate['CurrentPlayer']['Prestige'] < 11 and gamestate['EnemyPlayer']['Prestige'] < 14:
        game_phase = 'EARLY_GAME'
    else:
        game_phase = 'MID_GAME'
        
    #Find value of combos
    if game_phase == 'EARLY_GAME':
        combo_power = 1.453
    elif game_phase == 'MID_GAME':
        combo_power = 2.018
    elif game_phase == 'LATE_GAME':
        combo_power = 1.765
        
    #Find "card limit"
    if game_phase == 'EARLY_GAME':
        card_limit = 3.584
    elif game_phase == 'MID_GAME':
        card_limit = 9.803
    elif game_phase == 'LATE_GAME':
        card_limit = 6.175
        
    #Assign card its base value
    if game_phase == 'EARLY_GAME':
        base_value = EARLY_GAME_TIERLIST[card]
    elif game_phase == 'MID_GAME':
        base_value = MID_GAME_TIERLIST[card]
    elif game_phase == 'LATE_GAME':
        base_value = LATE_GAME_TIERLIST[card]
        
        
        
    #Find the # of cards of a suit in the player's deck
    number_of_patron_cards = count_cards_of_a_suit(patron_dictionary[card], gamestate, player)
    
    patron_bonus = number_of_patron_cards ** combo_power
    
    if patron_dictionary[card] == 'Treasury':
        return float(base_value)
    
    else:  
        card_value = float(base_value) * float(patron_bonus)
        return float(card_value)

def convert_gamestate_cardname_to_cardsjson_cardname(gamestate_cardname):
    if gamestate_cardname == 'ANSEIS_VICTORY':
        return "Ansei's Victory"
    elif gamestate_cardname == 'HIRA\'S_END' or gamestate_cardname == 'HIRAS_END' or gamestate_cardname == 'Hira\'s End':
        return "Hira's End"
    elif gamestate_cardname == 'LEGIONS_ARRIVAL' or gamestate_cardname == 'LEGION\'S_ARRIVAL' or gamestate_cardname == 'Legion\'s Arrival':
        return "Legion's Arrival"
    elif gamestate_cardname == 'RINGS_GUILE' or gamestate_cardname == 'RING\'S_GUILE' or gamestate_cardname == 'Ring\'s Guile':
        return "Ring's Guile"
    elif gamestate_cardname == 'ARCHERS_VOLLEY' or gamestate_cardname == 'ARCHER\'S_VOLLEY' or gamestate_cardname == 'Archers\' Volley':
        return "Archers' Volley"
    elif gamestate_cardname == 'CLANWITCH':
        return "Clan-Witch"
    elif gamestate_cardname == 'KARTH_MANHUNTER':
        return "Karth Man-Hunter"
    elif gamestate_cardname == "KING_ORGNUM'S_COMMAND" or gamestate_cardname == "KING_ORGNUM\'S_COMMAND" or gamestate_cardname == "King Orgnum's Command":
        return "King Orgnum's Command"
    elif gamestate_cardname == "SEA_RAIDERS_GLORY" or gamestate_cardname == "SEA_RAIDER\'S_GLORY" or gamestate_cardname == "Sea Raider's Glory":
        return "Sea Raider's Glory"
    elif gamestate_cardname == "SHADOWS_SLUMBER" or gamestate_cardname == "Shadow\'s Slumber" or gamestate_cardname == "SHADOW\'S_SLUMBER":
        return "Shadow's Slumber"
    elif gamestate_cardname == "TOLL_OF_FLESH":
        return "Toll of Flesh"
    elif gamestate_cardname == "TOLL_OF_SILVER":
        return "Toll of Silver"
    elif gamestate_cardname == "MURDER_OF_CROWS":
        return "Murder of Crows"
    elif gamestate_cardname == "LAW_OF_SOVEREIGN_ROOST":
        return "Law of Sovereign Roost"
    elif gamestate_cardname == "POOL_OF_SHADOW":
        return "Pool of Shadow"
    elif gamestate_cardname == "HIRAS_END" or gamestate_cardname == "HIRA\'S_END" or gamestate_cardname == "Hira's End":
        return "Hira's End"
    elif gamestate_cardname == "MARCH_ON_HATTU" or gamestate_cardname == "March on Hattu":
        return "March on Hattu"
    elif gamestate_cardname == "ANSEI'S_VICTORY" or gamestate_cardname == "ANSEI\'S_VICTORY" or gamestate_cardname == "Ansei\'s Victory":
        return "Ansei's Victory"
    elif gamestate_cardname == "WAY_OF_THE_SWORD" or gamestate_cardname == "Way of the Sword":
        return "Way of the Sword"
    elif gamestate_cardname == "KNIGHTS_OF_SAINT_PELIN":
        return "Knights of Saint Pelin"
    elif gamestate_cardname == "BAG_OF_TRICKS":
        return "Bag of Tricks"
    elif gamestate_cardname == "POUNCE_AND_PROFIT":
        return "Pounce and Profit"
    elif gamestate_cardname == "RINGS_GUILE" or gamestate_cardname == "RING\'S_GUILE" or gamestate_cardname == "Ring's Guile":
        return "Ring's Guile"
    elif gamestate_cardname == "SHADOWS_SLUMBER" or gamestate_cardname == "SHADOW\'S_SLUMBER" or gamestate_cardname == "Shadow's Slumber":
        return "Shadow's Slumber"
    elif gamestate_cardname == "SLIGHT_OF_HAND":
        return "Slight of Hand"
    elif gamestate_cardname == "KING_ORGNUMS_COMMAND" or gamestate_cardname == "KING_ORGNUM\'S_COMMAND" or gamestate_cardname == "King Orgnum's Command":
        return "King Orgnum's Command"
    elif gamestate_cardname == "SEA_RAIDERS_GLORY" or gamestate_cardname == "SEA_RAIDER\'S_GLORY" or gamestate_cardname == "Sea Raider's Glory":
        return "Sea Raider's Glory"
    elif gamestate_cardname == "WRIT_OF_COIN":
        return "Writ of Coin"
            
    else:
        name = gamestate_cardname
        
    name = name.replace('_', ' ').title()
    return name



if __name__ == "__main__":

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
            
    ##Test on a whole bunch of gamestates
    for game in gamestates:
        for card in game['CurrentPlayer']['Cooldown']:
            assert type(rate_card(card_name, game, 'CurrentPlayer')) == float, f'AAAAH {type(rate_card(card_name, game, "CurrentPlayer"))}'
            
        for card in game['CurrentPlayer']['Played']:
            card_name = card['name']
            assert type(rate_card(card_name, game, 'CurrentPlayer')) == float, f'AAAAH {type(rate_card(card_name, game, "CurrentPlayer"))}'