using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]

public class House : MonoBehaviour
{
    [SerializeField]
    private AudioClip _doorOpenSound;
    [SerializeField]
    private AudioClip _doorCloseSound;

    private Animator _animator;
    private AudioSource _audioSource;

    private int IsOpenParamHashId = Animator.StringToHash(IsOpenParamNameString);

    private const string IsOpenParamNameString = "IsOpen";

    private void Awake()
    {
        /*if (!_doorOpenSound)
        {
            Debug.LogError("No door open sound was set for " + GetType() + " script of " + gameObject.name + "!");
        }

        if (!_doorCloseSound)
        {
            Debug.LogError("No door close sound was set for " + GetType() + " script of " + gameObject.name + "!");
        }*/

        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
    }

    public void OpenDoor()
    {
        _animator.SetBool(IsOpenParamHashId, true);

        _audioSource.clip = _doorOpenSound;
        _audioSource.Play();
    }

    public void CLoseDoor()
    {
        _animator.SetBool(IsOpenParamHashId, false);

        _audioSource.clip = _doorCloseSound;
        _audioSource.Play();
    }
}
