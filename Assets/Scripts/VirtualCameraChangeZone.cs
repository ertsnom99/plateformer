﻿using Cinemachine;
using UnityEngine;

public class VirtualCameraChangeZone : MonoBehaviour
{
    [SerializeField]
    private VirtualCameraManager m_virtualCameraManager;
    [SerializeField]
    private CinemachineVirtualCamera m_virtualCamera;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (m_virtualCameraManager && m_virtualCamera && col.CompareTag("Player"))
        {
            m_virtualCameraManager.ChangeVirtualCamera(m_virtualCamera);
        }
    }
}