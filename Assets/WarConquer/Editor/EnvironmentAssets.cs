#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
namespace WarConquer.Editor
{
    public static class EnvironmentAssets
    {
        [Serializable] class SurfaceInfo { public string name,texture;public float[] color,emission;public float strength; }
        [Serializable] class SurfaceManifest { public SurfaceInfo[] materials; }
        const string Source="Assets/WarConquer/EnvironmentSource";
        const string Output="Assets/WarConquer/Resources/Environment";
        [MenuItem("War & Conquer/Entornos/Importar biomas y skyboxes")]
        public static void Build()
        {
            Directory.CreateDirectory(Output+"/Biomes");Directory.CreateDirectory(Output+"/Skyboxes");Directory.CreateDirectory(Output+"/Materials");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            foreach(string biome in new[]{"Forest","Swamp","Desert","Tundra","Volcanic","Wasteland","Void"})BuildBiome(biome);
            foreach(string leader in new[]{"Faunar","Zukgrok","Sahria","Xilthar"})
            {
                var file=Directory.GetFiles(Source+"/Sky"+leader+"/textures").First(f=>!f.EndsWith(".meta"));
                var importer=(TextureImporter)AssetImporter.GetAtPath(file);importer.textureShape=TextureImporterShape.Texture2D;importer.maxTextureSize=4096;importer.mipmapEnabled=true;importer.wrapMode=TextureWrapMode.Repeat;importer.SaveAndReimport();
                var mat=GetMaterial(Output+"/Skyboxes/"+leader+".mat",Shader.Find("Skybox/Panoramic"));
                mat.SetTexture("_MainTex",AssetDatabase.LoadAssetAtPath<Texture2D>(file));mat.SetFloat("_Mapping",1);mat.SetFloat("_ImageType",0);mat.SetFloat("_Exposure",.8f);EditorUtility.SetDirty(mat);
            }
            TerrainMeshOptimization.Run();AssetDatabase.SaveAssets();Debug.Log("ENVIRONMENT_IMPORT_COMPLETE: 7 biomes, 4 leader skies");
        }
        static Material GetMaterial(string path,Shader shader)
        {
            var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(mat==null){mat=new Material(shader);AssetDatabase.CreateAsset(mat,path);}else mat.shader=shader;return mat;
        }
        static string Key(string text)=>new string(text.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());
        static void BuildBiome(string biome)
        {
            var files=Directory.GetFiles(Source+"/"+biome,"*",SearchOption.AllDirectories);
            string modelPath=files.FirstOrDefault(f=>f.EndsWith(".fbx",StringComparison.OrdinalIgnoreCase))??files.FirstOrDefault(f=>f.EndsWith(".obj",StringComparison.OrdinalIgnoreCase));
            if(modelPath==null)throw new Exception("Missing converted model: "+biome);
            var importer=(ModelImporter)AssetImporter.GetAtPath(modelPath);importer.importCameras=false;importer.importLights=false;importer.addCollider=false;importer.importAnimation=true;importer.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;importer.SaveAndReimport();
            var textures=files.Where(f=>new[]{".png",".jpg",".jpeg",".tga"}.Contains(Path.GetExtension(f).ToLowerInvariant())).ToArray();
            var manifestPath=Source+"/"+biome+"/materials.json";
            var manifest=File.Exists(manifestPath)?JsonUtility.FromJson<SurfaceManifest>(File.ReadAllText(manifestPath)):null;
            foreach(var original in AssetDatabase.LoadAllAssetsAtPath(modelPath).OfType<Material>())
            {
                string name=Key(original.name);string texture=textures.OrderByDescending(f=>Score(name,Key(Path.GetFileNameWithoutExtension(f)),biome)).FirstOrDefault();
                if(texture!=null&&Score(name,Key(Path.GetFileNameWithoutExtension(texture)),biome)<=0)texture=null;
                var mat=GetMaterial(Output+"/Materials/"+biome+"_"+Key(original.name)+".mat",Shader.Find("Universal Render Pipeline/Lit"));
                Color color=original.HasProperty("_Color")?original.color:Color.white;
                var surface=manifest?.materials.FirstOrDefault(m=>Key(m.name)==name);
                if(surface!=null)
                {
                    color=new Color(surface.color[0],surface.color[1],surface.color[2],surface.color[3]).gamma;
                    texture=string.IsNullOrEmpty(surface.texture)?null:textures.FirstOrDefault(f=>Path.GetFileName(f)==surface.texture);
                }
                mat.SetColor("_BaseColor",texture!=null?Color.white:color);mat.SetTexture("_BaseMap",texture!=null?AssetDatabase.LoadAssetAtPath<Texture2D>(texture):original.mainTexture);mat.SetFloat("_Smoothness",.12f);mat.DisableKeyword("_EMISSION");mat.SetFloat("_Cull",0);mat.enableInstancing=true;
                if(original.HasProperty("_EmissionColor")){var emission=original.GetColor("_EmissionColor");if(emission.maxColorComponent>0){mat.EnableKeyword("_EMISSION");mat.SetColor("_EmissionColor",emission);}}
                if(surface!=null&&surface.strength>0&&surface.emission.Take(3).Any(v=>v>0))
                {mat.EnableKeyword("_EMISSION");mat.SetColor("_EmissionColor",new Color(surface.emission[0],surface.emission[1],surface.emission[2])*surface.strength);}
                EditorUtility.SetDirty(mat);importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material),original.name),mat);
            }
            if(manifest!=null)foreach(var surface in manifest.materials)
            {
                var mat=AssetDatabase.LoadAssetAtPath<Material>(Output+"/Materials/"+biome+"_"+Key(surface.name)+".mat");if(mat==null)continue;
                string texture=string.IsNullOrEmpty(surface.texture)?null:textures.FirstOrDefault(f=>Path.GetFileName(f)==surface.texture);
                var color=new Color(surface.color[0],surface.color[1],surface.color[2],surface.color[3]).gamma;
                mat.SetColor("_BaseColor",texture!=null?Color.white:color);mat.SetTexture("_BaseMap",texture!=null?AssetDatabase.LoadAssetAtPath<Texture2D>(texture):null);
                mat.DisableKeyword("_EMISSION");
                if(surface.strength>0&&surface.emission.Take(3).Any(v=>v>0)){mat.EnableKeyword("_EMISSION");mat.SetColor("_EmissionColor",new Color(surface.emission[0],surface.emission[1],surface.emission[2])*surface.strength);}
                EditorUtility.SetDirty(mat);
            }
            importer.SaveAndReimport();
            var root=new GameObject(biome);try
            {
                var body=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(modelPath));body.transform.SetParent(root.transform,false);
                foreach(var camera in body.GetComponentsInChildren<Camera>(true))UnityEngine.Object.DestroyImmediate(camera.gameObject);
                foreach(var light in body.GetComponentsInChildren<Light>(true))UnityEngine.Object.DestroyImmediate(light);
                var renderers=body.GetComponentsInChildren<Renderer>().Where(r=>r is MeshRenderer||r is SkinnedMeshRenderer).ToArray();
                if(renderers.Length==0)throw new Exception("Empty biome: "+biome);
                var bounds=renderers[0].bounds;foreach(var renderer in renderers.Skip(1))bounds.Encapsulate(renderer.bounds);
                HexBiomeCrop.Build(root,body,bounds,biome,Output);
                PrefabUtility.SaveAsPrefabAsset(root,Output+"/Biomes/"+biome+".prefab");
                Debug.Log("BIOME_READY "+biome+" materials="+root.GetComponentsInChildren<Renderer>().Sum(r=>r.sharedMaterials.Length));
            }finally{UnityEngine.Object.DestroyImmediate(root);}
        }
        static int Score(string mat,string tex,string biome)
        {
            if(tex.Contains("normal")||tex.Contains("opacity")||tex.Contains("objecte"))return -100;
            if(mat==tex)return 100;
            if(tex.Contains(mat)||mat.Contains(tex))return 80;
            foreach(string part in new[]{"land","rock","leaf","grass","tree","water","terrain","object","base","big","gora1","gora2","planeta","zemlya","trava","rashodniki","bark","sand","castle"})
                if(mat.Contains(part)&&tex.Contains(part))return 50+part.Length;
            return biome=="Desert"&&tex=="albedo"?20:0;
        }
        public static void BuildBatch()
        {
            try{Build();EditorApplication.Exit(0);}catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
        }
    }
}
#endif
