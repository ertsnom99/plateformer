using Cinemachine;
using UnityEngine;

public class TurretCharacter : PossessablePawn
{
    [Header("Canon")]
    [SerializeField]
    private float _minMagnitudeToAim = .8f;

    private Canon _canonScript;

    [Header("Bullet")]
    [SerializeField]
    private float _bulletVCamOrthographicSize = 8.0f;

    protected override void Awake()
    {
        base.Awake();

        _canonScript = GetComponentInChildren<Canon>();

        SetIsLookingForwardDelegate(IsCanonLookingForward);

#if UNITY_EDITOR
        if (!_canonScript)
        {
            Debug.LogError("No canon script was set for " + GetType() + " script of " + gameObject.name + "!");
        }
#endif
    }

    public override void UpdateWithInputs(Inputs inputs)
    {
        UpdateCanon(inputs);
        base.UpdateWithInputs(inputs);
    }

    private bool IsCanonLookingForward()
    {
        return _canonScript.transform.right.x > .0f;
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
            BulletCharacter bulletCharacter = _canonScript.Shoot().GetComponent<BulletCharacter>();
#if UNITY_EDITOR
            if (!bulletCharacter)
            {
                Debug.LogError("The bullet spawned by " + gameObject.name + " didn't have a BulletController script!");
                return;
            }
#endif
            // Transfer control
            Controller tempController = Controller;

            if (IsPossessed)
            {
                InstanciateBulletCamera(bulletCharacter);
                bulletCharacter.Possess(tempController, UnpossessBounds, ContactFilter.layerMask, OnUnpossessCallback, true);
            }
            else if (tempController)
            {
                tempController.SetControlledPawn(bulletCharacter);
            }

            // Release possession of turret
            IsPossessed = false;
            OnUnpossessCallback = null;
            UpdateVisual();
        }
    }

    private void InstanciateBulletCamera(BulletCharacter bulletCharacter)
    {
        CinemachineVirtualCamera bulletVirtualCamera = Instantiate(PossessionVirtualCamera.gameObject, PossessionVirtualCamera.transform.parent).GetComponent<CinemachineVirtualCamera>();
        VirtualCameraManager.Instance.RegisterVirtualCamera(bulletVirtualCamera);

        LensSettings lensSettings = LensSettings.Default;
        lensSettings.OrthographicSize = _bulletVCamOrthographicSize;

        bulletVirtualCamera.Follow = bulletCharacter.gameObject.transform;
        bulletVirtualCamera.m_Lens = lensSettings;

        bulletCharacter.SetPossessionVirtualCamera(bulletVirtualCamera);
    }
}
