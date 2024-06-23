using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "Player/PlayerSettings", order = 1)]
public class PlayerSettings : ScriptableObject
{
    [Header("Generic")]
    public float maxGroundVelocity;
    public float maxAirVelocity;
    public float maxAcceleration;
    public float airDrag;
    public float groundDrag;
    public float airControlAuthority;
    public LayerMask groundMask;

    
    [Header("Jumping")]
    public float jumpForce;
    public float jumpInputBufferTime;
    public float coyoteTime;

    [Header("Sliding")]
    public float slideDrag;

    
    [Header("Wallrun")]
    public float wallrunVerticalDampingFactor;
    public float wallrunJumpRepelForce;
    public float maxWallDistance;

    
    [Header("Camera")]
    public float cameraSensitivity;
    


}
