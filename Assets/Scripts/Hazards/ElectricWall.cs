using System.Collections;
using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(BoxCollider2D))]

public class ElectricWall : MonoBehaviour
{
    [Header("Activation")]
    [SerializeField]
    private float _firstActiveDelay = 1.0f;
    [SerializeField]
    private float _activeDuration = 2.0f;
    [SerializeField]
    private float _inactiveDuration = 0.75f;

    [Header("Visual")]
    [SerializeField]
    private GameObject _sprite;

    private BoxCollider2D _dangerZone;

    private void Awake()
    {
        _dangerZone = GetComponent<BoxCollider2D>();

        _dangerZone.isTrigger = true;
    }

    private void Start()
    {
        _dangerZone.enabled = false;
        _sprite.SetActive(false);

        StartCoroutine(ChangeActivation(_firstActiveDelay, true));
    }

    private IEnumerator ChangeActivation(float delay, bool activate)
    {
        yield return new WaitForSeconds(delay);

        _dangerZone.enabled = activate;
        _sprite.SetActive(activate);

        if (activate)
        {
            StartCoroutine(ChangeActivation(_activeDuration, false));
        }
        else
        {
            StartCoroutine(ChangeActivation(_inactiveDuration, true));
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag(GameManager.PlayerTag))
        {
            col.transform.position = SpawnManager.Instance.SpawnPosition;
        }
    }
}
