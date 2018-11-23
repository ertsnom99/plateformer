using UnityEngine;
using UnityEngine.UI;

public class GemManager : MonoBehaviour, IGemSubscriber
{
    [Header("Text")]
    [SerializeField]
    private Text m_text;

    private Gem[] m_gems;
    private int m_gemCount = 0;

    private void Start ()
    {
        FindAllGems();
        SubscribeToGems();
        UpdateText();
    }

    private void FindAllGems()
    {
        m_gems = FindObjectsOfType<Gem>();
    }

    private void SubscribeToGems()
    {
        foreach (Gem gem in m_gems)
        {
            gem.Subscribe(this);
        }
    }

    private void UpdateText()
    {
        m_text.text = m_gemCount + "/" + m_gems.Length + " Gems";
    }

    // Methods of the IGemSubscriber interface
    public void NotifyGemCollected(Gem gem)
    {
        m_gemCount++;
        UpdateText();
    }
}
