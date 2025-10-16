using UnityEngine;

public class FeatureHost : MonoBehaviour
{
    Transform _content;

    public void Boot(Transform uiRoot)
    {
        var contentTr = uiRoot.Find("Content");
        if (contentTr == null)
        {
            Debug.LogError("In UIRoot non esiste un child 'Content'. Apri UIRoot.prefab e aggiungilo.");
            return;
        }
        _content = contentTr;

        SafeSpawn("SpeedPanel");
        SafeSpawn("RpmPanel");
        SafeSpawn("InclinoPanel");
        SafeSpawn("GMeterPanel");
        SafeSpawn("FCWPanel");
        SafeSpawn("PanelMode");
        SafeSpawn("ArrowGear");

        ServiceRegistry.Resolve<IAudioService>().Play("welcome", priority: 2);
    }

    void SafeSpawn(string prefabName)
    {
        var go = Resources.Load<GameObject>($"UI/Prefabs/{prefabName}");
        if (go == null)
        {
            Debug.LogWarning($"Prefab '{prefabName}' non trovato in Resources/UI/Prefabs. Saltato.");
            return;
        }
        var instance = Instantiate(go, _content);
        instance.name = prefabName; 
    }
}
