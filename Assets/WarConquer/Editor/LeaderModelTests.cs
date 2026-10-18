#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace WarConquer.Editor
{
    public static class LeaderModelTests
    {
        static readonly string[] Leaders={"XIL'THAR","ZUKGROK","FAUNAR","SAHRIA"};
        static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
        public static void BuildAndTestBatch()
        {try{LeaderModelAssets.Build();TestBatch();}catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
        public static void TestBatch()
        {
            double ready=EditorApplication.timeSinceStartup+3;
            EditorApplication.CallbackFunction tick=null;
            tick=()=>{EditorApplication.QueuePlayerLoopUpdate();if(EditorApplication.timeSinceStartup<ready)return;EditorApplication.update-=tick;
            try{Validate();Gallery();EditorApplication.Exit(0);}catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}};
            EditorApplication.update+=tick;
        }
        public static void Validate()
        {
            foreach(string leader in Leaders)
            {
                var root=new GameObject("Leader model check");
                try
                {
                    Check(LeaderModels.Spawn(root.transform,leader),"Falta "+leader);
                    Check(root.GetComponentsInChildren<Transform>().All(t=>t.GetComponents<Component>().All(c=>c!=null)),"Missing Script: "+leader);
                    var animator=root.GetComponentInChildren<Animator>();Check(animator!=null&&animator.runtimeAnimatorController!=null&&!animator.applyRootMotion,"Animator/root motion: "+leader);
                    var clip=animator.runtimeAnimatorController.animationClips.Single();Check(clip.isLooping&&clip.length>=3.9f,"Idle no hace loop: "+leader);
                    var transforms=animator.GetComponentsInChildren<Transform>();var position=animator.transform.localPosition;var rotation=animator.transform.localRotation;var scale=animator.transform.localScale;
                    clip.SampleAnimation(animator.gameObject,0);var rest=transforms.Select(t=>t.localRotation).ToArray();var restPositions=transforms.Select(t=>t.localPosition).ToArray();
                    clip.SampleAnimation(animator.gameObject,1);var changed=transforms.Where((t,i)=>Quaternion.Angle(t.localRotation,rest[i])>.05f||Vector3.Distance(t.localPosition,restPositions[i])>.0001f).ToArray();Check(changed.Length>0,"Idle estático: "+leader);
                    if(leader=="XIL'THAR")Check(changed.All(t=>new[]{"Cube.018","Cube.019","Cube.021","Cube.022","Cube.023","Cube.024","Cube.025"}.Contains(t.name)),"Xil'thar mueve algo distinto a sus rocas.");
                    Check(Vector3.Distance(position,animator.transform.localPosition)<.0001f&&Quaternion.Angle(rotation,animator.transform.localRotation)<.001f&&Vector3.Distance(scale,animator.transform.localScale)<.0001f,"Idle desplaza la raíz: "+leader);
                    clip.SampleAnimation(animator.gameObject,clip.length);Check(transforms.Where((t,i)=>Quaternion.Angle(t.localRotation,rest[i])>.05f||Vector3.Distance(t.localPosition,restPositions[i])>.0001f).Count()==0,"Salto al cerrar loop: "+leader);
                    var renderers=root.GetComponentsInChildren<Renderer>();Check(renderers.Length>0,"Sin mallas: "+leader);
                    var expected=Resources.Load<Texture2D>("LeaderModels/"+LeaderModels.AssetName(leader)+"_BaseColor");
                    foreach(var r in renderers)foreach(var m in r.sharedMaterials)Check(m!=null&&m.shader!=null&&m.shader.name=="Universal Render Pipeline/Lit"&&m.GetTexture("_BaseMap")==expected,"Material incorrecto: "+leader+" / "+r.name+" / "+m?.name);
                    Check(root.GetComponentsInChildren<SkinnedMeshRenderer>().All(r=>r.sharedMesh!=null&&r.bones.All(b=>b!=null)),"Rig roto: "+leader);
                    for(int sample=0;sample<12;sample++)
                    {
                        clip.SampleAnimation(animator.gameObject,clip.length*sample/12f);
                        Check(Vector3.Distance(position,animator.transform.localPosition)<.0001f,"Deriva durante el Idle: "+leader);
                        Check(transforms.All(t=>float.IsFinite(t.position.x)&&float.IsFinite(t.position.y)&&float.IsFinite(t.position.z)),"Transformación inválida: "+leader);
                    }
                    animator.Rebind();animator.Update(0);animator.Update(.5f);
                    Check(animator.GetCurrentAnimatorStateInfo(0).IsName("IDLE")&&animator.GetCurrentAnimatorStateInfo(0).normalizedTime>0,"El Animator no reproduce Idle: "+leader);
                    Debug.Log("LEADER_ASSET_PASSED "+leader+": rig, material, Animator, Idle loop y raíz fija.");
                }
                finally{UnityEngine.Object.DestroyImmediate(root);}
            }
            var fixture=new GameObject("Leader board check");
            try
            {
                var g=new GameManager(CardCatalog.Load());g.NewGame(Leaders,2026,CardCatalog.LoadRules(),4);
                var world=fixture.AddComponent<Board3DScene>();string before=GamePersistence.Serialize(g.State);
                world.Sync(g,new HashSet<int>(),new HashSet<int>(),-1);
                foreach(var tile in g.State.tiles.Where(t=>t.baseOwner>=0))
                {
                    var root=world.transform.Find("HEX "+(tile.id+1)+"/BASE J"+(tile.baseOwner+1));
                    Check(root!=null&&root.Find("Líder 3D · "+Leaders[tile.baseOwner])!=null,"Líder intercambiado.");
                    Check(root.GetComponent<BoxCollider>()!=null&&root.GetComponent<Hex3DTarget>().tileId==tile.id,"Selección perdida.");
                    Check(root.localPosition==Vector3.up*BoardMeshFactory.Surface,"Punto de aparición cambiado.");
                }
                Check(before==GamePersistence.Serialize(g.State),"La sustitución visual modifica gameplay.");
                g.State.players[0].leader="SAHRIA";world.Sync(g,new HashSet<int>(),new HashSet<int>(),-1);
                var home=g.State.tiles.First(t=>t.baseOwner==0);Check(world.transform.Find("HEX "+(home.id+1)+"/BASE J1/Líder 3D · SAHRIA")!=null,"Cambio de líder no actualiza modelo.");
                Debug.Log("LEADER_BOARD_PASSED: cuatro identidades, bases, colliders, cambio de líder y estado intacto.");
            }
            finally{UnityEngine.Object.DestroyImmediate(fixture);}
        }
        public static void Gallery()
        {
            var root=new GameObject("Leader visual QA");RenderTexture texture=null;Texture2D pixels=null;
            try
            {
                for(int i=0;i<Leaders.Length;i++)
                {var slot=new GameObject(Leaders[i]);slot.transform.SetParent(root.transform);slot.transform.position=new Vector3((i-1.5f)*2.4f,0,0);LeaderModels.Spawn(slot.transform,Leaders[i]);var a=slot.GetComponentInChildren<Animator>();a.runtimeAnimatorController.animationClips[0].SampleAnimation(a.gameObject,1);}
                foreach(var t in root.GetComponentsInChildren<Transform>())t.gameObject.layer=29;
                var key=new GameObject("QA key");key.transform.SetParent(root.transform);key.transform.rotation=Quaternion.Euler(40,-35,0);var light=key.AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.4f;light.cullingMask=1<<29;
                var fill=new GameObject("QA fill");fill.transform.SetParent(root.transform);fill.transform.rotation=Quaternion.Euler(30,145,0);var fillLight=fill.AddComponent<Light>();fillLight.type=LightType.Directional;fillLight.intensity=.6f;fillLight.cullingMask=1<<29;
                var camObject=new GameObject("QA camera");camObject.transform.SetParent(root.transform);var camera=camObject.AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=1.65f;camera.cullingMask=1<<29;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.055f,.065f,.09f);camera.nearClipPlane=.1f;camera.farClipPlane=30;
                camera.orthographicSize=2.2f;camera.transform.position=new Vector3(0,2.4f,-7);camera.transform.LookAt(new Vector3(0,1.1f,0));
                texture=new RenderTexture(1800,700,24);texture.Create();camera.targetTexture=texture;camera.aspect=1800f/700;camera.Render();
                var previous=RenderTexture.active;RenderTexture.active=texture;pixels=new Texture2D(1800,700,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,1800,700),0,0);pixels.Apply();RenderTexture.active=previous;
                Directory.CreateDirectory("Library/LeaderModelQA");File.WriteAllBytes("Library/LeaderModelQA/leaders-gallery.png",pixels.EncodeToPNG());
                Debug.Log("LEADER_GALLERY_RENDERED: Xil'thar, Zukgrok, Faunar, Sahria (izquierda a derecha).");
            }
            finally{UnityEngine.Object.DestroyImmediate(root);if(texture!=null){texture.Release();UnityEngine.Object.DestroyImmediate(texture);}if(pixels!=null)UnityEngine.Object.DestroyImmediate(pixels);}
        }
    }
}
#endif
