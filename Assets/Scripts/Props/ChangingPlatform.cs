using System.Collections;
using UnityEngine;

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

    protected int IsVisibleParamHashId = Animator.StringToHash(IsVisibleParamNameString);

    public const string IsVisibleParamNameString = "IsVisible";

    private void Awake()
    {
        ResetPlatform();
    }

    private void Start()
    {
        StartChanging();
    }
    
    private IEnumerator ChangeBlocks()
    {
        while(_blockIndex < _blocks.Length)
        {
            yield return new WaitForSeconds(_blockDuration);

            _blocks[_blockIndex].SetBool(IsVisibleParamHashId, _type == PlatformType.Appearing);

            _blockIndex++;
        }
        
        _coroutine = null;

        HasFinishChanging = true;
    }

    public void ResetPlatform()
    {
        StopAllCoroutines();
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
