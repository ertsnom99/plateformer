using UnityEngine;

public class TurretController : PossessableCharacterController
{
    [Header("Canon")]
    [SerializeField]
    Canon _canonScript;
    [SerializeField]
    float _minMagnitudeToAim = .8f;

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
        Inputs inputs = NoControlInputs;

        if (ControlsEnabled())
        {
            // Get the inputs used during this frame
            inputs = FetchInputs();
        }

        UpdateDisplayInfo(inputs);
        UpdateCanon(inputs);
        UpdatePossession(inputs);
    }

    protected override Inputs FetchInputs()
    {
        Inputs inputs = new Inputs();

        if (UseKeyboard)
        {
            // Inputs from the keyboard
            inputs.Vertical = Input.GetAxisRaw("Vertical");
            inputs.Horizontal = Input.GetAxisRaw("Horizontal");
            inputs.Possess = Input.GetButtonDown("Possess");
            inputs.DisplayInfo = Input.GetButtonDown("DisplayInfo");
            inputs.Power = Input.GetButtonDown("Power");
        }
        else
        {
            // Inputs from the controler
            inputs.Vertical = Input.GetAxisRaw("Vertical");
            inputs.Horizontal = Input.GetAxisRaw("Horizontal");
            inputs.Possess = Input.GetButtonDown("Possess");
            inputs.DisplayInfo = Input.GetButtonDown("DisplayInfo");
            inputs.Power = Input.GetButtonDown("Power");
        }

        return inputs;
    }

    protected override void OnUpdateNotPossessed() { }

    protected override void OnPossess(PossessionPower possessingScript)
    {
        PossessionVirtualCamera.Follow = transform;
    }

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

        if (inputs.Power)
        {
            GameObject bullet = _canonScript.Shoot();

            BulletController bulletController = bullet.GetComponent<BulletController>();

            if (bulletController)
            {
                bulletController.SetInfoUI(InfoUI);
                bulletController.SetPossessionVirtualCamera(PossessionVirtualCamera);

                TransferPossession(bulletController);
            }
            else
            {
                Debug.LogError("The bullet spawned by " + gameObject.name + " didn't have a BulletController script!");
            }
        }
    }
}
