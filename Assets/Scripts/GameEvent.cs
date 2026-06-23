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

    public static Action OnPaused;
    public static Action OnRetry;
    public static Action OnRetire;

    public static Action OnGameLost;

    public static Action OnCloseBuildMenu;
    public static Action<PlacementPoint> OnOpenBuildMenu;
}
