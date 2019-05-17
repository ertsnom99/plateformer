using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [SerializeField]
    private Transform _followTarget;

    [SerializeField]
    private float _XParallaxFactor;
    [SerializeField]
    private float _YParallaxFactor;

    private Vector3 _targetPreviousPos;

    private void Awake()
    {
        if (_followTarget)
        {
            _targetPreviousPos = _followTarget.position;
        }
    }

    private void Update()
    {
        if (_followTarget)
        {
            Vector3 movement = _followTarget.position - _targetPreviousPos;
            Vector3 parallaxMovement = new Vector3(movement.x * _XParallaxFactor, movement.y * _YParallaxFactor, .0f);
            transform.position += parallaxMovement;
            _targetPreviousPos = _followTarget.position;
        }
    }
}
