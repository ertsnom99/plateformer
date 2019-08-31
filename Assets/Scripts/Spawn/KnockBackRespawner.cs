using UnityEngine;

public abstract class KnockBackRespawner : MonoBehaviour, IFadeImageSubscriber
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

    private CharacterController _characterController;
    private BouncingFormCharacterController _bouncingFormCharacterController;
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

            if (!_characterController)
            {
                _characterController = col.GetComponent<CharacterController>();

                _characterController.EnableControl(false);

                Fade.FadeOut(FadeDuration);
            }
        }
        else if (col.CompareTag(GameManager.EnemyTag))
        {
            BouncingFormCharacterController bouncingFormCharacterController = col.GetComponent<BouncingFormCharacterController>();

            if (bouncingFormCharacterController && bouncingFormCharacterController != _bouncingFormCharacterController)
            {
                _bouncingFormCharacterController = bouncingFormCharacterController;
                bouncingFormCharacterController.CancelBounce();
            }
            else if (!bouncingFormCharacterController)
            {
                CharacterController characterController = col.GetComponent<CharacterController>();

                if (characterController)
                {
                    Vector2 knockBackForce = CalculateKnockedBackDirection(col).normalized * _knockBackStrength;

                    if (!_platformerMovement || col.gameObject != _platformerMovement.gameObject)
                    {
                        _platformerMovement = col.GetComponent<PlatformerMovement>();
                    }

                    if (_platformerMovement)
                    {
                        _platformerMovement.KnockBack(knockBackForce, _knockBackDuration);

                        if (!_characterController)
                        {
                            _characterController = characterController;

                            _characterController.EnableControl(false);

                            Fade.FadeOut(FadeDuration);
                        }
                    }
                }
            }
        }
    }

    protected abstract Vector3 CalculateKnockedBackDirection(Collider2D col);

    // Methods of the IFadeImageSubscriber interface
    public void NotifyFadeInFinished()
    {
        if (_characterController)
        {
            _characterController.EnableControl(true);

            _characterController = null;
            _platformerMovement = null;
        }
    }

    public virtual void NotifyFadeOutFinished()
    {
        if (_characterController)
        {
            if (_characterController.CompareTag(GameManager.PlayerTag))
            {
                _characterController.transform.position = SpawnManager.Instance.SpawnPosition;
            }
            else
            {
                _characterController.transform.position = EnemySpawnManager.Instance.GetSpawnPosition(_characterController.gameObject);
            }

            _platformerMovement.EndKnockBack();

            Fade.FadeIn(FadeDuration);
        }
    }
}
