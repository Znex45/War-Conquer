using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WarConquer
{
    [Serializable] public class CatalogFile { public CardData[] cards; }
    public class CardCatalog
    {
        readonly Dictionary<string, CardData> cards;
        public static readonly string[] Leaders={"ZUKGROK","SAHRIA","FAUNAR"};
        public IEnumerable<CardData> All => cards.Values;
        public CardData this[string id] => cards[id];
        public bool HasDeckDefinitions => cards.Values.Any(c=>c.deckCopies!=null&&c.deckCopies.Length>0);
        public int Copies(string leader,CardData c)=>HasDeckDefinitions?(c.deckCopies?.FirstOrDefault(d=>d.leader==leader)?.count??0):(c.leader==leader?c.quantity:0);
        public IEnumerable<CardData> Deck(string leader)=>cards.Values.Where(c=>Copies(leader,c)>0&&!c.Has("Token"));
        public CardCatalog(IEnumerable<CardData> source)
        {
            cards = source.ToDictionary(c => c.id);
            foreach(string leader in Leaders)
            {
                var deck=Deck(leader).ToArray();
                if(deck.Sum(c=>Copies(leader,c))!=50) throw new InvalidOperationException(leader+": el mazo debe tener exactamente 50 cartas.");
                if(deck.Any(c=>Copies(leader,c)<1 || Copies(leader,c)>Math.Min(3,c.maxCopies))) throw new InvalidOperationException(leader+": número de copias inválido.");
            }
        }
        public static CardCatalog Load() => new CardCatalog(JsonUtility.FromJson<CatalogFile>(Resources.Load<TextAsset>("WarConquer/cards").text).cards);
        public static Rules LoadRules() => JsonUtility.FromJson<Rules>(Resources.Load<TextAsset>("WarConquer/rules").text);
    }
    public static class DeckManager
    {
        public static void Build(GameState s, Player p, CardCatalog catalog)
        {
            foreach(var card in catalog.Deck(p.leader))
                for(int i=0;i<catalog.Copies(p.leader,card);i++) p.deck.Add(new CardInstance { instanceId=s.nextId++,cardId=card.id });
            for(int i=p.deck.Count-1;i>0;i--) { int j=s.Random(i+1); var c=p.deck[i]; p.deck[i]=p.deck[j]; p.deck[j]=c; }
            Draw(s,p,s.rules.initialHand);
        }
        public static void Draw(GameState s, Player p, int count)
        {
            int actual=Math.Min(count,p.deck.Count);
            for(int i=0;i<actual;i++) { p.hand.Add(p.deck[0]); p.deck.RemoveAt(0); }
            if(actual<count) s.Log("J"+(p.id+1)+": mazo vacío, no roba más cartas.");
        }
    }
}
