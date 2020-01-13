using Cinemachine;
using UnityEngine;

public class TurretController : PossessableCharacterController
{
    [Header("Canon")]
    [SerializeField]
    Canon _canonScript;
    [SerializeField]
    float _minMagnitudeToAim = .8f;

    /*[Header("Bullet")]
    [SerializeField]
    private float _bulletVCamOrthographicSize = 8.0f;*/

    protected override void Awake()
    {
        base.Awake();

        if (!_canonScript)
        {
            Debug.LogError("No canon script was set for " + GetType() + " script of " + gameObject.name + "!");
        }
    }

    protected override void OnUpdatePossessed()
    {
        if (ControlsEnabled())
        {
            UpdateDisplayInfo(PossessingController.CurrentInputs);
            UpdateCanon(PossessingController.CurrentInputs);
            UpdatePossession(PossessingController.CurrentInputs);
        }
        else
        {
            UpdateDisplayInfo(NoControlInputs);
            UpdateCanon(NoControlInputs);
            UpdatePossession(NoControlInputs);
        }
    }

    protected override bool UseLeftSpawn()
    {
        return _canonScript.transform.right.x < .0f;
    }

    protected override void OnUpdateNotPossessed() { }

    protected override Inputs CreateInputs()
    {
        return NoControlInputs;
    }

    private void UpdateCanon(Inputs inputs)
    {
        Vector2 aimingDirection = new Vector2(inputs.Horizontal, inputs.Vertical);

        if (aimingDirection.magnitude >= _minMagnitudeToAim)
        {
            _canonScript.SetAimingDirection(aimingDirection.normalized);
        }

        if (inputs.PressPower)
        {
            GameObject bullet = _canonScript.Shoot();

            BulletController bulletController = bullet.GetComponent<BulletController>();

            if (bulletController)
            {
                CinemachineVirtualCamera bulletVirtualCamera = Instantiate(PossessionVirtualCamera.gameObject, PossessionVirtualCamera.transform.parent).GetComponent<CinemachineVirtualCamera>();
                VirtualCameraManager.Instance.RegisterVirtualCamera(bulletVirtualCamera);

                /*LensSettings lensSettings = LensSettings.Default;
                lensSettings.OrthographicSize = _bulletVCamOrthographicSize;*/

                bulletVirtualCamera.Follow = bullet.transform;
                //bulletVirtualCamera.m_Lens = lensSettings;

                bulletController.SetInfoUI(InfoUI);
                bulletController.SetPossessionVirtualCamera(bulletVirtualCamera);

                TransferPossession(bulletController);
            }
            else
            {
                Debug.LogError("The bullet spawned by " + gameObject.name + " didn't have a BulletController script!");
            }
        }
    }
}
