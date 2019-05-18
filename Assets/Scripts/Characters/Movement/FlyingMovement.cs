using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]

public class FlyingMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    private float _speed = 45.0f;

    [Header("Animation")]
    [SerializeField]
    private float _minVelocityToRotatePropellant = 4.0f;
    [SerializeField]
    private float _propellantRotationSpeed = 10.0f;
    [SerializeField]
    private bool _flipSprite = false;

    [Header("Sprites")]
    [SerializeField]
    private SpriteRenderer _propellantSprite;

    private Inputs _currentInputs;

    private SpriteRenderer _bodySprite;
    private Rigidbody2D _rigidbody;

    private void Awake()
    {
        _bodySprite = GetComponent<SpriteRenderer>();
        _rigidbody = GetComponent<Rigidbody2D>();

        if (!_propellantSprite)
        {
            Debug.LogError("No sprite setted for the propellant!");
        }
    }

    private void FixedUpdate()
    {
        _rigidbody.AddForce(new Vector2(_currentInputs.Horizontal, _currentInputs.Vertical) * _speed - _rigidbody.velocity);

        Animate();
    }

    private void Animate()
    {
        float targetPropellantAngle = .0f;
        Vector3 originalPropellantRotation = _propellantSprite.transform.localRotation.eulerAngles;

        // Flip the sprite if necessary
        bool flipSprite = false;
        
        flipSprite = (_bodySprite.flipX == _flipSprite ? (_rigidbody.velocity.x < -.01f) : (_rigidbody.velocity.x > .01f));
        
        if (flipSprite)
        {
            // Flip body
            _bodySprite.flipX = !_bodySprite.flipX;

            // Flip propellant
            Vector3 originalPropellantPosition = _propellantSprite.transform.localPosition;
            _propellantSprite.transform.localPosition = new Vector3(-originalPropellantPosition.x, originalPropellantPosition.y, originalPropellantPosition.z);
            _propellantSprite.transform.rotation = Quaternion.Euler(originalPropellantRotation.x, originalPropellantRotation.y, -originalPropellantRotation.z);
            originalPropellantRotation = _propellantSprite.transform.localRotation.eulerAngles;
        }
        
        // Adjust the rotation of the propellant
        // The propellant rotate only if their is enough movement
        if (_rigidbody.velocity.magnitude >= _minVelocityToRotatePropellant)
        {
            targetPropellantAngle = Mathf.Sign(-_rigidbody.velocity.x) * Vector2.Angle(-_rigidbody.velocity, Vector2.down);
        }
        
        Quaternion currentPropellantRotation = Quaternion.Euler(originalPropellantRotation);
        Quaternion targetPropellantRotation = Quaternion.Euler(originalPropellantRotation.x, originalPropellantRotation.y, targetPropellantAngle);

        // Rotate the propellant over time
        _propellantSprite.transform.rotation = Quaternion.Lerp(currentPropellantRotation, targetPropellantRotation, _propellantRotationSpeed * Time.deltaTime);
    }

    public void SetInputs(Inputs inputs)
    {
        _currentInputs = inputs;
    }
}
