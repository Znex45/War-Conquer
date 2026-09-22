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
        public CardCatalog(IEnumerable<CardData> source)
        {
            cards = source.ToDictionary(c => c.id);
            foreach(string leader in Leaders)
            {
                var deck=cards.Values.Where(c=>c.leader==leader && c.quantity>0 && !c.Has("Token")).ToArray();
                if(deck.Sum(c=>c.quantity)!=50) throw new InvalidOperationException(leader+": el mazo debe tener exactamente 50 cartas.");
                if(deck.Any(c=>c.quantity<1 || c.quantity>Math.Min(3,c.maxCopies))) throw new InvalidOperationException(leader+": número de copias inválido.");
            }
        }
        public static CardCatalog Load() => new CardCatalog(JsonUtility.FromJson<CatalogFile>(Resources.Load<TextAsset>("WarConquer/cards").text).cards);
        public static Rules LoadRules() => JsonUtility.FromJson<Rules>(Resources.Load<TextAsset>("WarConquer/rules").text);
    }
    public static class DeckManager
    {
        public static void Build(GameState s, Player p, CardCatalog catalog)
        {
            foreach(var card in catalog.All.Where(c=>c.leader==p.leader && c.quantity>0))
                for(int i=0;i<card.quantity;i++) p.deck.Add(new CardInstance { instanceId=s.nextId++,cardId=card.id });
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
