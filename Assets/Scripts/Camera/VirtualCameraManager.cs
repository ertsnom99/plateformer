﻿using Cinemachine;
using System;
using UnityEngine;

public class VirtualCameraManager : MonoBehaviour
{
    private CinemachineVirtualCamera[] m_virtualCameras;

    public CinemachineVirtualCamera ActiveVirtualCamera { get; private set; }

    private void Awake()
    {
        m_virtualCameras = GetComponentsInChildren<CinemachineVirtualCamera>(true);

        foreach(CinemachineVirtualCamera virtualCamera in m_virtualCameras)
        {
            virtualCamera.gameObject.SetActive(false);
        }

        if (m_virtualCameras.Length > 0)
        {
            ActiveVirtualCamera = m_virtualCameras[0];
            ActiveVirtualCamera.gameObject.SetActive(true);
        }
    }

    public void ChangeVirtualCamera(CinemachineVirtualCamera virtualCamera)
    {
        if (virtualCamera != ActiveVirtualCamera && Array.IndexOf(m_virtualCameras, virtualCamera) != -1)
        {
            ActiveVirtualCamera.gameObject.SetActive(false);

            ActiveVirtualCamera = virtualCamera;
            ActiveVirtualCamera.gameObject.SetActive(true);
        }
    }
}