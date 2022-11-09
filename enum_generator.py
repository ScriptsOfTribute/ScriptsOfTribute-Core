import json

cards_json = open('cards.json', 'r', encoding='utf-8-sig').read()

cards = json.loads(cards_json)


def generate_card_enum_file():
    card_enum = 'namespace TalesOfTribute;\n\nenum CardId {\n'

    for card in cards:
        name = '_'.join(
            map(lambda word: word.upper(),
                card['Name'].replace('-', '')
                .replace('\'', '')
                .split(' ')))
        id = card['id']
        card_enum += f'    {name} = {id},\n'

    card_enum += '}\n'

    open('./src/Board/CardId.cs', 'w').write(card_enum)


generate_card_enum_file()
