using System;

[Serializable]
public struct Inputs
{
    public float vertical;
    public float horizontal;
    public bool jump;
    public bool releaseJump;
    public bool dash;
    public bool releaseDash;
    public bool heldCharge;
    public bool releaseCharge;
}