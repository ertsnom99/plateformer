using Cinemachine;
using System;
using System.Collections.Generic;

public class VirtualCameraManager : MonoSingleton<VirtualCameraManager>
{
    private List<CinemachineVirtualCamera> _virtualCameras = new List<CinemachineVirtualCamera>();

    public CinemachineVirtualCamera ActiveVirtualCamera { get; private set; }

    protected override void Awake()
    {
        _virtualCameras.AddRange(GetComponentsInChildren<CinemachineVirtualCamera>(true));

        foreach(CinemachineVirtualCamera virtualCamera in _virtualCameras)
        {
            virtualCamera.gameObject.SetActive(false);
        }

        if (_virtualCameras.Count > 0)
        {
            ActiveVirtualCamera = _virtualCameras[0];
            ActiveVirtualCamera.gameObject.SetActive(true);
        }
    }

    public void RegisterVirtualCamera(CinemachineVirtualCamera virtualCamera)
    {
        if (_virtualCameras.IndexOf(virtualCamera) == -1)
        {
            _virtualCameras.Add(virtualCamera);
            virtualCamera.transform.parent = transform;
        }
    }

    public void RemoveVirtualCamera(CinemachineVirtualCamera virtualCamera)
    {
        if (_virtualCameras.IndexOf(virtualCamera) != -1)
        {
            _virtualCameras.Remove(virtualCamera);
            virtualCamera.transform.parent = null;

            if (_virtualCameras.Count > 0)
            {
                ActiveVirtualCamera = _virtualCameras[0];
                ActiveVirtualCamera.gameObject.SetActive(true);
            }
        }
    }

    public void ChangeVirtualCamera(CinemachineVirtualCamera virtualCamera)
    {
        if (virtualCamera != ActiveVirtualCamera && _virtualCameras.IndexOf(virtualCamera) != -1)
        {
            ActiveVirtualCamera.gameObject.SetActive(false);

            ActiveVirtualCamera = virtualCamera;
            ActiveVirtualCamera.gameObject.SetActive(true);
        }
    }
}
