﻿using System;

[Serializable]
public struct Inputs
{
    public float Vertical;
    public float Horizontal;
    public bool Jump;
    public bool ReleaseJump;
    public bool Dash;
    public bool ReleaseDash;
    public bool Possess;


    public bool HeldCharge;
    public bool ReleaseCharge;
}