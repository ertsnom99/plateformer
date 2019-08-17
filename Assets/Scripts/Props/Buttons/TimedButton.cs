using System.Collections;
using UnityEngine;

public class TimedButton : Button
{
    [Header("Unpress")]
    [SerializeField]
    private float _unpresseDelay = 2.0f;

    private bool _unpressAfterPress = true;

    protected override void Awake()
    {
        base.Awake();

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
        
        //if (_unpressAfterPress)
        //{
            Unpress();
        //}
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
