using System;
using UnityEngine;  

public static class GameEvent
{
    public static Action<Gun> OnGunPicked;
    public static Action OnGunDropped;
    public static Action<float> OnPlayerPowerUpCollected;
    public static Action<float> OnTowerPowerUpCollected;
    public static Action<float> OnGunPowerUpCollected;

    public static Action OnEnemyKilled;
    public static Action OnWaveStarted;
    public static Action OnWaveFinished;
}
