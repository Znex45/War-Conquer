using System.Collections.Generic;
using UnityEngine;
namespace WarConquer
{
    public static class ImportedBiomeVisuals
    {
        static readonly Dictionary<Biome,GameObject> models=new Dictionary<Biome,GameObject>();
        public static bool TryBuild(Transform parent,HexTile tile,out BoardBiomeVisuals.Habitat habitat)
        {
            habitat=null;if(tile.biome==Biome.Neutral)return false;
            var biome=tile.biome==Biome.AshLand?Biome.Wasteland:tile.biome;
            if(!models.TryGetValue(biome,out var prefab))models[biome]=prefab=Resources.Load<GameObject>("Environment/Biomes/"+biome);
            if(prefab==null)return false;
            var instance=Object.Instantiate(prefab,parent,false);instance.name="Paisaje importado - "+biome;
            foreach(var child in instance.GetComponentsInChildren<Transform>(true))child.gameObject.layer=BoardMeshFactory.Layer;
            // Animate only scenery whose names indicate motion. Keep the complete terrain rigid.
            instance.AddComponent<ImportedBiomeMotion>().Initialize(biome,tile.id);
            habitat=new BoardBiomeVisuals.Habitat{root=parent,biome=tile.biome};return true;
        }
    }
    public sealed class ImportedBiomeMotion:MonoBehaviour
    {
        struct Detail { public Transform node;public Vector3 position;public Quaternion rotation; }
        struct Glow { public Renderer renderer;public int slot;public Color color; }
        readonly List<Detail> details=new List<Detail>();
        readonly List<Glow> glows=new List<Glow>();
        MaterialPropertyBlock glowProperties;
        Biome biome;float offset,nextFrame;
        public void Initialize(Biome value,int seed)
        {
            glowProperties=new MaterialPropertyBlock();biome=value;offset=seed*.73f;
            if(biome==Biome.Volcanic||biome==Biome.Void)
                foreach(var renderer in GetComponentsInChildren<Renderer>())
                {
                    var materials=renderer.sharedMaterials;
                    for(int i=0;i<materials.Length;i++)
                        if(materials[i]!=null&&materials[i].IsKeywordEnabled("_EMISSION")&&materials[i].HasProperty("_EmissionColor"))
                            glows.Add(new Glow{renderer=renderer,slot=i,color=materials[i].GetColor("_EmissionColor")});
                }
            foreach(var node in GetComponentsInChildren<Transform>())
            {
                if(node==transform)continue;string name=node.name.ToLowerInvariant();
                if(name.Contains("water")||name.Contains("lava")||name.Contains("leaf")||name.Contains("grass")||name.Contains("planet"))
                    details.Add(new Detail{node=node,position=node.localPosition,rotation=node.localRotation});
            }
            enabled=details.Count>0||glows.Count>0;
        }
        void Update()
        {
            float now=Time.unscaledTime;if(now<nextFrame)return;nextFrame=now+1f/30;
            float wave=Mathf.Sin(now*1.1f+offset);
            foreach(var glow in glows)
            {
                if(glow.renderer==null)continue;
                glow.renderer.GetPropertyBlock(glowProperties,glow.slot);
                glowProperties.SetColor("_EmissionColor",glow.color*(.85f+.15f*wave));
                glow.renderer.SetPropertyBlock(glowProperties,glow.slot);
            }
            foreach(var d in details)
            {
                if(d.node==null)continue;
                // Rotation is independent of the source model's units and preserves its authored pose.
                d.node.localRotation=d.rotation*Quaternion.Euler(0,biome==Biome.Void?wave*3:0,biome==Biome.Forest?wave*1.4f:wave*.3f);
            }
        }
    }
}

