using UnityEngine;

public abstract class KnockedBackRespawner : MonoBehaviour, IFadeImageSubscriber
{
    [Header("Fade out")]
    [SerializeField]
    protected FadeImage Fade;
    [SerializeField]
    protected float FadeDuration = .25f;

    [Header("Knock Back")]
    [SerializeField]
    private float _knockBackStrength = 12.0f;
    [SerializeField]
    private float _knockBackDuration = .3f;

    private PlayerController _playerController;
    private PlatformerMovement _platformerMovement;

    protected virtual void Start()
    {
        Fade.Subscribe(this);
    }

    protected private void OnTouchSpike(Collider2D col)
    {
        if (col.CompareTag(GameManager.PlayerTag))
        {
            Vector2 knockBackForce = CalculateKnockedBackDirection(col).normalized * _knockBackStrength;

            if (!_platformerMovement || col.gameObject != _platformerMovement.gameObject)
            {
                _platformerMovement = col.GetComponent<PlatformerMovement>();
            }

            _platformerMovement.KnockBack(knockBackForce, _knockBackDuration);

            if (!_playerController)
            {
                _playerController = col.GetComponent<PlayerController>();

                _playerController.EnableControl(false);

                Fade.FadeOut(FadeDuration);
            }
        }
    }

    protected abstract Vector3 CalculateKnockedBackDirection(Collider2D col);

    // Methods of the IFadeImageSubscriber interface
    public void NotifyFadeInFinished()
    {
        if (_playerController)
        {
            _playerController.EnableControl(true);

            _playerController = null;
            _platformerMovement = null;
        }
    }

    public virtual void NotifyFadeOutFinished()
    {
        if (_playerController)
        {
            _playerController.transform.position = SpawnManager.Instance.SpawnPosition;
            _platformerMovement.EndKnockBack();

            Fade.FadeIn(FadeDuration);
        }
    }
}
