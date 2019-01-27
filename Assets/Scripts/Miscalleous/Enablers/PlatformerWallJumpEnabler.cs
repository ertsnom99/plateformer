﻿using UnityEngine;

public class PlatformerWallJumpEnabler : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    private PlatformerMovement m_movementScript;
    [SerializeField]
    private bool m_enable = true;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag(GameManager.PlayerTag))
        {
            m_movementScript.EnableSlideOfWall(m_enable);
            m_movementScript.EnableWallJump(m_enable);
        }
    }
}