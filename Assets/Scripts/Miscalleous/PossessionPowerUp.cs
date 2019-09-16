using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(AudioSource))]

public class PossessionPowerUp : MonoBehaviour
{
    [Header("Sound")]
    [SerializeField]
    private AudioClip _takenSound;

    private bool _isTaken = false;

    private SpriteRenderer _spriteRenderer;
    private AudioSource _audioSource;

    private void Awake()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _audioSource = GetComponentInChildren<AudioSource>();
    }

    private void Update()
    {
        if (_isTaken && !_audioSource.isPlaying)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag(GameManager.PlayerTag))
        {
            _isTaken = true;
            _audioSource.PlayOneShot(_takenSound);

            _spriteRenderer.enabled = false;

            col.GetComponent<PlayerController>().SetCanUsePossession(true);
        }
    }
}
