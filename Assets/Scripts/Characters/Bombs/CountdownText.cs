using UnityEngine;
using UnityEngine.UI;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(Text))]

public class CountdownText : MonoBehaviour, IProximityExplodableSubscriber
{
    [Header("Explodable")]
    [SerializeField]
    private ProximityExplodable m_proximityExplodable;

    private Text m_text;

    private void Awake()
    {
        m_text = GetComponent<Text>();
        m_text.text = "";
    }

    private void Start()
    {
        m_proximityExplodable.Subscribe(this);
    }

    // Methods of the IProximityExplodableSubscriber interface
    public void NotifyCountdownStarted(float timeRemaining)
    {
        m_text.text = Mathf.Ceil(timeRemaining).ToString();
    }

    public void NotifyCountdownUpdated(float timeRemaining)
    {
        m_text.text = Mathf.Ceil(timeRemaining).ToString();
    }

    public void NotifyCountdownFinished() { }
}
