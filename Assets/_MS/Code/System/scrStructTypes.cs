using UnityEngine;

[System.Serializable]
public struct SkillStruct
{
    public SkillsEnum skill;
    public bool isOn;
}

[System.Serializable]
public struct LevelStruct
{
    public float highScore;
}

[System.Serializable]
public struct LeaderboardData
{
    public string username;
    public int rank;
    public int score;
    public Texture2D avatar;
    //public Steamworks.CSteamID steamId;
}