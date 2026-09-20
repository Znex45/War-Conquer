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
            float scale=32.5f*Zoom;Vector2 offset=new Vector2(490,320)+Pan;
            foreach(var tile in s.tiles)centers[tile.id]=new Vector2(offset.x+tile.x*scale,offset.y-tile.y*scale);
            foreach(var t in s.tiles)
            {
                int id=t.id;var pos=centers[id];float size=1.6f*scale;
                var rect=GrayboxUI.Rect(root,"Hex "+(id+1),pos.x-size/2,pos.y-size/2,size,size);
                var hex=rect.gameObject.AddComponent<HexGraphic>();hex.color=t.biome==Biome.Neutral?GrayboxUI.TerritoryColor(t.territory):GrayboxUI.BiomeColor(t.biome);
                hex.border=selected.Contains(id)?Color.white:valid.Contains(id)?GrayboxUI.Green:focus==id?new Color32(248,165,79,255):new Color32(27,34,37,255);
                hex.Click=()=>controller.TileClick(id);
                var label=GrayboxUI.Text(rect,(id+1).ToString(),size*.25f,size*.09f,size*.5f,12*Zoom,Mathf.RoundToInt(10*Zoom),new Color32(237,233,213,255));label.alignment=TextAnchor.UpperCenter;
                if(t.owner>=0)
                {
                    var ownership=GrayboxUI.Text(rect,"J"+(t.owner+1),size*.3f,size*.75f,size*.4f,11*Zoom,Mathf.RoundToInt(9*Zoom),GrayboxUI.Ink);ownership.alignment=TextAnchor.UpperCenter;
                }
                if(t.blocked)GrayboxUI.Text(rect,"▲",size*.22f,size*.25f,size*.7f,size*.5f,Mathf.RoundToInt(23*Zoom),GrayboxUI.Muted);
                if(t.baseOwner>=0)
                {
                    var baseRect=GrayboxUI.Rect(rect,"Base J"+(t.baseOwner+1),size*.19f,size*.21f,size*.62f,size*.62f);
                    var baseHex=baseRect.gameObject.AddComponent<HexGraphic>();baseHex.color=GrayboxUI.Background;baseHex.border=GrayboxUI.PlayerColor(t.baseOwner);baseHex.raycastTarget=false;
                    var mark=GrayboxUI.Text(baseRect,"J"+(t.baseOwner+1),0,0,size*.62f,size*.62f,Mathf.RoundToInt(16*Zoom),GrayboxUI.PlayerColor(t.baseOwner),FontStyle.Bold);mark.alignment=TextAnchor.MiddleCenter;
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
                else if(t.resource>0&&t.baseOwner<0){var res=GrayboxUI.Text(rect,t.territory==4&&t.q==0&&t.r==0?"CENTRO\n+2":"+"+t.resource,0,size*.31f,size,size*.5f,Mathf.RoundToInt(10*Zoom),GrayboxUI.Ink,FontStyle.Bold);res.alignment=TextAnchor.MiddleCenter;}
                if(t.specialEffect!=null){var unstable=GrayboxUI.Text(rect,"!",size*.70f,size*.33f,10*Zoom,15*Zoom,Mathf.RoundToInt(13*Zoom),GrayboxUI.Yellow,FontStyle.Bold);}
                var routes=s.fastRoutes.Select((route,index)=>new{route,index}).Where(p=>p.route.a==id||p.route.b==id).ToList();
                if(routes.Count>0)
                {
                    var marker=GrayboxUI.Text(rect,string.Join("/",routes.Select(p=>"R"+(p.index+1))),0,size*.64f,size,12*Zoom,Mathf.RoundToInt(9*Zoom),GrayboxUI.Purple,FontStyle.Bold);marker.alignment=TextAnchor.MiddleCenter;
                }
            }
            string[] directions={"NORTE","ESTE","SUR","OESTE"};
            Vector2[] tags={new Vector2(-305,-245),new Vector2(237,-115),new Vector2(140,223),new Vector2(-421,-115)};
            for(int p=0;p<4;p++)
            {
                var position=offset+tags[p]*Zoom;
                var tag=GrayboxUI.Box(root,"Territorio "+(p+1),position.x,position.y,184,45,new Color32(24,30,40,245));
                GrayboxUI.Box(tag,"Color de zona",0,0,4,45,GrayboxUI.TerritoryColor(p));
                var text=GrayboxUI.Text(tag,"J"+(p+1)+" · "+directions[p]+"\n"+s.players[p].conquestPoints+" / 10 PC · 17 HEX",8,3,172,40,13,GrayboxUI.Ink,FontStyle.Bold);text.alignment=TextAnchor.MiddleCenter;
            }
            GrayboxUI.Text(root,"CENTRO · 19 CASILLAS\nTerreno inicial sin bioma",16,562,220,42,12,GrayboxUI.Muted);
        }
    }
}
