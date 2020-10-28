using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

public class CuePresets : MonoBehaviour
{
    // This method uses a Preset to copy the serialized values from the source to the target and return true if the copy succeed.
    public static bool CopyObjectSerialization(Object source, Object target)
    {
        Preset preset = new Preset(source);
        return preset.ApplyTo(target);
    }

    // This method creates a Preset from a given Object and save it as an asset in the project.
    public static void CreatePresetAsset(Object source, string name)
    {
        Preset preset = new Preset(source);
        AssetDatabase.CreateAsset(preset, "Assets/Settings/CuePresets" + name + ".preset");
    }
}
