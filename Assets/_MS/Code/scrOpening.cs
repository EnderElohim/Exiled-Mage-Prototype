using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
//using MoonlightStudio;

public class scrOpening : MonoBehaviour
{
    private const string SAVEDATASTRING = "SaveData";
    private const string SETTINGSDATASTRING = "SettingData";

    public soPlayerSave currentSave;
    public soPlayerSave defaultSave;
    public soSettingsData settingsData;
    public soSettingsData defaultSettings;

    private void Start()
    {
        HandleJsonStuff();
        SceneManager.LoadScene("Hub");
    }


    private void HandleJsonStuff()
    {
        //if (sessionData.isGameStarted == false)
        //{
        //    sessionData.isGameStarted = true;
        //}


        //if (Utilities.IsHaveScriptableObjectJsonData(SAVEDATASTRING) == false)
        //{
        //    CreateSaveFile();
        //}

        //Utilities.LoadScriptableObjectJsonData(currentSave, SAVEDATASTRING);

        //if (Utilities.IsHaveScriptableObjectJsonData(SETTINGSDATASTRING) == false)
        //{
        //    CreateSettingsFile();
        //}

        //Utilities.LoadScriptableObjectJsonData(settingsData, SETTINGSDATASTRING);

        //PlayerPrefs.SetFloat("currentSoundVolume", settingsData.currentMusicVolume);
    }

    private void CreateSaveFile()
    {
        //Utilities.SaveScriptableObjectJsonData(defaultSave, SAVEDATASTRING);
    }

    private void CreateSettingsFile()
    {
        defaultSettings.resHorizontal = Screen.currentResolution.width;
        defaultSettings.resVertical = Screen.currentResolution.height;
        //Utilities.SaveScriptableObjectJsonData(defaultSettings, SETTINGSDATASTRING);
    }
}
