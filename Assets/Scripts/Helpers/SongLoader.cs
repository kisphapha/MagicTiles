
using UnityEngine.UI;
using UnityEngine;

public static class SongLoader
{
    public static Song LoadFromJson(string jsonPath)
    {
        // If file is in Resources folder
        TextAsset jsonFile = Resources.Load<TextAsset>(jsonPath);
        if (jsonFile == null)
        {
            Debug.LogError($"Song JSON not found: {jsonPath}");
            return null;
        }

        return JsonUtility.FromJson<Song>(jsonFile.text);
    }
}
