using UnityEngine;

[CreateAssetMenu(fileName = "LevelListener", menuName = "Level Listener")]
public class LevelListener : ScriptableObject
{
    [SerializeField] LevelData[] levels;

    public LevelData GetLevel(int index)
    {
        if (index < 0 || index >= levels.Length)
            return null;

        return levels[index];
    }
}
