using UnityEngine;

public class Kernel : MonoBehaviour
{
    public void Initialize()
    {
        ServiceRegistry.Reset();

        ServiceRegistry.Register<IEventBus>(new EventBus());
        ServiceRegistry.Register<ITimeService>(new TimeService());
        ServiceRegistry.Register<IAudioService>(new AudioService());           
        ServiceRegistry.Register<IVehicleDataService>(new VehicleDataServiceSim()); 
        ServiceRegistry.Register<IConfigService>(new ConfigService());         

        var uiRootPrefab = Resources.Load<GameObject>("UI/Prefabs/UIRoot");
        if (uiRootPrefab == null)
        {
            Debug.LogError("UIRoot prefab non trovato. Assicurati che sia in Assets/Resources/UI/Prefabs/UIRoot.prefab");
            return;
        }

        var uiRoot = Instantiate(uiRootPrefab);
        DontDestroyOnLoad(uiRoot);

        var hostGo = new GameObject("FeatureHost");
        DontDestroyOnLoad(hostGo);
        var host = hostGo.AddComponent<FeatureHost>();
        host.Boot(uiRoot.transform);
    }
}
