using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "Player/PlayerSettings", order = 1)]
public class PlayerSettings : ScriptableObject
{
    public float maxHorizontalSpeed;
    public float maxAcceleration;
    public float fallDrag;
    public float walkDrag;
    public float cameraSensitivity;
    public float jumpForce;
    public float jumpInputBufferTime;
    public float coyoteTime;
    public float airControlAuthority;
    public LayerMask groundMask;

}
