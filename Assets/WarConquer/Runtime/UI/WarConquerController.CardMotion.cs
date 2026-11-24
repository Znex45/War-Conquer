using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace WarConquer
{
    public partial class WarConquerController
    {
        Player animatedHandOwner;
        readonly Dictionary<int,string> previousHand=new Dictionary<int,string>();
        readonly HashSet<int> previousDeck=new HashSet<int>();
        readonly Dictionary<int,Vector3> previousCardPositions=new Dictionary<int,Vector3>();
        void AnimateHandChanges(Player player)
        {
            if(animatedHandOwner==player)
            {
                int order=0;
                foreach(var card in player.hand.Where(c=>!previousHand.ContainsKey(c.instanceId)&&previousDeck.Contains(c.instanceId)))
                    StartCoroutine(FlyCard(player,card.cardId,piles.TransformPoint(new Vector3(72,-120,0)),hand.TransformPoint(new Vector3(110+matchExtraWidth*.5f,-138,0)),order++*.12f));
                foreach(var card in player.discardPile.Where(c=>previousHand.ContainsKey(c.instanceId)))
                    StartCoroutine(FlyCard(player,card.cardId,previousCardPositions.TryGetValue(card.instanceId,out var pos)?pos:hand.TransformPoint(new Vector3(110,-138,0)),piles.TransformPoint(new Vector3(213,-120,0)),order++*.12f));
            }
            animatedHandOwner=player;previousHand.Clear();previousDeck.Clear();previousCardPositions.Clear();
            foreach(var c in player.hand)previousHand[c.instanceId]=c.cardId;
            foreach(var c in player.deck)previousDeck.Add(c.instanceId);
            int index=0;
            foreach(var c in player.hand.Where(c=>HandMatches(Game.Catalog[c.cardId],c)).Skip(page*6).Take(6))
                previousCardPositions[c.instanceId]=hand.TransformPoint(new Vector3(111+index++*(208+matchExtraWidth/6f)+matchExtraWidth/12f,-138,0));
        }
        IEnumerator FlyCard(Player owner,string cardId,Vector3 from,Vector3 to,float delay)
        {
            if(delay>0)yield return new WaitForSecondsRealtime(delay);
            if(animatedHandOwner!=owner)yield break;
            var visual=CardPresentation.Draw(root,Game,Game.Catalog[cardId],owner,0,0,198,201,false,false);
            visual.name="Carta en tránsito";
            var group=visual.gameObject.AddComponent<CanvasGroup>();group.blocksRaycasts=false;group.interactable=false;
            visual.pivot=new Vector2(.5f,.5f);visual.SetAsLastSibling();
            float elapsed=0;
            while(elapsed<.55f&&visual!=null&&animatedHandOwner==owner)
            {
                float t=Mathf.Clamp01(elapsed/.55f),u=t*t*(3-2*t);
                visual.position=Vector3.Lerp(from,to,u)+Vector3.up*(Mathf.Sin(t*Mathf.PI)*65f*cachedScaler.scaleFactor);
                visual.localRotation=Quaternion.Euler(0,0,Mathf.Sin(t*Mathf.PI)*-8f);
                elapsed+=Time.unscaledDeltaTime;yield return null;
            }
            if(visual!=null)Destroy(visual.gameObject);
        }
    }
}
