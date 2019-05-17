using Cinemachine;
using System;

public class VirtualCameraManager : MonoSingleton<VirtualCameraManager>
{
    private CinemachineVirtualCamera[] _virtualCameras;

    public CinemachineVirtualCamera ActiveVirtualCamera { get; private set; }

    protected override void Awake()
    {
        _virtualCameras = GetComponentsInChildren<CinemachineVirtualCamera>(true);

        foreach(CinemachineVirtualCamera virtualCamera in _virtualCameras)
        {
            virtualCamera.gameObject.SetActive(false);
        }

        if (_virtualCameras.Length > 0)
        {
            ActiveVirtualCamera = _virtualCameras[0];
            ActiveVirtualCamera.gameObject.SetActive(true);
        }
    }

    public void ChangeVirtualCamera(CinemachineVirtualCamera virtualCamera)
    {
        if (virtualCamera != ActiveVirtualCamera && Array.IndexOf(_virtualCameras, virtualCamera) != -1)
        {
            ActiveVirtualCamera.gameObject.SetActive(false);

            ActiveVirtualCamera = virtualCamera;
            ActiveVirtualCamera.gameObject.SetActive(true);
        }
    }
}
