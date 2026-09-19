using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace WarConquer
{
    public class BoardView
    {
        readonly RectTransform root;
        readonly WarConquerController controller;
        public float Zoom=1;
        public Vector2 Pan;
        readonly Dictionary<int,Vector2> centers=new Dictionary<int,Vector2>();
        public BoardView(RectTransform parent,WarConquerController owner){root=parent;controller=owner;}
        public void Render(GameManager g,HashSet<int> valid,HashSet<int> selected,int focus)
        {
            GrayboxUI.Clear(root);centers.Clear();var s=g.State;
            float scale=26*Zoom;Vector2 offset=new Vector2(490,327)+Pan;
            foreach(var tile in s.tiles)centers[tile.id]=new Vector2(offset.x+tile.x*scale,offset.y-tile.y*scale);
            // Water gaps and double bridges distinguish the five islands without assigning any initial biome.
            foreach(var edge in s.connections.Where(c=>c.bridge))DrawLine(centers[edge.a],centers[edge.b],8*Zoom,new Color32(156,141,105,255));
            foreach(var route in s.fastRoutes)DrawLine(centers[route.a],centers[route.b],4*Zoom,GrayboxUI.Purple);
            foreach(var t in s.tiles)
            {
                int id=t.id;var pos=centers[id];float size=41.6f*Zoom;
                var rect=GrayboxUI.Rect(root,"Hex "+(id+1),pos.x-size/2,pos.y-size/2,size,size);
                var hex=rect.gameObject.AddComponent<HexGraphic>();hex.color=GrayboxUI.BiomeColor(t.biome);
                hex.border=selected.Contains(id)?Color.white:valid.Contains(id)?GrayboxUI.Green:focus==id?new Color32(248,165,79,255):t.owner>=0?Color.Lerp(GrayboxUI.PlayerColor(t.owner),GrayboxUI.Background,.55f):new Color32(102,113,126,255);
                hex.Click=()=>controller.TileClick(id);
                var label=GrayboxUI.Text(rect,(id+1).ToString(),size*.28f,1,size*.5f,12*Zoom,Mathf.RoundToInt(9*Zoom),new Color32(183,192,198,255));label.alignment=TextAnchor.UpperCenter;
                if(t.blocked)GrayboxUI.Text(rect,"▲",size*.22f,size*.25f,size*.7f,size*.5f,Mathf.RoundToInt(23*Zoom),GrayboxUI.Muted);
                if(t.baseOwner>=0)
                {
                    var mark=GrayboxUI.Text(rect,"J"+(t.baseOwner+1),0,size*.26f,size,size*.5f,Mathf.RoundToInt(16*Zoom),GrayboxUI.PlayerColor(t.baseOwner),FontStyle.Bold);mark.alignment=TextAnchor.MiddleCenter;
                }
                if(t.unit!=null||t.structure!=null)
                {
                    var piece=t.unit??t.structure;var data=g.Data(piece);float ps=size*.63f;
                    var pr=GrayboxUI.Rect(rect,"Piece "+piece.id,(size-ps)/2,size*.21f,ps,ps);var pg=pr.gameObject.AddComponent<PieceGraphic>();pg.color=GrayboxUI.PlayerColor(piece.owner);pg.square=data.IsStructure;pg.raycastTarget=false;
                    string shortName=string.Concat(data.name.Split(' ').Where(w=>w.Length>2).Take(2).Select(w=>w[0]));
                    var name=GrayboxUI.Text(pr,shortName,0,1,ps,ps*.52f,Mathf.RoundToInt(10*Zoom),GrayboxUI.Background,FontStyle.Bold);name.alignment=TextAnchor.MiddleCenter;
                    var hp=GrayboxUI.Text(pr,piece.health.ToString(),0,ps*.45f,ps,ps*.48f,Mathf.RoundToInt(10*Zoom),GrayboxUI.Background,FontStyle.Bold);hp.alignment=TextAnchor.MiddleCenter;
                    string status=(piece.poison>0?"V"+piece.poison:"")+(g.IsSleeping(piece)?" Zz":"");
                    if(status.Length>0){var st=GrayboxUI.Text(rect,status,-3,size*.78f,size+6,13*Zoom,Mathf.RoundToInt(10*Zoom),GrayboxUI.Green,FontStyle.Bold);st.alignment=TextAnchor.MiddleCenter;}
                }
                else if(t.resource>0&&t.baseOwner<0){var res=GrayboxUI.Text(rect,"+"+t.resource,0,size*.38f,size,size*.3f,Mathf.RoundToInt(11*Zoom),GrayboxUI.Muted);res.alignment=TextAnchor.MiddleCenter;}
                if(t.specialEffect!=null){var unstable=GrayboxUI.Text(rect,"!",size*.70f,size*.33f,10*Zoom,15*Zoom,Mathf.RoundToInt(13*Zoom),GrayboxUI.Yellow,FontStyle.Bold);}
            }
            string[] directions={"NORTE","ESTE","SUR","OESTE"};
            for(int p=0;p<4;p++)
            {
                var center=s.tiles.First(t=>t.baseOwner==p);var c=centers[center.id];
                float x=(p==0||p==2)?c.x-254*Zoom:c.x-92;
                float y=c.y+(p==0?-38:p==2?38:-119)*Zoom;
                var tag=GrayboxUI.Box(root,"Territorio "+(p+1),x,y,184,28,new Color32(24,30,40,245));
                var text=GrayboxUI.Text(tag,"J"+(p+1)+" · "+directions[p]+" · 17",4,3,176,22,13,GrayboxUI.PlayerColor(p),FontStyle.Bold);text.alignment=TextAnchor.MiddleCenter;
            }
            GrayboxUI.Text(root,"CENTRO · 19",offset.x+107*Zoom,offset.y-30*Zoom,140,20,12,GrayboxUI.Muted,FontStyle.Bold);
        }
        void DrawLine(Vector2 a,Vector2 b,float width,Color color)
        {
            var direction=b-a;var line=GrayboxUI.Box(root,"Conexión",a.x,a.y,direction.magnitude,width,color);line.pivot=new Vector2(0,.5f);line.localRotation=Quaternion.Euler(0,0,-Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg);
        }
    }
}
