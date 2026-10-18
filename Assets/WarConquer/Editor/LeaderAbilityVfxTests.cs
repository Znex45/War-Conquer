#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace WarConquer.Editor
{
    public static class LeaderAbilityVfxTests
    {
        static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
        static void Sync(Board3DScene b,GameManager g)=>b.Sync(g,new HashSet<int>(),new HashSet<int>(),-1);
        public static void PreviewBatch()
        {
            double ready=EditorApplication.timeSinceStartup+3;EditorApplication.CallbackFunction tick=null;
            tick=()=>{EditorApplication.QueuePlayerLoopUpdate();if(EditorApplication.timeSinceStartup<ready)return;EditorApplication.update-=tick;try{Preview();EditorApplication.Exit(0);}catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}};EditorApplication.update+=tick;
        }
        static void Preview()
        {
            bool asyncShaders=ShaderUtil.allowAsyncCompilation;ShaderUtil.allowAsyncCompilation=false;
            var root=new GameObject("Ability preview");
            try
            {
                var g=new GameManager(CardCatalog.Load());g.NewGame(new[]{"XIL'THAR","ZUKGROK","FAUNAR","SAHRIA"},2026,CardCatalog.LoadRules(),4);var board=root.AddComponent<Board3DScene>();Sync(board,g);
                var t=g.State.tiles.First(x=>x.owner==0&&x.baseOwner<0);for(int i=0;i<4;i++)TerrainManager.Terraform(g,t.id,i%2==0?Biome.Forest:Biome.Desert,0);
                t=g.State.tiles.First(x=>x.owner==1&&x.baseOwner<0);CombatManager.DirectDamage(g,g.Place("firefly-token",1,t.id),99);ChoiceManager.Resolve(g,new[]{(int)Biome.Forest});
                g.State.activePlayer=3;g.State.stage=TurnStage.Deployment;g.State.Active.currentEnergy=20;g.SetSahriaDeployment(true);t=g.State.tiles.First(x=>x.owner==3&&x.baseOwner<0);TerrainManager.Terraform(g,t.id,Biome.Swamp,3);
                var c=PrototypeScenario.Take(g,3,"dune-cammel");g.State.Active.hand.Add(c);Check(g.Play(c.instanceId,new[]{t.id}),"Sahria preview");Sync(board,g);
                var vfx=root.GetComponent<BoardLeaderAbilityVfx>();vfx.Tick(.45f);board.ResizeTexture(1000,700);
                System.IO.Directory.CreateDirectory("Library/LeaderAbilityQA");
                foreach(var p in g.State.players)
                {
                    Vector3 pos=board.LeaderVisual(p.id).position;board.Zoom=5;board.Pan=new Vector2(-pos.x/.016f,pos.z/.016f);board.UpdateCamera();board.BoardCamera.Render();
                    var previous=RenderTexture.active;RenderTexture.active=board.Texture;var image=new Texture2D(1000,700,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1000,700),0,0);image.Apply();RenderTexture.active=previous;
                    System.IO.File.WriteAllBytes("Library/LeaderAbilityQA/"+p.leader.Replace("'","")+".png",image.EncodeToPNG());UnityEngine.Object.DestroyImmediate(image);
                }
                Debug.Log("LEADER_VFX_VISUAL_PASSED");
            }
            finally{UnityEngine.Object.DestroyImmediate(root);ShaderUtil.allowAsyncCompilation=asyncShaders;}
        }
        public static void RunBatch()
        {
            try{Run();EditorApplication.Exit(0);}catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
        }
        public static void Run()
        {
            foreach(int count in new[]{2,3,4})
            {
                var root=new GameObject("Leader VFX QA");
                try
                {
                    var g=new GameManager(CardCatalog.Load());g.NewGame(new[]{"XIL'THAR","ZUKGROK","FAUNAR","SAHRIA"},2026,CardCatalog.LoadRules(),count);
                    var board=root.AddComponent<Board3DScene>();Sync(board,g);var vfx=root.GetComponent<BoardLeaderAbilityVfx>();
                    int signals=0;g.LeaderAbilityResolved+=s=>signals++;
                    var tile=g.State.tiles.First(t=>t.owner==0&&t.baseOwner<0);
                    for(int i=1;i<=8;i++){TerrainManager.Terraform(g,tile.id,i%2==0?Biome.Desert:Biome.Forest,0);Sync(board,g);Check(signals==i/4&&vfx.ActivationCount==i/4,"Xil recompensa incorrecta "+i);}
                    Check(!g.Terraform(-1,Biome.Forest)&&signals==2,"Terraformación inválida emite VFX.");
                    string before=GamePersistence.Serialize(g.State);vfx.Tick(2);Check(vfx.ActiveBursts==0&&GamePersistence.Serialize(g.State)==before,"VFX modifica estado o no limpia.");
                    var death=g.State.tiles.First(t=>t.owner==1&&t.baseOwner<0&&!t.IsOccupied);var token=g.Place("firefly-token",1,death.id);CombatManager.DirectDamage(g,token,99);Sync(board,g);
                    Check(signals==2&&g.State.pendingChoices.Count>0,"Token dispara antes de resolver.");
                    Check(ChoiceManager.Resolve(g,new[]{(int)Biome.Swamp}),"No resuelve muerte.");Sync(board,g);Check(signals==3&&vfx.CloudCount==3,"No hay tres nubes al Slab real.");
                    Check(vfx.GetComponentsInChildren<TrailRenderer>().Length==3,"Faltan trails.");vfx.Tick(2);Check(vfx.CloudCount==0&&vfx.ActiveBursts==0,"Nubes no limpian.");
                    var another=g.State.tiles.First(t=>t.owner==1&&t.baseOwner<0&&!t.IsOccupied);token=g.Place("firefly-token",1,another.id);CombatManager.DirectDamage(g,token,99);ChoiceManager.Resolve(g,Array.Empty<int>(),true);Sync(board,g);Check(signals==3,"Cancelar muerte dispara VFX.");
                    g.Restore(GamePersistence.Deserialize(GamePersistence.Serialize(g.State)));Sync(board,g);Check(vfx.ActiveBursts==0,"Carga repite VFX.");
                    Debug.Log("LEADER_VFX_MAP_PASSED "+count+": Xil 1/2/3/4/8, Token, destino, trails, cancelación, carga, limpieza.");
                }
                finally{UnityEngine.Object.DestroyImmediate(root);}
            }
            foreach(string leader in new[]{"SAHRIA","FAUNAR"})
            {
                var root=new GameObject("Leader deployment VFX QA");
                try
                {
                    var g=new GameManager(CardCatalog.Load());g.NewGame(new[]{leader,"ZUKGROK"},2026,CardCatalog.LoadRules(),2);var board=root.AddComponent<Board3DScene>();Sync(board,g);var vfx=root.GetComponent<BoardLeaderAbilityVfx>();
                    var tile=g.State.tiles.First(t=>t.owner==0&&t.baseOwner<0&&!t.IsOccupied);var card=PrototypeScenario.Take(g,0,leader=="SAHRIA"?"dune-cammel":"berry-bush");g.State.Active.hand.Add(card);g.State.Active.currentEnergy=20;
                    if(leader=="SAHRIA"){TerrainManager.Terraform(g,tile.id,Biome.Swamp,0);g.SetSahriaDeployment(true);Sync(board,g);Check(vfx.ActivationCount==0,"Botón Sahria dispara VFX.");}
                    Check(!g.Play(card.instanceId,new[]{-1}),"Objetivo inválido aceptado.");
                    Check(vfx.ActivationCount==0,"Acción inválida dispara.");
                    Check(g.Play(card.instanceId,new[]{tile.id}),"Despliegue falla "+g.LastMessage);Sync(board,g);
                    if(leader=="SAHRIA")Check(vfx.ActiveBursts==2&&board.UnitVisual(tile.unit.id)!=null,"Aura no sigue unidad real.");
                    else{Check(vfx.FoliageCount==1&&g.State.Active.firstStructureUsed,"Disponibilidad Faunar incorrecta.");vfx.Tick(1);Check(vfx.GetComponentsInChildren<Transform>().Where(t=>t.name=="Hoja").All(t=>t.localScale==Vector3.zero),"Hojas no caen/desaparecen.");}
                    Check(g.Undo(),"No permite deshacer.");Sync(board,g);Check(vfx.ActiveBursts==0,"Deshacer conserva VFX.");vfx.Tick(1);
                    if(leader=="FAUNAR")Check(vfx.GetComponentsInChildren<Transform>().Any(t=>t.name=="Hoja"&&t.localScale.sqrMagnitude>0),"Hojas no vuelven.");
                    Debug.Log("LEADER_VFX_DEPLOY_PASSED "+leader);
                }
                finally{UnityEngine.Object.DestroyImmediate(root);}
            }
        }
    }
}
#endif
