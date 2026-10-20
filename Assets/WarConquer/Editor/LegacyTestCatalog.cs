#if UNITY_EDITOR
using System.IO;
using UnityEngine;
namespace WarConquer.Editor
{
    internal static class LegacyTestCatalog
    {
        public static CardCatalog Load()=>new CardCatalog(JsonUtility.FromJson<CatalogFile>(File.ReadAllText("Assets/WarConquer/Editor/Fixtures/cards-before-sahria-zukgrok.json")).cards);
    }
}
#endif
