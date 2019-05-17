using UnityEngine;
using UnityEngine.UI;

public class FlashingText : MonoBehaviour
{
    [Header("Flash")]
    [SerializeField]
    private float _flashDuration = 1.0f;
    [SerializeField]
    private bool _useStartTimeHasReference;
    private float _startTime;

    private Text _text;

    private void Awake()
    {
        _text = GetComponent<Text>();
    }

    private void Start()
    {
        if (_useStartTimeHasReference)
        {
            _startTime = Time.time;
        }
    }

    private void Update()
    {
        UpdateColor();
    }

    private void UpdateColor()
    {
        float alpha = Mathf.Lerp(0.0f, 1.0f, Mathf.PingPong((Time.time - _startTime) / _flashDuration, 1));
        _text.color = new Color(_text.color.r, _text.color.g, _text.color.b, alpha);
    }

    private void OnEnable()
    {
        if (!_useStartTimeHasReference)
        {
            _startTime = Time.time;
        }
        else
        {
            UpdateColor();
        }
    }

    private void OnDisable()
    {
        _text.color = new Color(_text.color.r, _text.color.g, _text.color.b, 0.0f);
    }
}
