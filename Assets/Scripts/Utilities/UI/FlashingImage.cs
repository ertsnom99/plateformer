using UnityEngine;
using UnityEngine.UI;

public class FlashingImage : MonoBehaviour
{
    [Header("Flash")]
    [SerializeField]
    private float _flashDuration = 1.0f;
    [SerializeField]
    private bool _useStartTimeHasReference;
    private float _startTime;

    private Image _image;

    private void Awake()
    {
        _image = GetComponent<Image>();
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
        _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, alpha);
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
        _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, 0.0f);
    }
}
