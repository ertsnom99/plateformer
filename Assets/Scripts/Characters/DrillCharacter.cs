using System.Collections;
using UnityEngine;

// This script requires those components and will be added if they aren't already there
[RequireComponent(typeof(PlatformerMovement))]
[RequireComponent(typeof(BurrowMovement))]
[RequireComponent(typeof(AudioSource))]

public class DrillCharacter : PossessablePawn, IPhysicsCollision2DListener
{
    [Header("Burrow")]
    [SerializeField]
    private AnimationCurve _burrowIntransition;
    [SerializeField]
    private AnimationCurve _burrowOuttransition;
    [SerializeField]
    private float _extraRadius;
    [SerializeField]
    private AudioClip _burrowInSound;
    [SerializeField]
    private AudioClip _burrowOutSound;

    private RaycastHit2D[] _hitBuffer = new RaycastHit2D[16];
    private IEnumerator _transitionCoroutine;

    private bool _burrowed = false;
    private Vector2 _burrowNormal = Vector2.zero;

    protected Collider2D _collider;
    private PlatformerMovement _platformerMovementScript;
    private BurrowMovement _burrowMovementScript;
    private AudioSource _audioSource;

    protected int BurrowingParamHashId = Animator.StringToHash(BurrowingParamNameString);
    protected int BurrowXParamHashId = Animator.StringToHash(BurrowXParamNameString);
    protected int BurrowYParamHashId = Animator.StringToHash(BurrowYParamNameString);

    public const string BurrowingParamNameString = "Burrowing";
    public const string BurrowInParamNameString = "BurrowIn";
    public const string BurrowXParamNameString = "BurrowX";
    public const string BurrowYParamNameString = "BurrowY";

    protected override void Awake()
    {
        base.Awake();

        _collider = GetComponent<Collider2D>();
        _platformerMovementScript = GetComponent<PlatformerMovement>();
        _burrowMovementScript = GetComponent<BurrowMovement>();
        _audioSource = GetComponent<AudioSource>();

        if (!Collider)
        {
            Debug.LogError("No collider was set for " + GetType() + " script of " + gameObject.name + "!");
        }

        SetIsLookingForwardDelegate(_platformerMovementScript.IsLookingForward);

        setBurrowed(false);
    }

    public override void UpdateWithInputs(Inputs inputs)
    {
        UpdateMovement(inputs);
        UpdateBurrowing(inputs);

        if (_transitionCoroutine == null && !_burrowed)
        {
            base.UpdateWithInputs(inputs);
        }
    }

    private void UpdateMovement(Inputs inputs)
    {
        if (_burrowed)
        {
            _burrowMovementScript.SetInputs(inputs);
        }
        else
        {
            _platformerMovementScript.SetInputs(inputs);
            _platformerMovementScript.UpdateMovement();
        }
    }

    private void UpdateBurrowing(Inputs inputs)
    {
        // Already in transition or not attmepting to burrow
        if (_transitionCoroutine == null && inputs.PressPower && _burrowNormal != Vector2.zero)
        {
            Burrow();
        }
    }

    private void Burrow()
    {
        float burrowDepth;

        // Burrowing left or right
        if (Mathf.Abs(Vector2.Dot(-_burrowNormal, Vector2.right)) > .9f)
        {
            burrowDepth = _collider.bounds.size.x + _extraRadius;
        }
        // Burrowing down
        else if (Mathf.Abs(Vector2.Dot(-_burrowNormal, Vector2.down)) > .9f)
        {
            burrowDepth = _collider.bounds.size.y + _extraRadius;
        }
        // Do nothing if any other direction
        else
        {
            return;
        }

        // Check for other collider that could be in the way
        int count = Collider.Cast(-_burrowNormal, ContactFilter, _hitBuffer, burrowDepth);

        for (int i = 0; i < count; i++)
        {
            if (!_hitBuffer[i].collider.gameObject.CompareTag(GameManager.BurrowableTag))
            {
                return;
            }
        }

        // Disable control and movement
        _platformerMovementScript.enabled = false;
        _burrowMovementScript.enabled = false;

        // Start burrow transition
        _transitionCoroutine = Transition(_burrowed ? _burrowOuttransition : _burrowIntransition,
                                          - _burrowNormal,
                                          burrowDepth,
                                          !_burrowed,
                                          _burrowed ? _burrowOutSound : _burrowInSound);

        StartCoroutine(_transitionCoroutine);
    }

    private IEnumerator Transition(AnimationCurve curve, Vector2 direction, float depth, bool burrowIn, AudioClip sound)
    {
        Animator.SetBool(BurrowingParamHashId, true);
        Animator.SetInteger(BurrowXParamHashId, (int)direction.x);
        Animator.SetInteger(BurrowYParamHashId, (int)direction.y);

        float elapsedTime = .0f;
        float curveDuration = curve[curve.keys.Length - 1].time;
        Vector2 startPos = transform.position;
        Vector2 endPos = startPos + direction * depth;

        _audioSource.clip = sound;
        _audioSource.Play();

        // Burrow over time and update sound volume
        while (elapsedTime < curveDuration)
        {
            yield return 0;
            elapsedTime += Time.deltaTime;
            transform.position = Vector2.Lerp(startPos, endPos, curve.Evaluate(elapsedTime));
            _audioSource.volume = 1.0f - curve.Evaluate(elapsedTime);
        }

        setBurrowed(burrowIn);

        Animator.SetBool(BurrowingParamHashId, false);

        _transitionCoroutine = null;
        _burrowNormal = Vector2.zero;
    }

    private void setBurrowed(bool isBurrowed)
    {
        _burrowed = isBurrowed;
        _burrowMovementScript.SetMoveDirection(-_burrowNormal);

        _platformerMovementScript.enabled = !_burrowed;
        _burrowMovementScript.enabled = _burrowed;

        gameObject.layer = LayerMask.NameToLayer(isBurrowed ? GameManager.BurrowedLayer : GameManager.DefaultLayer);
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        foreach(ContactPoint2D contact in collision.contacts)
        {
            if (contact.collider.CompareTag(GameManager.BurrowLimitTag))
            {
                _burrowNormal = AdjustNormalForBurrow(contact.normal);
            }
        }
    }

    public void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag(GameManager.BurrowLimitTag))
        {
            _burrowNormal = Vector2.zero;
        }
    }

    // Make sure to have either an horizontal or vertical normal
    private Vector2 AdjustNormalForBurrow(Vector2 normal)
    {
        Vector2 adjustedNormal;

        if (Vector2.Dot(normal, Vector2.left) >= .5f)
        {
            adjustedNormal = Vector2.left;
        }
        else if (Vector2.Dot(normal, Vector2.up) >= .5f)
        {
            adjustedNormal = Vector2.up;
        }
        else if (Vector2.Dot(normal, Vector2.right) >= .5f)
        {
            adjustedNormal = Vector2.right;
        }
        else
        {
            adjustedNormal = Vector2.down;
        }

        return adjustedNormal;
    }

    // Methods of the IPhysicsCollision2DListener interface
    public void OnPhysicsCollision2DEnter(PhysicsCollision2D collision)
    {
        if (collision.GameObject.CompareTag(GameManager.BurrowableTag))
        {
            _burrowNormal = AdjustNormalForBurrow(collision.Normal);
        }
    }

    public void OnPhysicsCollision2DStay(PhysicsCollision2D collision) { }

    public void OnPhysicsCollision2DExit(PhysicsCollision2D collision)
    {
        if (collision.GameObject.CompareTag(GameManager.BurrowableTag))
        {
            _burrowNormal = Vector2.zero;
        }
    }
}
