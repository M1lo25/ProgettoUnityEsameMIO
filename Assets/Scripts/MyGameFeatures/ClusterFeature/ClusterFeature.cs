using UnityEngine;

namespace ICXK3
{
    public class ClusterFeature : MonoBehaviour
    {
        [SerializeField] private GameObject clusterHudPrefab;

        private IBroadcaster _bus;
        private IVehicleDataService _vehicle;

        private void Awake()
        {
            _bus = new Broadcaster();
            Locator.Register<IBroadcaster>(_bus);

            _vehicle = new VehicleDataService(_bus);
            Locator.Register<IVehicleDataService>(_vehicle);

            if (clusterHudPrefab) Instantiate(clusterHudPrefab);
        }

        private void Update()
        {
            _vehicle?.SimTick();
        }
    }
}
