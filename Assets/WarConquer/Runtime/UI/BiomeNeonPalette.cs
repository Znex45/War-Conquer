using System;
using UnityEngine;
namespace WarConquer
{
    public static class BiomeNeonPalette
    {
        [Serializable] public class Entry { public string name;public float[] primary,secondary; }
        [Serializable] class Palette { public Entry[] entries; }
        static Palette palette;
        public static Color Neon(Color source)
        {
            Color.RGBToHSV(source,out float h,out float s,out float v);
            return Color.HSVToRGB(h,s,.95f);
        }
        public static void Get(Biome biome,out Color primary,out Color secondary)
        {
            if(palette==null){var data=Resources.Load<TextAsset>("Environment/neon-palettes");palette=data!=null?JsonUtility.FromJson<Palette>(data.text):new Palette{entries=new Entry[0]};}
            string name=(biome==Biome.AshLand?Biome.Wasteland:biome).ToString();
            foreach(var entry in palette.entries)if(entry.name==name)
            {primary=Neon(new Color(entry.primary[0],entry.primary[1],entry.primary[2]));secondary=Neon(new Color(entry.secondary[0],entry.secondary[1],entry.secondary[2]));return;}
            primary=secondary=Neon(GrayboxUI.BiomeColor(biome));
        }
    }
}
