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

    private Character _character;
    private PlatformerMovement _platformerMovement;

    protected virtual void Awake()
    {
        if (!Fade)
        {
            Debug.LogError("No fade was set for " + GetType() + " script of " + gameObject.name + "!");
        }
    }

    protected virtual void Start()
    {
        Fade.Subscribe(this);
    }

    protected virtual void OnTouchSpike(Collider2D col)
    {
        if (col.CompareTag(GameManager.PlayerTag))
        {
            Vector2 knockBackForce = CalculateKnockedBackDirection(col).normalized * _knockBackStrength;

            if (!_platformerMovement || col.gameObject != _platformerMovement.gameObject)
            {
                _platformerMovement = col.GetComponent<PlatformerMovement>();
            }

            _platformerMovement.KnockBack(knockBackForce, _knockBackDuration);

            if (!_character)
            {
                _character = col.GetComponent<Character>();
                _character.Controller.EnableControl(false);

                if (!Fade.IsFading)
                {
                    Fade.FadeOut(FadeDuration);
                }
            }

            CharacterHealth characterHealth = col.GetComponent<CharacterHealth>();
            characterHealth.Damage(1);
        }
        else if (col.CompareTag(GameManager.EnemyTag))
        {
            BouncingFormCharacterController bouncingFormCharacterController = col.GetComponent<BouncingFormCharacterController>();

            if (bouncingFormCharacterController)
            {
                bouncingFormCharacterController.CancelBounce();
            }
            else if (!bouncingFormCharacterController)
            {
                Character character = col.GetComponent<Character>();

                if (character)
                {
                    Vector2 knockBackForce = CalculateKnockedBackDirection(col).normalized * _knockBackStrength;

                    if (!_platformerMovement || col.gameObject != _platformerMovement.gameObject)
                    {
                        _platformerMovement = col.GetComponent<PlatformerMovement>();
                    }

                    if (_platformerMovement)
                    {
                        _platformerMovement.KnockBack(knockBackForce, _knockBackDuration);

                        if (!_character)
                        {
                            _character = character;

                            _character.Controller.EnableControl(false);

                            if (!Fade.IsFading)
                            {
                                Fade.FadeOut(FadeDuration);
                            }
                        }
                    }
                }
            }

            CharacterHealth characterHealth = col.GetComponent<CharacterHealth>();

            if (characterHealth)
            {
                characterHealth.Damage(1);
            }
        }
    }

    protected abstract Vector3 CalculateKnockedBackDirection(Collider2D col);

    // Methods of the IFadeImageSubscriber interface
    public void NotifyFadeInFinished()
    {
        if (_character)
        {
            _character.Controller.EnableControl(true);

            _character = null;
            _platformerMovement = null;
        }
    }

    public virtual void NotifyFadeOutFinished()
    {
        if (_character)
        {
            if (_character.CompareTag(GameManager.PlayerTag))
            {
                _character.transform.position = SpawnManager.Instance.SpawnPosition;
            }
            else
            {
                _character.transform.position = EnemySpawnManager.Instance.GetSpawnPosition(_character.gameObject);
            }

            _platformerMovement.EndKnockBack();

            Fade.FadeIn(FadeDuration);
        }
    }
}
