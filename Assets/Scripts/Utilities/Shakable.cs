using System.Collections;
using UnityEngine;

public class Shakable : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField]
    private float _duration = 0.2f;
    [SerializeField]
    private float _frequence = 0.01f;
    [SerializeField]
    private float _strength = 0.8f;
    [SerializeField]
    private float _angle = -4.0f;
    [SerializeField]
    private bool _useRotation = true;
    [SerializeField]
    private Vector3 _usedRotationAxis = new Vector3(1, 0, 1);

    private Vector3 _initPos;
    private float _remainingDuration = 0.0f;
    private Vector3 _direction = new Vector3(0, 0, 0);
    private Vector3 _axis = new Vector3(0, 0, 0);

    protected virtual void Awake()
    {
        _initPos = transform.localPosition;
    }

    protected virtual void Update()
    {
        if (_remainingDuration > 0)
        {
            float shakeProgress = _remainingDuration / _duration;
            
            transform.localPosition = _initPos + new Vector3(_direction.x, _direction.y, 0);

            if (_useRotation)
            {
                transform.localEulerAngles = _axis * _angle * shakeProgress;
            }

            _remainingDuration -= Time.deltaTime;

            if (_remainingDuration < 0.0f)
            {
                _remainingDuration = 0.0f;

                transform.localPosition = _initPos;

                if (_useRotation)
                {
                    transform.localEulerAngles = Vector3.zero;
                }

                StopAllCoroutines();
            }
        }
    }

    public void Shake()
    {
        _remainingDuration = _duration;
        _direction = GenerateShakeDirection();
        _axis = GenerateShakeAxis();
        
        StartCoroutine(UpdateShakePos());
    }

    private IEnumerator UpdateShakePos()
    {
        while (true)
        {
            yield return new WaitForSeconds(_frequence);

            _direction = GenerateShakeDirection();
        }
    }

    private Vector3 GenerateShakeDirection()
    {
        float shakeProgress = _remainingDuration / _duration;
        return Random.insideUnitCircle * _strength * shakeProgress;
    }

    private Vector3 GenerateShakeAxis()
    {
        return new Vector3(Mathf.Sign(Random.Range(0, 2) - 1) * _usedRotationAxis.x, Mathf.Sign(Random.Range(0, 2) - 1) * _usedRotationAxis.y, Mathf.Sign(Random.Range(0, 2) - 1) * _usedRotationAxis.z);
    }
}
