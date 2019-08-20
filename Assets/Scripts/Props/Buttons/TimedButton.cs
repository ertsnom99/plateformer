using System.Collections;
using UnityEngine;

public class TimedButton : Button
{
    [Header("Unpress")]
    [SerializeField]
    private float _unpresseDelay = 2.0f;
    [SerializeField]
    private Collider2D _pressArea;

    private ContactFilter2D _contactFilter;
    private Collider2D[] _overlapResults = new Collider2D[4];

    private bool _unpressAfterPress = true;

    protected override void Awake()
    {
        base.Awake();

        _contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        _contactFilter.useLayerMask = true;
        _contactFilter.useTriggers = false;

        if (_unpressAfterPress && IsPressed)
        {
            StartCoroutine(WaitToUnpress());
        }
    }

    protected override void Press()
    {
        base.Press();

        if (_unpressAfterPress)
        {
            StartCoroutine(WaitToUnpress());
        }
    }

    private IEnumerator WaitToUnpress()
    {
        yield return new WaitForSeconds(_unpresseDelay);

        bool stillPresssing;

        do
        {
            stillPresssing = false;

            _overlapResults = new Collider2D[_overlapResults.Length];
            _pressArea.OverlapCollider(_contactFilter, _overlapResults);

            foreach (Collider2D collider in _overlapResults)
            {
                if (collider && CanPressButton(collider))
                {
                    stillPresssing = true;
                }
            }
            
            if (stillPresssing)
            {
                yield return 0;
            }
        }
        while (stillPresssing);

        Unpress();
    }

    public void UnpressAfterPress(bool unpress)
    {
        _unpressAfterPress = unpress;

        if (_unpressAfterPress && IsPressed)
        {
            StartCoroutine(WaitToUnpress());
        }
        else if (!_unpressAfterPress && IsPressed)
        {
            StopAllCoroutines();
        }
    }
}
