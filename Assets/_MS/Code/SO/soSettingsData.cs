using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Settings Data", menuName = "Moonlight Studio/Settings Data", order = 0)]
public class soSettingsData : ScriptableObject
{
    public int resHorizontal;
    public int resVertical;
    public float sensitivity;
    public int currentQualityLevel;
    public int invertVerticalAimVal;
    public float currentMusicVolume;
    public float currentSfxVolume;
    public int suddenStopVal;
    public int screenModeId;
    public int vSyncVal;
    public int aaVal;
}
