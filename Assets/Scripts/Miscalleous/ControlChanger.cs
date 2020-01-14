using System.Collections;
using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(BoxCollider2D))]

public class ControlChanger : MonoBehaviour
{
    [Header("Enter")]
    [SerializeField]
    private Inputs _enterForcedControls;

    [Header("After enter")]
    [SerializeField]
    private float _afterEnterDelay = 0.5f;
    [SerializeField]
    private Inputs _afterEnterForcedControls;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(GameManager.PlayerTag))
        {
            PlatformerMovement platformerMovement = collision.GetComponent<PlatformerMovement>();
            platformerMovement.SetInputs(_enterForcedControls);

            StartCoroutine(ReleaseJump(platformerMovement));
            StartCoroutine(AfterEnter(platformerMovement));
        }
    }

    private IEnumerator ReleaseJump(PlatformerMovement platformerMovement)
    {
        yield return new WaitForEndOfFrame();

        Inputs input = _enterForcedControls;
        input.PressJump = false;

        platformerMovement.SetInputs(input);
    }

    private IEnumerator AfterEnter(PlatformerMovement platformerMovement)
    {
        yield return new WaitForSeconds(_afterEnterDelay);

        platformerMovement.SetInputs(_afterEnterForcedControls);
    }
}
