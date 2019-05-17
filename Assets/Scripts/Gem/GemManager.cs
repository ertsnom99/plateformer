using UnityEngine;
using UnityEngine.UI;

public class GemManager : MonoBehaviour, IGemSubscriber
{
    [Header("Text")]
    [SerializeField]
    private Text _text;

    private Gem[] _gems;
    private int _gemCount = 0;

    private void Start ()
    {
        FindAllGems();
        SubscribeToGems();
        UpdateText();
    }

    private void FindAllGems()
    {
        _gems = FindObjectsOfType<Gem>();
    }

    private void SubscribeToGems()
    {
        foreach (Gem gem in _gems)
        {
            gem.Subscribe(this);
        }
    }

    private void UpdateText()
    {
        _text.text = _gemCount + "/" + _gems.Length + " Gems";
    }

    // Methods of the IGemSubscriber interface
    public void NotifyGemCollected(Gem gem)
    {
        _gemCount++;
        UpdateText();
    }
}
