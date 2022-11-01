using System;
using System.Collections.Generic;

namespace TalesOfTribute
{
    class PatronBase
    {
        private int[] PlayerTier = new int[2];
        private Dictionary<int, int> IdMap = new Dictionary<int, int>();

        public PatronBase(Player player1, Player player2)
        {
            IdMap.Add(player1.Id, 0);
            IdMap.Add(player2.Id, 1);
        }

        public int GetTier(int player_id){
            int id = IdMap[player_id];
            return PlayerTier[id];
        }

        public virtual bool ActivatePatronAbility(Player player1, Player player2=null, int chosen_card_id = -1)
        {
            return true;
        }
    }

    class DukeOfCrows : PatronBase
    {
        const int PECK_CARD = 38; 

        public DukeOfCrows(Player player1, Player player2) : base(player1, player2)
        {
            Manager.Instance.InsertCard(player1, id_card=PECK_CARD, where='Deck');
            Manager.Instance.InsertCard(player2, id_card=PECK_CARD, where='Deck');
        }

        public bool ActivatePatronAbility(Player player1, Player player2 = null, int chosen_card_id = -1)
        {
            int Tier = GetTier(player1.Id);
            int id = IdMap[player1.Id];

            if (player1.CoinsAmount <= 0 || Tier > 0)
            {
                return false;
            }

            player1.PowerAmount += player1.CoinsAmount - 1;
            player1.CoinsAmount = 0;
            PlayerTier[id]++;
            PlayerTier[(id + 1) % 2]--;

            return true;
        }
    }

    class AnseiFrandarHunding : PatronBase
    {
        public AnseiFrandarHunding(Player player1, Player player2) : base(player1, player2)
        {
            Manager.Instance.InsertCard(player1, id_card=-1/*TODO*/, where='Deck');
            Manager.Instance.InsertCard(player2, id_card=-1/*TODO*/, where='Deck');
        }

        public bool ActivatePatronAbility(Player player1, Player player2=null, int chosen_card_id = -1)
        {
            int Tier = GetTier(player1.Id);

            // TODO in Manager we need to check if this patron favors someone 
            //- passive action gain 1 Coin at the start of turn
            if (player1.PowerAmount < 2 || Tier == 1) 
            {
                return false;
            }
        
            player1.PowerAmount -= 2;
            player1.CoinsAmount++;
            PlayerTier[id] = 1;
            PlayerTier[(id + 1) % 2] = 0;
            
            return true;
        }
    }

    class  SaintPelin: PatronBase
    {   
        const int FORTIFY_CARD = 75;

        public SaintPelin(Player player1, Player player2) : base(player1, player2)
        {
            Manager.Instance.InsertCard(player1, id_card=FORTIFY_CARD, where='Deck');
            Manager.Instance.InsertCard(player2, id_card=FORTIFY_CARD, where='Deck');
        }

        public bool ActivatePatronAbility(Player player1, Player player2=null, int chosen_card_id = -1)
        {
            int Tier = GetTier(player1.Id);
            int id = IdMap[player1.Id];
           
            if (player1.PowerAmount < 2)
            {
                return false;
			}
            
            player1.PowerAmount -= 2;
            Manager.Instance.MoveFromCooldownToDeck(player1, chosen_card_id, where='Top'); // card id or instance of card, couse 2 cards can have same id
            
            if (Tier <= 0)
            {
                PlayerTier[id]++;
                PlayerTier[(id + 1) % 2]--;
            }
            
            return true;
        }
    }

    class  Rajhin: PatronBase
    {
        const int SWIPE = 88;
        const int BEWILDERMENT = 8778;

        public Rajhin(Player player1, Player player2) : base(player1, player2)
        {
            Manager.Instance.InsertCard(player1, id_card=SWIPE, where='Deck');
            Manager.Instance.InsertCard(player2, id_card=SWIPE, where='Deck');
        }

        public bool ActivatePatronAbility(Player player1, Player player2=null, int chosen_card_id = -1)
        {
            int Tier = GetTier(player1.Id);
            int id = IdMap[player1.Id];

            if (player1.CoinsAmount < 3)
            {
                return false
            }

            player1.PowerAmount -= 3;
            Manager.Instance.InsertCard(player2, id_card=BEWILDERMENT, where='Cooldown');

            if (Tier <= 0)
            {
                PlayerTier[id]++;
                PlayerTier[(id + 1) % 2]--;
            }

            return true;
        }
    }

	class  PsijicLoremasterCelarus: PatronBase
    {
        const int MAINLAND_INQUIRIES = 62;
        public PsijicLoremasterCelarus(Player player1, Player player2) : base(player1, player2)
        {
            Manager.Instance.InsertCard(player1, id_card=MAINLAND_INQUIRIES, where='Deck');
            Manager.Instance.InsertCard(player2, id_card=MAINLAND_INQUIRIES, where='Deck');
        }

        public bool ActivatePatronAbility(Player player1, Player player2=null, int chosen_card_id = -1)
        {
            int Tier = GetTier(player1.Id);
            int id = IdMap[player1.Id];

            if (player1.CoinsAmount < 4)
            {
                return false;
            }

            player1.PowerAmount -= 4;
            Manager.Instance.KnockOutAgent(player2, chosen_card_id);

            if (Tier <= 0)
            {
                PlayerTier[id]++;
                PlayerTier[(id + 1) % 2]--;
            }

            return true;
        }
    }

	class  GrandmasterDelmeneHlaalu: PatronBase
    {
        const int GOODS_SHIPMENT = 12;

        public GrandmasterDelmeneHlaalu(Player player1, Player player2) : base(player1, player2)
        {
            Manager.Instance.InsertCard(player1, id_card=GOODS_SHIPMENT, where='Deck');
            Manager.Instance.InsertCard(player2, id_card=GOODS_SHIPMENT, where='Deck');
        }

        public bool ActivatePatronAbility(Player player1, Player player2=null, int chosen_card_id = -1)
        {
            int Tier = GetTier(player1.Id);
            int id = IdMap[player1.Id];

            player1.PrestigeAmount += Manager.Instance.GetCardCost(chosen_card_id) - 1;
            Manager.Instance.RemoveCard(player1, chosen_card_id);

            if (Tier <= 0)
            {
                PlayerTier[id]++;
                PlayerTier[(id + 1) % 2]--;
            }

            return true;
        }
    }

    class  SorcererKingOrgnum: PatronBase
    {
        const int SEA_ELF_RIDE_CARD = 95;
        const int MAORMER_BOARDING_PARTY_CARD = 92;

        public SorcererKingOrgnum(Player player1, Player player2) : base(player1, player2)
        {
            Manager.Instance.InsertCard(player1, id_card=SEA_ELF_RIDE_CARD, where='Deck');
            Manager.Instance.InsertCard(player2, id_card=SEA_ELF_RIDE_CARD, where='Deck');
        }

        public bool ActivatePatronAbility(Player player1, Player player2=null, int chosen_card_id = -1)
        {
            int Tier = GetTier(player1.Id);
            int id = IdMap[player1.Id];

            if (player1.CoinsAmount < 3)
            {
                return false;
            }
            
            player1.CoinsAmount -= 3;
            int NumberOfPlayerCards = Manager.Instance.GetAmountOfCards(player1);
            switch (Tier)
            {
                case -1:
                    player1.PowerAmount += 2;
                    break;
                case 0:
                    player1.PowerAmount += NumberOfPlayerCards/6;
                    break;
                case 1:
                    player1.PowerAmount += NumberOfPlayerCards/4;
                    Manager.Instance.InsertCard(player2, id_card=MAORMER_BOARDING_PARTY_CARD, where='Cooldown');
                    break;
            }
            if (Tier <= 0)
            {
                PlayerTier[id]++;
                PlayerTier[(id + 1) % 2]--;
            }
            return true;
        }
    }
    
    class RedEagle: PatronBase
    {
        const int WAR_SONG_CARD = 25;

        public RedEagle(Player player1, Player player2) : base(player1, player2)
        {
            Manager.Instance.InsertCard(player1, id_card=WAR_SONG_CARD, where='Deck');
            Manager.Instance.InsertCard(player2, id_card=WAR_SONG_CARD, where='Deck');
        }
        
        public bool ActivatePatronAbility(Player player1, Player player2=null, int chosen_card_id = -1)
        {
            int Tier = GetTier(player1.Id);
            int id = IdMap[player1.Id];

            if (player1.PowerAmount < 2)
            {
                return false;
            }
            
            player1.PowerAmount -= 2;
            Manager.Instance.DrawCard(player1);

            if (Tier <= 0)
            {
                PlayerTier[id]++;
                PlayerTier[(id + 1) % 2]--;
            }

            Manager.Instance.DrawCard(player1);
            return true;
        }
    }
}