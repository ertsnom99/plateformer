using Cinemachine;
using System;
using UnityEngine;

public class VirtualCameraManager : MonoBehaviour
{
    private CinemachineVirtualCamera[] m_virtualCameras;
    private CinemachineVirtualCamera m_activeVirtualCamera;

    private void Awake()
    {
        m_virtualCameras = GetComponentsInChildren<CinemachineVirtualCamera>(true);

        foreach(CinemachineVirtualCamera virtualCamera in m_virtualCameras)
        {
            virtualCamera.gameObject.SetActive(false);
        }

        if (m_virtualCameras.Length > 0)
        {
            m_activeVirtualCamera = m_virtualCameras[0];
            m_activeVirtualCamera.gameObject.SetActive(true);
        }
    }

    public void ChangeVirtualCamera(CinemachineVirtualCamera virtualCamera)
    {
        if (virtualCamera != m_activeVirtualCamera && Array.IndexOf(m_virtualCameras, virtualCamera) != -1)
        {
            m_activeVirtualCamera.gameObject.SetActive(false);

            m_activeVirtualCamera = virtualCamera;
            m_activeVirtualCamera.gameObject.SetActive(true);
        }
    }
}
