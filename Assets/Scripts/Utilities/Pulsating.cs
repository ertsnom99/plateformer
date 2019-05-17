using UnityEngine;

public class Pulsating : MonoBehaviour
{
    [Header("Pulsation")]
    [SerializeField]
    private float _pulsationScale = 1.1f;
    private Vector3 _initScale;
    [SerializeField]
    private float _pulseDuration = 1.0f;
    [SerializeField]
    private AnimationCurve _pulsationMovement;
    [SerializeField]
    private bool _useStartTimeHasReference;
    private float _startTime;

    private void Awake()
    {
        _initScale = transform.localScale;
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
        UpdateLocalScale();
    }

    private void UpdateLocalScale()
    {
        float scale = _pulsationMovement.Evaluate(Mathf.PingPong((Time.time - _startTime) / (_pulseDuration / 2), 1)) * _pulsationScale;
        transform.localScale = new Vector3(_initScale.x + scale, _initScale.y + scale, _initScale.z + scale);
    }

    private void OnEnable()
    {
        if (!_useStartTimeHasReference)
        {
            _startTime = Time.time;
        }
        else
        {
            UpdateLocalScale();
        }
    }

    private void OnDisable()
    {
        transform.localScale = _initScale;
    }
}
