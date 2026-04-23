using UnityEngine;

public class HarmData : MonoBehaviour
{
    [SerializeField] HarmStats harmStats;

    public HarmStats GetHarmStats()
    {
        return harmStats;
    }
}
