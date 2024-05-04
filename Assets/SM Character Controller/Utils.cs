using System.Collections;
using UnityEngine;

public enum STATE{
    IDLE = 0,
    WALKING = 0b1,
    RUNNING = 0b10,
    DASHING = 0b100,
    JUMPING = 0b1000,
    CLIMBING = 0b10000,
    SWIMMING = 0b100000
}

