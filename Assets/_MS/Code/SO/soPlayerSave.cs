using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Save Data", menuName = "Moonlight Studio/Save Data", order = 2)]
public class soPlayerSave : ScriptableObject
{
    public int currentLevel;
    public SkillStruct[] skills;
}
