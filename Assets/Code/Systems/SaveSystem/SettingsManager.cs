using System.Collections.Generic;
using UnityEngine;

// set up alongside Settomgs UI toolkit script to have a settings menu up and running
// this is seprate so that it it is decoupled from the others and able to work independently.

public static class SettingsManager
{
    //public static SettingsManager Instance {get;private set;}
 
    private static SettingsData settings = new SettingsData();

    ///////////////////////////////////////////////////////////////////////////// Load Settings
    // will want to do this on start up just to make sure they are in there
    public static void LoadSettingsFromJson()
    {
        string loadedData = SaveSystem.SettingsLoad();
        if (loadedData != null)
        {
            settings = JsonUtility.FromJson<SettingsData>(loadedData);
        }
    }

    ///////////////////////////////////////////////////////////////////////////// Save Settings
    /// usefull throught this script
    public static void SaveSettingsToJson()
    {
        if (settings == null) return;

        // convert settings into a json string
        string dataString = JsonUtility.ToJson(settings);
        
        // save settings data
        SaveSystem.SettingsSave(dataString);
    }

    ///////////////////////////////////////////////////////////////////////////// Volume Slider Change 
    public static void OnVolumeChanged(string sliderName, float newlevel)
    {
        // Find the matching VolumeSetting object
        VolumeSetting setting = settings.volumes.Find(v => v.volumeID == sliderName);
        
        if (setting != null)
        {
            setting.level = newlevel;
            
            // after update save
            SaveSettingsToJson();
        }
    }

}

