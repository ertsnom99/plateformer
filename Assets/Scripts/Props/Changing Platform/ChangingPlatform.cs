using System.Collections;
using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(AudioSource))]

public class ChangingPlatform : MonoBehaviour
{
    enum PlatformType { Appearing, Disappearing };

    [Header("Change")]
    [SerializeField]
    private PlatformType _type;

    [Header("Animated Blocks")]
    [SerializeField]
    private Animator[] _blocks;
    [SerializeField]
    private float _blockDuration = .2f;

    private IEnumerator _coroutine = null;
    private int _blockIndex = 0;

    public bool HasFinishChanging { get; private set; }

    [Header("Sound")]
    [SerializeField]
    private AudioClip _changingSound;

    private AudioSource _audioSource;

    protected int IsVisibleParamHashId = Animator.StringToHash(IsVisibleParamNameString);

    public const string IsVisibleParamNameString = "IsVisible";

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();

        ResetPlatform();
    }
    
    private IEnumerator ChangeBlocks()
    {
        while(_blockIndex < _blocks.Length)
        {
            yield return new WaitForSeconds(_blockDuration);

            _blocks[_blockIndex].SetBool(IsVisibleParamHashId, _type == PlatformType.Appearing);

            _audioSource.PlayOneShot(_changingSound);

            _blockIndex++;
        }
        
        _coroutine = null;

        HasFinishChanging = true;
    }

    public void ResetPlatform()
    {
        StopAllCoroutines();
        _coroutine = null;

        _blockIndex = 0;

        foreach (Animator animator in _blocks)
        {
            animator.SetBool(IsVisibleParamHashId, _type != PlatformType.Appearing);
        }

        HasFinishChanging = false;
    }

    public void StartChanging()
    {
        if(_blocks.Length > 0 && _coroutine == null)
        {
            if (HasFinishChanging)
            {
                ResetPlatform();
            }
            
            _coroutine = ChangeBlocks();
            StartCoroutine(_coroutine);
        }
    }
}
