using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(AudioSource))]

public class AmbiantManager : KeptMonoSingleton<AmbiantManager>
{
    [Header("Ambiant")]
    [SerializeField]
    private AudioClip m_generalAmbiant;
    [SerializeField]
    private float m_generalAmbiantVolume = .8f;
    [SerializeField]
    private AudioClip m_bossAmbiant;
    [SerializeField]
    private float m_bossAmbiantVolume = .5f;

    private AudioSource m_audioSource;

    protected override void Awake()
    {
        base.Awake();

        m_audioSource = GetComponent<AudioSource>();
        m_audioSource.loop = true;
    }

    private void Start()
    {
        m_audioSource.clip = m_generalAmbiant;
        m_audioSource.volume = m_generalAmbiantVolume;
        m_audioSource.Play();
    }

    public void StartBossAmbiant()
    {
        m_audioSource.clip = m_bossAmbiant;
        m_audioSource.volume = m_bossAmbiantVolume;
        m_audioSource.Play();
    }
}
