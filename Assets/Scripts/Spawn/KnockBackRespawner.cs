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

    private Pawn _pawn;
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

            if (!_pawn)
            {
                _pawn = col.GetComponent<Pawn>();
                _pawn.Controller.EnableControl(false);

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
            BouncingFormCharacter bouncingFormCharacter = col.GetComponent<BouncingFormCharacter>();

            if (bouncingFormCharacter)
            {
                bouncingFormCharacter.CancelBounce();
            }
            else if (!bouncingFormCharacter)
            {
                Pawn Enemy = col.GetComponent<Pawn>();

                if (Enemy)
                {
                    Vector2 knockBackForce = CalculateKnockedBackDirection(col).normalized * _knockBackStrength;

                    if (!_platformerMovement || col.gameObject != _platformerMovement.gameObject)
                    {
                        _platformerMovement = col.GetComponent<PlatformerMovement>();
                    }

                    if (_platformerMovement)
                    {
                        _platformerMovement.KnockBack(knockBackForce, _knockBackDuration);

                        if (!_pawn)
                        {
                            _pawn = Enemy;

                            _pawn.Controller.EnableControl(false);

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
        if (_pawn)
        {
            _pawn.Controller.EnableControl(true);

            _pawn = null;
            _platformerMovement = null;
        }
    }

    public virtual void NotifyFadeOutFinished()
    {
        if (_pawn)
        {
            if (_pawn.CompareTag(GameManager.PlayerTag))
            {
                _pawn.transform.position = SpawnManager.Instance.SpawnPosition;
            }
            else
            {
                _pawn.transform.position = EnemySpawnManager.Instance.GetSpawnPosition(_pawn.gameObject);
            }

            _platformerMovement.EndKnockBack();

            Fade.FadeIn(FadeDuration);
        }
    }
}
