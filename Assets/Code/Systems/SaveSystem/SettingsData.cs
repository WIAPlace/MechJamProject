using System;
using System.Collections.Generic;
using UnityEngine;

// Data holder for saved settings

[Serializable]
public class SettingsData
{
    ///////////////////////////////// Volumes
    public List<VolumeSetting> volumes = new List<VolumeSetting>();

    ///////////////////////////////// Control Settings
    public float sensetivityX;
    public float sensetivityY;
    
    ///////////////////////////////// System Settings
    public int[] screenSize;
}

[Serializable] // used for the many diffrent volumes
public class VolumeSetting
{
    public string volumeID; // Master, Music, SFX, etc
    public float level; 
}


