public interface IEventBus
{
    void Publish<T>(T evt);
    void Subscribe<T>(System.Action<T> handler);
    void Unsubscribe<T>(System.Action<T> handler);
}

public interface ITimeService { float DeltaTime { get; } }

public interface IAudioService
{
    void Play(string key, int priority = 1, bool loop = false);
    void Stop(string key);
}

public enum DriveMode { Eco, Comfort, Sport }

public interface IVehicleDataService
{
    float SpeedKph { get; }
    int Rpm { get; }
    int Gear { get; }
    void ApplyMode(DriveMode mode);
}

public interface IConfigService
{
    DriveMode DefaultMode { get; }
}
