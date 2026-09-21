#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace WarConquer.Editor
{
    public static class Board3DAssets
    {
        public static void Ensure()
        {
            const string folder="Assets/WarConquer/Resources/WarConquer/Materials";
            Directory.CreateDirectory(folder);AssetDatabase.Refresh();
            Create(folder+"/BoardURP.mat","Universal Render Pipeline/Lit");
            Create(folder+"/BoardStandard.mat","Standard");
            AssetDatabase.SaveAssets();
        }
        static void Create(string path,string shaderName)
        {
            if(AssetDatabase.LoadAssetAtPath<Material>(path)!=null)return;
            var shader=Shader.Find(shaderName);if(shader==null)throw new System.InvalidOperationException("Shader 3D ausente: "+shaderName);
            var material=new Material(shader);material.SetColor("_Color",Color.white);material.SetColor("_BaseColor",Color.white);
            if(material.HasProperty("_Smoothness"))material.SetFloat("_Smoothness",.2f);
            if(material.HasProperty("_Glossiness"))material.SetFloat("_Glossiness",.2f);
            material.enableInstancing=true;AssetDatabase.CreateAsset(material,path);
        }
    }
}
#endif
