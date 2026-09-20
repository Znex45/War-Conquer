using System;
using System.Linq;
using UnityEngine;

namespace WarConquer
{
    [Serializable] public class GameSnapshot
    {
        public int format=2;
        public bool hasBattle;
        public GameState state;
        public int[] unitSlots,structureSlots,terrainSlots;
    }
    public static class GamePersistence
    {
        // Unity serializes inline custom classes by value, including null slots.
        // Occupancy masks retain explicit absence instead of reconstructing phantom pieces on load.
        public static string Serialize(GameState s)
        {
            return JsonUtility.ToJson(new GameSnapshot {state=s,hasBattle=s.battle!=null,
                unitSlots=s.tiles.Where(t=>t.unit!=null).Select(t=>t.id).ToArray(),
                structureSlots=s.tiles.Where(t=>t.structure!=null).Select(t=>t.id).ToArray(),
                terrainSlots=s.tiles.Where(t=>t.specialEffect!=null).Select(t=>t.id).ToArray()},true);
        }
        public static GameState Deserialize(string json)
        {
            var snapshot=JsonUtility.FromJson<GameSnapshot>(json);
            if(snapshot==null||snapshot.format!=2||snapshot.state?.tiles==null||snapshot.unitSlots==null||snapshot.structureSlots==null||snapshot.terrainSlots==null)
                throw new InvalidOperationException("Formato de guardado no reconocido.");
            if(!snapshot.hasBattle)snapshot.state.battle=null;
            foreach(var tile in snapshot.state.tiles)
            {
                if(!snapshot.unitSlots.Contains(tile.id))tile.unit=null;
                if(!snapshot.structureSlots.Contains(tile.id))tile.structure=null;
                if(!snapshot.terrainSlots.Contains(tile.id))tile.specialEffect=null;
            }
            return snapshot.state;
        }
    }
}
