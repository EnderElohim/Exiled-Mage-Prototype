using DigitalRuby.LightningBolt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class soGameData : ScriptableObject
{
    public bool isDemo;
    public List<string> demoChapterEndLevels;
    public List<string> demoChapterEndLevelToNextChapter;
    public string demoLastLevel;
    public int levelOffset;
    public int leaderboardCount;
    public string[] leaderboardStrings;
    public string[] leaderboardCommunityNameStrings;
    public string[] levelComplateStrings;
    public string normalRushLeaderboardString;
    public string chaoticRushLeaderboardString;
    public soPlayerData chaoticRushPlayerData;
    public GameObject cursor;
    public AudioClip[] getHitSounds;
    public LightningBoltScript hookshotRenderer;
    public LayerMask hookShotLayer;
    public LayerMask hookShotErrorLayer;
    public LayerMask interactionLayer;
    public LayerMask wallLayer;
    public LayerMask ceilingLayer;

}