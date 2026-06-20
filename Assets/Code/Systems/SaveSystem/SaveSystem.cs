using System.IO;
using UnityEngine;

/// <summary>
/// Static Class Tool for uses in saving Json Files
/// </summary>
public static class SaveSystem 
{
    //////////////////////////////////////////////////////////////////////////////// Folders
    /// Save Folder Refrences
    public static readonly string SAVE_FOLDER = Application.dataPath + "/Saves/";
    public static readonly string SETTINGS_FOLDER = Application.dataPath + "/Saves/Settings/";

    //////////////////////////////////////////////////////////////////////////////// Initialize
    /// Initialization, used at the start to make sure path exists
    public static void Init()
    {
        // test if save folder exists
        if (!Directory.Exists(SAVE_FOLDER))
        {
            // create save folder
            Directory.CreateDirectory(SAVE_FOLDER);
        }
        if (!Directory.Exists(SETTINGS_FOLDER))
        {
            // create settings folder
            Directory.CreateDirectory(SETTINGS_FOLDER);
        }
    }
    
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///     SINGLE FILE
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    
    //////////////////////////////////////////////////////////////////////////////// Quick Save
    /// in loader script we will use "string exampleJson = JsonUtility.ToJson('instance of save data class')"
    /// then pass that new string to this function.
    public static void QuickSave(string saveString)
    {   // writes over any file in this path
        File.WriteAllText(SAVE_FOLDER + "/quickSave.txt",saveString);
    }

    //////////////////////////////////////////////////////////////////////////////// Quick Load
    /// in loader script we will use "ClassName example = JsonUtility.FromJson<ClassName>(return string value)" 
    /// to make the return usable as loaded data.
    public static string QuickLoad()
    {
        if (File.Exists(SAVE_FOLDER + "/quickSave.txt"))
        {   // if file exists load it
            string saveSting = File.ReadAllText(SAVE_FOLDER+"/quickSave.txt");
            return saveSting;
        }
        else
        {   // if no file exists return null
            return null;
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///     MULTIPLE FILES
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    //////////////////////////////////////////////////////////////////////////////// New Save
    /// will be used to save to a new file
    public static void Save(string saveString) 
    {
        int saveNumber = 1;
        while (File.Exists("save_" + saveNumber + ".txt"))
        {   // if there is a file that is already under that number 
            // check if there is one in the next number
            saveNumber++;
        }
        // create a new file at at the new save file under the specified number
        File.WriteAllText(SAVE_FOLDER + "/save_"+ saveNumber +".txt",saveString);
    }
    //////////////////////////////////////////////////////////////////////////////// Save Specific
    /// will be used to save to a specific file
    public static void Save(string saveString, int saveNumber) 
    {
        // create a new file or overwrite the file at at the save file under the specified number
        File.WriteAllText(SAVE_FOLDER + "/save_"+ saveNumber +".txt",saveString);
    }


    //////////////////////////////////////////////////////////////////////////////// Load Last
    /// Load the last file that was writen to, thus the most recent
    public static string Load()
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(SAVE_FOLDER);
        FileInfo[] saveFiles = directoryInfo.GetFiles();
        FileInfo mostRecentFile = null;

        foreach(FileInfo fileInfo in saveFiles)
        {  // check what the most recent file is
            if (mostRecentFile == null)
            {   // start off by making the first file this.
                mostRecentFile = fileInfo;
            }
            else
            {
                if (fileInfo.LastWriteTime > mostRecentFile.LastWriteTime)
                {   // if most recent save is older than save being checked, 
                    // the most recent file becomes the one being checked.
                    mostRecentFile = fileInfo;
                }
            }
        }

        if(mostRecentFile != null)
        {   // return the most recent save file if there is any
            string saveSting = File.ReadAllText(SAVE_FOLDER + mostRecentFile.FullName);
            return saveSting;
        }
        else
        {   // if there is no recent file return null
            return null;
        }
    }

    //////////////////////////////////////////////////////////////////////////////// Load Specific
    /// Load a specific file at the stated number, if it exists
    public static string Load(int saveNumber)
    {
        FileInfo specificSave = null;
        string filePath = SAVE_FOLDER + "/save_"+ saveNumber +".txt";

        // if file path exist return the save string data
        if (File.Exists(filePath))
        {
            string saveSting = File.ReadAllText(SAVE_FOLDER + specificSave.FullName);
            return saveSting;
        }
        else 
        {   // if there is no file under that name return null
            return null;
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///     SETTINGS SAVES
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// Usefull if we want our settings to be saved sepratly from the rest of the game data
    
    //////////////////////////////////////////////////////////////////////////////// Save Settings
    /// in loader script we will use "string exampleJson = JsonUtility.ToJson('instance of save data class')"
    /// then pass that new string to this function.
    public static void SettingsSave(string saveString)
    {   // writes over any file in this path
        File.WriteAllText(SETTINGS_FOLDER + "/settings.txt",saveString);
    }

    //////////////////////////////////////////////////////////////////////////////// Load Settings
    /// in loader script we will use "ClassName example = JsonUtility.FromJson<ClassName>(return string value)" 
    /// to make the return usable as loaded data.
    public static string SettingsLoad()
    {
        if (File.Exists(SETTINGS_FOLDER + "/settings.txt"))
        {   // if file exists load it
            string saveSting = File.ReadAllText(SETTINGS_FOLDER+"/settings.txt");
            return saveSting;
        }
        else
        {   // if no file exists return null
            return null;
            /// Possibly look into doing a default settings JSON file 
            /// and outputting that if there is none.
        }
    }

    
}
