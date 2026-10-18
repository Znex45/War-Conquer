#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace WarConquer.Editor
{
    public static class LeaderModelAssets
    {
        public const string Folder="Assets/WarConquer/Resources/LeaderModels";
        [Serializable] class MaterialInfo { public string name;public float metallic,roughness,emissionStrength;public float[] emission; }
        [Serializable] class LeaderInfo { public string name;public MaterialInfo[] materials; }
        [Serializable] class Manifest { public LeaderInfo[] leaders; }
        [MenuItem("War & Conquer/Modelos de líderes/Preparar modelos importados")]
        public static void Build()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText(Folder+"/import-manifest.json"));
            foreach(var info in manifest.leaders)
            {
                string modelPath=Folder+"/"+info.name+".fbx";
                var importer=(ModelImporter)AssetImporter.GetAtPath(modelPath);
                importer.animationType=ModelImporterAnimationType.Generic;importer.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;
                importer.importAnimation=true;importer.optimizeGameObjects=false;importer.addCollider=false;importer.isReadable=false;
                importer.meshCompression=ModelImporterMeshCompression.Off;importer.importCameras=false;importer.importLights=false;
                importer.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;
                importer.SaveAndReimport();
                var clips=importer.defaultClipAnimations;
                if(clips.Length==0)throw new Exception("No hay Idle importado: "+info.name);
                foreach(var clip in clips){clip.name="IDLE";clip.loopTime=true;clip.loopPose=true;clip.lockRootRotation=true;clip.lockRootHeightY=true;clip.lockRootPositionXZ=true;}
                var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(Folder+"/"+info.name+"_BaseColor.png");
                var textureImporter=(TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture));
                textureImporter.maxTextureSize=2048;textureImporter.mipmapEnabled=true;textureImporter.sRGBTexture=true;textureImporter.textureCompression=TextureImporterCompression.Compressed;textureImporter.SaveAndReimport();
                for(int i=0;i<info.materials.Length;i++)
                {
                    var source=info.materials[i];string path=Folder+"/"+info.name+"_Material_"+i+".mat";
                    var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
                    if(mat==null){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(mat,path);}
                    mat.name=info.name+" · "+source.name;mat.SetColor("_BaseColor",Color.white);mat.SetTexture("_BaseMap",texture);
                    mat.SetFloat("_Metallic",source.metallic);mat.SetFloat("_Smoothness",1-source.roughness);mat.SetFloat("_Cull",0);
                    if(source.emissionStrength>0&&source.emission.Take(3).Any(v=>v>0))
                    {mat.EnableKeyword("_EMISSION");mat.SetColor("_EmissionColor",new Color(source.emission[0],source.emission[1],source.emission[2])*source.emissionStrength);}
                    mat.enableInstancing=true;EditorUtility.SetDirty(mat);
                    importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material),source.name),mat);
                }
                importer.clipAnimations=clips;
                importer.SaveAndReimport();
                var model=AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
                var idle=AssetDatabase.LoadAllAssetsAtPath(modelPath).OfType<AnimationClip>().First(c=>c.name=="IDLE");
                string controllerPath=Folder+"/"+info.name+"_Idle.controller";
                var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
                if(controller==null)controller=AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
                var machine=controller.layers[0].stateMachine;
                var state=machine.states.FirstOrDefault(s=>s.state.name=="IDLE").state??machine.AddState("IDLE");state.motion=idle;machine.defaultState=state;
                var root=new GameObject(info.name+"_Leader");
                try
                {
                    var alignment=new GameObject("Visual alignment").transform;alignment.SetParent(root.transform,false);
                    var body=(GameObject)PrefabUtility.InstantiatePrefab(model);body.transform.SetParent(alignment,false);
                    alignment.localRotation=Quaternion.Euler(0,270,0);
                    var renderers=body.GetComponentsInChildren<Renderer>();if(renderers.Length==0)throw new Exception("Modelo vacío: "+info.name);
                    var bounds=renderers[0].bounds;foreach(var r in renderers.Skip(1))bounds.Encapsulate(r.bounds);
                    float scale=1.75f*Mathf.Min(1.38f/bounds.size.y,1.1f/Mathf.Max(bounds.size.x,bounds.size.z));
                    alignment.localScale=Vector3.one*scale;alignment.localPosition=new Vector3(-bounds.center.x,-bounds.min.y,-bounds.center.z)*scale;
                    var animator=body.GetComponent<Animator>()??body.AddComponent<Animator>();animator.runtimeAnimatorController=controller;animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
                    foreach(var child in root.GetComponentsInChildren<Transform>(true))child.gameObject.layer=BoardMeshFactory.Layer;
                    PrefabUtility.SaveAsPrefabAsset(root,Folder+"/"+info.name+"_Leader.prefab");
                }
                finally{UnityEngine.Object.DestroyImmediate(root);}
                Debug.Log("LEADER_MODEL_READY "+info.name);
            }
            AssetDatabase.SaveAssets();
        }
        public static void BuildBatch(){try{Build();EditorApplication.Exit(0);}catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
    }
}
#endif
