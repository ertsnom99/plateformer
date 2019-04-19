using Cinemachine;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(PlatformerMovement))]

public class BouncingCharacterControl : CharacterControl, IBouncingPhysicsObjectSubscriber
{
    [Header("Controls")]
    [SerializeField]
    private bool m_useKeyboard;

    [Header("Charge")]
    [SerializeField]
    private float m_maxChargeTime = 5.0f;
    private bool m_isCharging = false;
    private float m_chargeTime = .0f;

    // TEMP
    [SerializeField]
    private Text m_chargeText;

    [Header("Bounce")]
    [SerializeField]
    private GameObject m_arrow;
    [SerializeField]
    private GameObject m_bounceFormPrefab;
    private GameObject m_bounceForm;
    [SerializeField]
    private CircleCollider2D m_bounceFormSpawnAreaCollider;
    protected ContactFilter2D m_contactFilter;
    [SerializeField]
    private float m_minLaunchStrength = 30.0f;
    [SerializeField]
    private float m_maxLaunchStrength = 70.0f;

    [Header("Sound")]
    [SerializeField]
    private AudioClip m_chargeSound;
    [SerializeField]
    private AudioClip m_launchSound;
    [SerializeField]
    private AudioClip m_returnToNormalFormSound;

    [Header("Camera")]
    [SerializeField]
    private CinemachineVirtualCamera m_virtualCameraNormalForm;
    [SerializeField]
    private CinemachineVirtualCamera m_virtualCameraBounceForm;
    
    private SpriteRenderer m_renderer;
    private BoxCollider2D m_collider;
    private AudioSource m_audioSource;
    private PlatformerMovement m_movementScript;
    private BouncingPhysicsObject m_bouncingPhysicsObjectScript;

    protected override void Awake()
    {
        base.Awake();

        m_arrow.SetActive(false);

        // Tells to use the layer settings from the Physics2D settings (the matrix)
        m_contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        m_contactFilter.useLayerMask = true;
        m_contactFilter.useTriggers = false;

        // Get all component, correctly set them up and check that nothing is missing
        m_renderer = GetComponent<SpriteRenderer>();
        m_collider = GetComponent<BoxCollider2D>();
        m_audioSource = GetComponent<AudioSource>();
        m_movementScript = GetComponent<PlatformerMovement>();

        if (!m_bounceFormPrefab)
        {
            Debug.LogError("No bouncing form gameobject was set!");
        }
        else
        {
            m_bounceForm = Instantiate(m_bounceFormPrefab);
            m_bouncingPhysicsObjectScript = m_bounceForm.GetComponent<BouncingPhysicsObject>();

            if (!m_bouncingPhysicsObjectScript)
            {
                Debug.LogError("No BouncingPhysicsObject script on the bouncing form gameobject!");
            }
            else
            {
                m_bouncingPhysicsObjectScript.Subscribe(this);
            }

            if (!m_virtualCameraBounceForm)
            {
                Debug.LogError("No virtual camera for during bounce was set!");
            }
            else
            {
                m_virtualCameraBounceForm.Follow = m_bounceForm.transform;
            }
        }

        if (!m_virtualCameraNormalForm)
        {
            Debug.LogError("No virtual camera for out of bounce was set!");
        }
        else
        {
            m_virtualCameraNormalForm.Follow = transform;
        }

        if (!m_bounceFormSpawnAreaCollider)
        {
            Debug.LogError("No circle collider to check for enough space for bouncing form was set!");
        }
        else
        {
            m_bounceFormSpawnAreaCollider.enabled = false;
        }

        // TEMP
        m_chargeText.text = "";
    }

    private void Start()
    {
        // TODO: Camera should be set when the player take spossession of this character
        if (!VirtualCameraManager.Instance)
        {
            Debug.LogError("Couldn't find an instance of VirtualCameraManager!");
        }

        ShowNormalForm(false);
    }

    private void Update()
    {
        // Only update when time isn't stop
        if (Time.deltaTime > .0f)
        {
            if (ControlsCharacter())
            {
                // Get the inputs used during this frame
                Inputs inputs = FetchInputs();

                // Check if charge started
                if (m_movementScript.IsGrounded && !m_isCharging && inputs.heldCharge && HasEnoughSpaceForBounceForm())
                {
                    UpdateMovement(new Inputs());

                    m_bounceFormSpawnAreaCollider.enabled = true;
                    m_audioSource.PlayOneShot(m_chargeSound);

                    m_chargeTime = .0f;
                    m_isCharging = true;
                }
                else if (m_isCharging && inputs.releaseCharge)
                {
                    m_bounceFormSpawnAreaCollider.enabled = false;
                    m_arrow.SetActive(false);
                    m_audioSource.Stop();

                    m_isCharging = false;

                    // Only launch of a launch direction is given
                    if (inputs.vertical != .0f || inputs.horizontal != .0f)
                    {
                        ShowBouncingForm(true);
                        LaunchBouncingForm(inputs);
                    }
                    
                    // TEMP
                    m_chargeText.text = "";
                }

                if (!m_isCharging)
                {
                    UpdateMovement(inputs);
                }
                else
                {
                    if (inputs.vertical != .0f || inputs.horizontal != .0f)
                    {
                        m_arrow.SetActive(true);

                        float angle = Mathf.Atan2(inputs.vertical, inputs.horizontal) * Mathf.Rad2Deg;
                        m_arrow.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                    }
                    else
                    {
                        m_arrow.SetActive(false);
                    }

                    if (m_chargeTime < m_maxChargeTime)
                    {
                        m_chargeTime = Mathf.Clamp(m_chargeTime + Time.deltaTime, .0f, m_maxChargeTime);

                        // TEMP
                        m_chargeText.text = Mathf.Lerp(m_minLaunchStrength, m_maxLaunchStrength, m_chargeTime / m_maxChargeTime).ToString();
                    }
                }
            }
        }
    }

    /*void OnDrawGizmos()
    {
        // Draw a semitransparent blue cube at the transforms position
        Gizmos.color = new Color(1, 0, 0, 0.5f);

        Vector2 point = (Vector2)transform.position + m_collider.offset;
        Vector2 size = m_collider.size + new Vector2(2.0f, 2.0f) * m_collider.edgeRadius;

        Gizmos.DrawCube(point, size);
    }*/

    protected override bool ControlsCharacter()
    {
        return base.ControlsCharacter() && m_bouncingPhysicsObjectScript.MovementFrozen;
    }

    private Inputs FetchInputs()
    {
        Inputs inputs = new Inputs();

        if (m_useKeyboard)
        {
            // Inputs from the keyboard
            inputs.vertical = Input.GetAxisRaw("Vertical");
            inputs.horizontal = Input.GetAxisRaw("Horizontal");
            inputs.jump = Input.GetButtonDown("Jump");
            inputs.releaseJump = Input.GetButtonUp("Jump");
            inputs.dash = Input.GetButtonDown("Dash");
            inputs.releaseDash = Input.GetButtonUp("Dash");
            inputs.heldCharge = Input.GetButton("Charge");
            inputs.releaseCharge = Input.GetButtonUp("Charge");
        }
        else
        {
            // TODO: Create inputs specific to the controler
            // Inputs from the controler
            inputs.vertical = Input.GetAxisRaw("Vertical");
            inputs.horizontal = Input.GetAxisRaw("Horizontal");
            inputs.jump = Input.GetButtonDown("Jump");
            inputs.releaseJump = Input.GetButtonUp("Jump");
            inputs.dash = Input.GetButtonDown("Dash");
            inputs.releaseDash = Input.GetButtonUp("Dash");
            inputs.heldCharge = Input.GetButton("Charge");
            inputs.releaseCharge = Input.GetButtonUp("Charge");
        }

        return inputs;
    }

    private bool HasEnoughSpaceForNormalForm()
    {
        Vector2 point = (Vector2)transform.position + m_collider.offset;
        Vector2 size = m_collider.size + new Vector2(2.0f, 2.0f) * m_collider.edgeRadius;

        // Check if there is a collider in the way
        Collider2D[] colliders = Physics2D.OverlapBoxAll(point, size, .0f);
        m_collider.OverlapCollider(m_contactFilter, colliders);

        foreach (Collider2D collider in colliders)
        {
            if (collider.gameObject != m_bounceForm && !collider.isTrigger)
            {
                return false;
            }
        }

        return true;
    }

    private void ShowNormalForm(bool replace)
    {
        // Hide the bouncing form
        m_bounceForm.SetActive(false);

        // Replace and show the current character
        if (replace)
        {
            AlignNormalFormToBouncingForm();
        }

        m_renderer.enabled = true;
        m_collider.enabled = true;
        m_movementScript.enabled = true;

        // Update the virtual camera to use
        VirtualCameraManager.Instance.ChangeVirtualCamera(m_virtualCameraNormalForm);
    }

    private void AlignNormalFormToBouncingForm()
    {
        transform.position = m_bounceForm.transform.position - m_bounceFormSpawnAreaCollider.transform.localPosition - (Vector3)m_bounceFormSpawnAreaCollider.offset;
    }

    private bool HasEnoughSpaceForBounceForm()
    {
        float overlapRadius = Mathf.Max(m_bounceFormSpawnAreaCollider.transform.lossyScale.x, m_bounceFormSpawnAreaCollider.transform.lossyScale.y) * m_bounceFormSpawnAreaCollider.radius;
        Vector2 overlapPoint = (Vector2)m_bounceFormSpawnAreaCollider.transform.position + m_bounceFormSpawnAreaCollider.offset;

        // Check if there is a collider in the way
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(overlapPoint, overlapRadius);

        foreach (Collider2D collider in hitColliders)
        {
            if (collider.gameObject != gameObject && !collider.isTrigger)
            {
                return false;
            }
        }

        return true;
    }

    private void ShowBouncingForm(bool replace)
    {
        // Hide the current character
        m_renderer.enabled = false;
        m_collider.enabled = false;
        m_movementScript.enabled = false;

        // Replace and show the bouncing form
        if (replace)
        {
            AlignBouncingFormToNormalForm();
        }

        m_bounceForm.SetActive(true);

        // Update the virtual camera to use
        VirtualCameraManager.Instance.ChangeVirtualCamera(m_virtualCameraBounceForm);
    }

    private void AlignBouncingFormToNormalForm()
    {
        m_bounceForm.transform.position = (Vector2)m_bounceFormSpawnAreaCollider.transform.position + m_bounceFormSpawnAreaCollider.offset;
    }

    private void LaunchBouncingForm(Inputs inputs)
    {
        if (!m_bounceForm.activeSelf)
        {
            Debug.LogError("Can't launch the bouncing form because it'snt active!!!");
        }
        else
        {
            Vector2 launchDirection = new Vector2(inputs.horizontal, inputs.vertical).normalized;
            Vector2 launchForce = launchDirection * Mathf.Lerp(m_minLaunchStrength, m_maxLaunchStrength, m_chargeTime / m_maxChargeTime);
            m_bouncingPhysicsObjectScript.Launch(launchForce);

            m_audioSource.PlayOneShot(m_launchSound);
        }
    }

    protected override void UpdateMovement(Inputs inputs)
    {
        m_movementScript.SetInputs(inputs);
    }

    public void SetKeyboardUse(bool useKeyboard)
    {
        m_useKeyboard = useKeyboard;
    }

    // Methods of the IBouncingPhysicsObjectSubscriber interface
    public void NotifyBounceStarted() { }

    public void NotifyBounceFinished()
    {
        // Replace the character to make sure that the correct area is tested
        AlignNormalFormToBouncingForm();
        
        if (HasEnoughSpaceForNormalForm())
        {
            ShowNormalForm(true);
            m_audioSource.PlayOneShot(m_returnToNormalFormSound);
        }
        else
        {
            StartCoroutine(WaitForEnoughSpaceForNormalForm());
        }
    }

    private IEnumerator WaitForEnoughSpaceForNormalForm()
    {
        while (true)
        {
            // Replace the character to make sure that the correct area is tested for collision
            AlignNormalFormToBouncingForm();

            if (HasEnoughSpaceForNormalForm())
            {
                ShowNormalForm(true);
                m_audioSource.PlayOneShot(m_returnToNormalFormSound);
                StopAllCoroutines();
            }

            yield return null;
        }
    }
}
