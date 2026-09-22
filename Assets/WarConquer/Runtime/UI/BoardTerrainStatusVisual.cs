using UnityEngine;
namespace WarConquer
{
    public sealed class BoardTerrainStatusVisual:MonoBehaviour
    {
        Transform ring;TextMesh label;Camera view;
        public void Initialize(BoardMeshFactory mesh,Camera camera)
        {
            view=camera;ring=mesh.Part(transform,"Anillo de terreno inestable",mesh.Ring,Vector3.zero,Vector3.one*.88f,new Color(1,.55f,.12f)).transform;
            label=mesh.Label(transform,"Aviso de tirada",new Vector3(0,.24f,.42f),.046f,new Color(1,.8f,.3f));
        }
        public void Sync(TerrainEffect effect)
        {gameObject.SetActive(effect!=null);if(effect!=null)label.text="! D6 "+effect.threshold+"+";}
        void Update()
        {
            if(view!=null)label.transform.rotation=view.transform.rotation;
            ring.localScale=Vector3.one*(.88f+.045f*Mathf.Sin(Time.unscaledTime*3));
            ring.localPosition=Vector3.up*(.008f+.009f*(1+Mathf.Sin(Time.unscaledTime*3)));
        }
    }
}
