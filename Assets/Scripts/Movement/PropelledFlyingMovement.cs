using UnityEngine;

public class PropelledFlyingMovement : FlyingMovement
{
    [Header("Animation")]
    [SerializeField]
    private float _minVelocityToRotatePropellant = 4.0f;
    [SerializeField]
    private float _propellantRotationSpeed = 10.0f;

    [Header("Sprites")]
    [SerializeField]
    private SpriteRenderer _propellantSprite;

    protected override void Awake()
    {
        base.Awake();

        if (!_propellantSprite)
        {
            Debug.LogError("No sprite setted for the propellant for " + GetType() + " script of " + gameObject.name + "!");
        }
    }

    protected override void Animate()
    {
        Vector3 originalPropellantRotation = _propellantSprite.transform.localRotation.eulerAngles;

        // Flip the sprite if necessary
        if (ShouldFlipSprite())
        {
            SpriteRenderer.flipX = !SpriteRenderer.flipX;

            // Flip propellant
            Vector3 originalPropellantPosition = _propellantSprite.transform.localPosition;
            _propellantSprite.transform.localPosition = new Vector3(-originalPropellantPosition.x, originalPropellantPosition.y, originalPropellantPosition.z);
            _propellantSprite.transform.rotation = Quaternion.Euler(originalPropellantRotation.x, originalPropellantRotation.y, -originalPropellantRotation.z);
            originalPropellantRotation = _propellantSprite.transform.localRotation.eulerAngles;
        }

        //float targetPropellantAngle = .0f;
        Vector3 targetPropellantAngle = Vector3.up;

        // Adjust the rotation of the propellant
        // The propellant rotate only if their is enough movement
        if (Rigidbody.velocity.magnitude >= _minVelocityToRotatePropellant)
        {
            targetPropellantAngle = Rigidbody.velocity;
        }

        Quaternion currentPropellantRotation = Quaternion.Euler(originalPropellantRotation);
        Quaternion targetPropellantRotation = Quaternion.LookRotation(Vector3.forward, targetPropellantAngle);

        // Rotate the propellant over time
        _propellantSprite.transform.rotation = Quaternion.Lerp(currentPropellantRotation, targetPropellantRotation, _propellantRotationSpeed * Time.deltaTime);
    }
}
