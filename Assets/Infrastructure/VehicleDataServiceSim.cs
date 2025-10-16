public class VehicleDataServiceSim : IVehicleDataService
{
    public float SpeedKph { get; private set; } = 0f;
    public int Rpm { get; private set; } = 800; // minimo
    public int Gear { get; private set; } = 0;  // N

    public void ApplyMode(DriveMode mode)
    {
        
    }

}
