using System.Collections;
using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(BoxCollider2D))]

public class RetractableSpike : KnockBackRespawner
{
    [Header("Activation")]
    [SerializeField]
    private float _firstActiveDelay = 1.0f;
    [SerializeField]
    private float _activeDuration = 2.0f;
    [SerializeField]
    private float _inactiveDuration = 0.75f;

    private BoxCollider2D _dangerZone;
    private Animator _animator;

    protected int IsRetractedParamHashId = Animator.StringToHash(IsRetractedParamNameString);

    public const string IsRetractedParamNameString = "IsRetracted";

    protected override void Awake()
    {
        base.Awake();

        _dangerZone = GetComponent<BoxCollider2D>();
        _animator = GetComponent<Animator>();
    }

    protected override void Start()
    {
        base.Start();

        _dangerZone.enabled = false;
        _animator.SetBool(IsRetractedParamHashId, true);

        StartCoroutine(ChangeActivation(_firstActiveDelay, true));
    }

    private IEnumerator ChangeActivation(float delay, bool activate)
    {
        yield return new WaitForSeconds(delay);

        _dangerZone.enabled = activate;
        _animator.SetBool(IsRetractedParamHashId, !activate);

        if (activate)
        {
            StartCoroutine(ChangeActivation(_activeDuration, false));
        }
        else
        {
            StartCoroutine(ChangeActivation(_inactiveDuration, true));
        }
    }

    protected override Vector3 CalculateKnockedBackDirection(Collider2D col)
    {
        Vector3 knockedBackDirection = transform.up + (col.transform.position - transform.position);
        knockedBackDirection = new Vector3(Mathf.Sign(knockedBackDirection.x), Mathf.Sign(knockedBackDirection.y), .0f).normalized;

        return knockedBackDirection;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        OnTouchSpike(collision);
    }
}
