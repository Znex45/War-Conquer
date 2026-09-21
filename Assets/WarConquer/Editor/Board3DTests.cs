#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WarConquer.Editor
{
    public static class Board3DTests
    {
        static void Check(bool condition,string message){if(!condition)throw new Exception(message);}
        sealed class Fixture : IDisposable
        {
            public readonly GameManager game;
            public readonly Board3DScene world;
            readonly GameObject root;
            public Fixture(CardCatalog catalog,int humans=4)
            {
                game=new GameManager(catalog);game.NewGame(new[]{"ZUKGROK","SAHRIA","ZUKGROK","SAHRIA"},2026,CardCatalog.LoadRules(),humans);
                GrayboxUI.PlayerLeaders=game.State.players.Select(p=>p.leader).ToArray();
                root=new GameObject("3D test fixture");world=root.AddComponent<Board3DScene>();world.Initialize();world.BoardCamera.aspect=982f/648;Sync();
            }
            public void Sync()=>world.Sync(game,new HashSet<int>(),new HashSet<int>(),-1);
            public void Dispose(){UnityEngine.Object.DestroyImmediate(root);}
        }
        public static void RunAll(CardCatalog catalog,Action<string,Action> test)
        {
            test("3D: 87 prismas con volumen, centros originales y selección por collider",()=>{
                using(var f=new Fixture(catalog))
                {
                    Check(f.world.TileCount==87,"No hay 87 casillas 3D.");
                    foreach(var tile in f.game.State.tiles)
                    {
                        var root=f.world.transform.Find("HEX "+(tile.id+1));Check(root!=null&&root.localPosition==Board3DScene.Position(tile),"Coordenadas de casilla alteradas.");
                        var collider=root.Find("Casilla hexagonal 3D").GetComponent<MeshCollider>();
                        Check(collider.sharedMesh.bounds.size.y>.3f&&collider.GetComponent<Hex3DTarget>().tileId==tile.id,"Casilla plana o sin identidad de selección.");
                    }
                    int picked=f.game.State.tiles.Count(t=>f.world.Pick(f.world.ViewportOf(t.id,t.baseOwner>=0?1.2f:BoardMeshFactory.Surface))==t.id);
                    Check(picked>=75,"Selección de cámara incorrecta: "+picked+"/87 casillas visibles.");
                }
            });
            test("3D: los 87 hexágonos siguen conectados borde con borde y sin puentes",()=>{
                using(var f=new Fixture(catalog))
                {
                    foreach(var t in f.game.State.tiles)foreach(int n in t.neighbors)
                        Check(Math.Abs(Vector3.Distance(Board3DScene.Position(t),Board3DScene.Position(f.game.State.tiles[n]))-1.385640646f)<.0001f,"Separación 3D cambia una conexión.");
                    Check(f.game.State.connections.All(c=>!c.bridge),"Apareció un puente.");
                }
            });
            test("3D: movimiento, daño y muerte actualizan una sola miniatura sin mutar reglas",()=>{
                using(var f=new Fixture(catalog))
                {
                    var g=f.game;int home=g.State.tiles.First(t=>t.owner==0&&t.baseOwner<0).id;var piece=PrototypeScenario.Spawn(g,0,"bestia-micelial",home);f.Sync();
                    Check(f.world.PieceCount==1,"Falta miniatura.");var visual=f.world.GetComponentsInChildren<Hex3DTarget>().First(t=>t.name.StartsWith("UNIDAD "));
                    g.State.stage=TurnStage.Assault;int destination=MovementManager.Paths(g,piece).Keys.First();Check(MovementManager.Move(g,piece,destination),"Movimiento fallido.");f.Sync();
                    Check(visual.tileId==destination&&visual.transform.localPosition==Board3DScene.Position(g.State.tiles[destination])+Vector3.up*BoardMeshFactory.Surface,"Miniatura no sigue a la unidad.");
                    CombatManager.Damage(g,piece,1,false);string before=GamePersistence.Serialize(g.State);f.Sync();
                    Check(visual.GetComponentInChildren<TextMesh>().text.Contains(piece.health+" ♥")&&before==GamePersistence.Serialize(g.State),"Vida visual incorrecta o render altera estado.");
                    CombatManager.Damage(g,piece,100,false);f.Sync();Check(f.world.PieceCount==0,"Queda una miniatura de unidad destruida.");
                }
            });
            test("3D: estructuras, biomas y selección se actualizan conservando mazos y energía",()=>{
                using(var f=new Fixture(catalog))
                {
                    var g=f.game;int home=g.State.tiles.First(t=>t.owner==0&&t.baseOwner<0).id;var piece=PrototypeScenario.Spawn(g,0,"jardin-micelial",home);
                    g.State.stage=TurnStage.Terraforming;Check(g.Terraform(home,Biome.Forest),"Terraformación fallida.");
                    string before=GamePersistence.Serialize(g.State);f.world.Sync(g,new HashSet<int>{home},new HashSet<int>(),home);
                    Check(f.world.GetComponentsInChildren<Transform>().Any(t=>t.name.StartsWith("ESTRUCTURA Jardín")),"Falta estructura 3D.");
                    Check(f.world.transform.Find("HEX "+(home+1)).Find("Bioma Bosque")!=null,"Bioma 3D desactualizado.");
                    Check(before==GamePersistence.Serialize(g.State)&&StateValidator.Validate(g.State,catalog)=="","Vista modifica la partida.");
                }
            });
            test("3D: cámara giratoria y zoom mantienen coordenadas de selección y no afectan turnos",()=>{
                using(var f=new Fixture(catalog))
                {
                    string before=GamePersistence.Serialize(f.game.State);int center=f.game.State.tiles.First(ConquestManager.IsCenter).id;
                    foreach(float yaw in new[]{0f,90f,180f,270f})
                    {
                        f.world.Yaw=yaw;f.world.Zoom=1.6f;f.world.Pan=new Vector2(40,-20);f.world.UpdateCamera();
                        Check(f.world.Pick(f.world.ViewportOf(center))==center,"Raycast desajustado después de girar.");
                    }
                    Check(f.world.Pick(new Vector2(-.1f,.5f))==-1&&before==GamePersistence.Serialize(f.game.State),"Cámara cambia la partida o acepta un clic fuera del tablero.");
                }
            });
            test("3D: entrada normalizada funciona con el panel y escalado de interfaz",()=>{
                var holder=new GameObject("Viewport fixture",typeof(RectTransform));try
                {
                    var rect=holder.GetComponent<RectTransform>();rect.pivot=new Vector2(0,1);rect.sizeDelta=new Vector2(982,648);
                    Check(Vector2.Distance(BoardViewportInput.Normalize(rect,new Vector2(491,-324)),new Vector2(.5f,.5f))<.0001f,"Centro normalizado incorrecto.");
                    Check(BoardViewportInput.Normalize(rect,new Vector2(0,-648))==Vector2.zero,"Origen invertido.");
                }finally{UnityEngine.Object.DestroyImmediate(holder);}
            });
            test("3D: el modo solitario conserva solo dos bases y todas las cartas",()=>{
                using(var f=new Fixture(catalog,1))
                {
                    Check(f.world.GetComponentsInChildren<Transform>().Count(t=>t.name.StartsWith("BASE J"))==2,"Bases fantasma en modo IA.");
                    Check(StateValidator.Validate(f.game.State,catalog)==""&&f.game.State.players[1].isAI,"Vista elimina la configuración de IA.");
                }
            });
        }
    }
}
#endif
