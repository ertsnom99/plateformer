using UnityEngine;
using UnityEngine.UI;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(Text))]

public class CountdownText : MonoBehaviour, IProximityExplodableSubscriber
{
    [Header("Explodable")]
    [SerializeField]
    private Explodable _proximityExplodable;

    private Text _text;

    private void Awake()
    {
        _text = GetComponent<Text>();
        _text.text = "";
    }

    private void Start()
    {
        _proximityExplodable.Subscribe(this);
    }

    // Methods of the IProximityExplodableSubscriber interface
    public void NotifyCountdownStarted(GameObject explodableGameObject, float timeRemaining)
    {
        _text.text = Mathf.Ceil(timeRemaining).ToString();
    }

    public void NotifyCountdownUpdated(GameObject explodableGameObject, float timeRemaining)
    {
        _text.text = Mathf.Ceil(timeRemaining).ToString();
    }

    public void NotifyCountdownFinished(GameObject explodableGameObject) { }

    public void NotifyExploded(GameObject explodableGameObject) { }
}
