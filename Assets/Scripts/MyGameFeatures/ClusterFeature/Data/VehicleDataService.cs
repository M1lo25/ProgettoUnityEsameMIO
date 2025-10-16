using System;
using UnityEngine;

namespace ICXK3
{
    public interface IVehicleDataService : IService
    {
        float Speed { get; }
        float RPM { get; }
        float Roll { get; }
        Vector2 G { get; }
        void SimTick();
    }

    public struct OnSpeedChanged { public float kmh; public OnSpeedChanged(float s){ kmh = s; } }
    public struct OnRpmChanged   { public float rpm; public OnRpmChanged(float r){ rpm = r; } }
    public struct OnTTCChanged   { public float ttc; public OnTTCChanged(float t){ ttc = t; } }

    public class VehicleDataService : IVehicleDataService
    {
        public float Speed { get; private set; }
        public float RPM   { get; private set; }
        public float Roll  { get; private set; }
        public Vector2 G   { get; private set; }

        private readonly IBroadcaster _bus;

        const float VMax = 185f;
        const float IdleRpm = 1700f;
        const float RpmMax = 5500f;

        const float ThrottleAccel    = 10f;
        const float CoastDecelBase   = 1.5f;
        const float CoastDecelFactor = 0.015f;
        const float BrakeDecel       = 28.5f;

        const float RpmFollow = 6f;
        const float RpmBlipOnThrottle = 300f;

        const float G2MS2 = 9.81f;

        float _prevSpeedMs;
        float _steer;
        const float SteerAccel  = 2.0f;
        const float SteerReturn = 3.0f;
        const float YawRateMax  = 0.9f;   // rad/s @ sterzo pieno

        float _leadSpeedKmh = 10f;
        float _distanceM    = 25f;

        public VehicleDataService(IBroadcaster bus) { _bus = bus; }

        public void SimTick()
        {
            float dt = Time.deltaTime;

            bool throttle = Input.GetKey(KeyCode.UpArrow);
            bool brake    = Input.GetKey(KeyCode.DownArrow);

            if (throttle) Speed += ThrottleAccel * dt;
            else          Speed -= Mathf.Max(CoastDecelBase, CoastDecelFactor * Speed) * dt;

            if (brake)    Speed -= BrakeDecel * dt;

            Speed = Mathf.Clamp(Speed, 0f, VMax);

            float t = Mathf.Clamp01(Speed / VMax);
            float targetRpm = Mathf.Lerp(IdleRpm, RpmMax, t);
            if (throttle) targetRpm += RpmBlipOnThrottle;
            RPM = Mathf.Lerp(RPM, Mathf.Clamp(targetRpm, IdleRpm, RpmMax), dt * RpmFollow);

            if (Input.GetKey(KeyCode.A))      _steer = Mathf.MoveTowards(_steer, -1f, SteerAccel * dt);
            else if (Input.GetKey(KeyCode.D)) _steer = Mathf.MoveTowards(_steer,  1f, SteerAccel * dt);
            else                               _steer = Mathf.MoveTowards(_steer,  0f, SteerReturn * dt);

            float egoMs = Speed / 3.6f;

            float aLong = (egoMs - _prevSpeedMs) / Mathf.Max(1e-4f, dt);
            float gLong = aLong / G2MS2;

            float yawRate = _steer * YawRateMax;
            float aLat = egoMs * yawRate;
            float gLat = aLat / G2MS2;

            G = new Vector2(gLat, gLong);

            float targetRoll = Mathf.Clamp(_steer * 20f, -30f, 30f);
            Roll = Mathf.Lerp(Roll, targetRoll, dt * 5f);

            _bus.Broadcast(new OnSpeedChanged(Speed));
            _bus.Broadcast(new OnRpmChanged(RPM));
            _bus.Broadcast(new OnRollChanged(Roll));
            _bus.Broadcast(new OnGChanged(G));

            if (Input.GetKey(KeyCode.I)) _leadSpeedKmh += 40f * dt;
            if (Input.GetKey(KeyCode.U)) _leadSpeedKmh -= 40f * dt;
            if (Input.GetKey(KeyCode.L)) _distanceM    -= 10f * dt;
            if (Input.GetKey(KeyCode.K)) _distanceM    += 10f * dt;

            _leadSpeedKmh = Mathf.Max(0f, _leadSpeedKmh);
            _distanceM    = Mathf.Clamp(_distanceM, 0.5f, 200f);

            float leadMs = _leadSpeedKmh / 3.6f;
            float rel = Mathf.Max(0f, egoMs - leadMs);
            float ttc = rel > 0f ? _distanceM / rel : 999f;
            _bus.Broadcast(new OnTTCChanged(ttc));

            _prevSpeedMs = egoMs;
        }
    }
}
