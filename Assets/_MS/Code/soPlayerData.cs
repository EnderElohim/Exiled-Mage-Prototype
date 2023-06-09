using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Moonlight Studio/Player Data", order = 1)]
public class soPlayerData : ScriptableObject
{
    [Header("Stats")]
    public bool isDebugOn;
    public float smoothInputSpeed = 0.2f;
    public int jumpLimit = 1;
    public float baseSpeed = 25;// = 26
    public float dashWalkingSpeed = 40;
    public float wallRunSpeedMultiplier = 1.25f;
    public float jumpSpeed = 20f;
    public float maximumFallSpeed = -50;
    public float maximumStompFallSpeed = -100;
    public float gravity = 45.0f;
    public float lookSpeed = 2.0f;
    public Vector2 lookXLimit = new Vector2(85, -65);
    public float heightLimit = 1.25f;
    public float wallDetectRange = 2;
    public float groundDetectRange = 2.5f;
    public float interactionRange = 100;
    public float airSpeedMultipliar = 1.45f;
    public LayerMask interactionLayer;
    public LayerMask wallLayer;
    public float respawnSpeed = 1f;
    public float respawnCamShakeDuration = 0.1f;
    public float respawnCamShakePower = 20;
    public float maxHeight = 2.2f;
    public float minHeight = 1f;
    public float defaultFov = 75;
    public bool flyingAvalible;


    [Header("Hookshot")]
    public float momentumExtraSpeed = 8;
    public float maxHookRange = 65;
    public float hookshotSpeedMultiplier = 10;
    public float hookshotSpeedMin = 40;
    public float hookshotSpeedMax = 100;
    public float hookshotStopDistance = 3;
    public float hookshotCooldown = 0.7f;
    public LayerMask hookShotLayer;
    public LayerMask hookShotErrorLayer;

    [Header("Dash")]
    public float dashSpeedX;
    public float dashSlowMultiplierX;
    public float dashDuration = 1;
    public float dashCooldown = 1;
    public float dashFovToInDuration;
    public float dashFovToOffDuration;
    public float dashFovDuration;
    public float dashFov = 100;

    [Header("Skills")]
    public SkillStruct[] skills;
}
