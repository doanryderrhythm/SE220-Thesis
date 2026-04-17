using System;
using UnityEngine;  

public static class GameEvent
{
    public static Action<Gun> OnGunPicked;
    public static Action OnGunDropped;
    public static Action<float, float> OnCoinCollected;
}
