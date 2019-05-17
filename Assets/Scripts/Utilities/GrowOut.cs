using UnityEngine;

public class GrowOut : MonoBehaviour
{
    [Header("Pulsation")]
    [SerializeField]
    private float _minScale = 0.1f;
    [SerializeField]
    private float _maxScale = 1.0f;
    private float _scaleDiff;
    [SerializeField]
    private float _growDuration = 0.2f;
    [SerializeField]
    private AnimationCurve _pgrowOutMovement;

    private bool _growing = false;
    private float _startTime;

    private void Awake()
    {
        _scaleDiff = _maxScale - _minScale;
    }

    private void Start()
    {
        ResetScale();
    }

    private void Update()
    {
        if (_growing)
        {
            float progress = (Time.time - _startTime) / _growDuration;

            if (progress > 1.0f)
            {
                progress = 1.0f;
                _growing = false;
            }

            float scale = _minScale + _pgrowOutMovement.Evaluate(progress) * _scaleDiff;
            transform.localScale = new Vector3(scale, scale, 1.0f);
        }
    }

    public void StartGrow()
    {
        ResetScale();

        _startTime = Time.time;
        _growing = true;
    }

    private void ResetScale()
    {
        transform.localScale = new Vector3(_minScale, _minScale, 1.0f);
    }
}
