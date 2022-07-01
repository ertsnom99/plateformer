using UnityEngine;

public class FlyingSkullMovement : FlyingMovement
{
    [Header("Animation")]
    [SerializeField]
    private float _minVelocityToRotate = 4.0f;
    [SerializeField]
    private float _rotationSpeed = 10.0f;

    protected override void Animate()
    {
        Vector3 originalPropellantRotation = transform.localRotation.eulerAngles;

        // Flip the sprite if necessary
        if (ShouldFlip())
        {
            Debug.Log("IsLookingForward: " + IsLookingForward());
            Debug.Log("CurrentInputs.Horizontal < .0f: " + (CurrentInputs.Horizontal < .0f));
            SpriteRenderer.flipX = !SpriteRenderer.flipX;
            
            transform.rotation = Quaternion.Euler(originalPropellantRotation.x, originalPropellantRotation.y, -originalPropellantRotation.z);
            originalPropellantRotation = transform.localRotation.eulerAngles;
        }
        
        Vector3 targetAngle = Vector3.up;

        // Adjust the rotation of the propellant
        // The propellant rotate only if their is enough movement
        if (Rigidbody.velocity.magnitude >= _minVelocityToRotate)
        {
            float angle = SpriteRenderer.flipX ? -90.0f : 90.0f;
            targetAngle = Quaternion.Euler(0, 0, angle) * Rigidbody.velocity;
        }

        Quaternion currentPropellantRotation = Quaternion.Euler(originalPropellantRotation);
        Quaternion targetPropellantRotation = Quaternion.LookRotation(Vector3.forward, targetAngle);

        // Rotate the propellant over time
        transform.rotation = Quaternion.Lerp(currentPropellantRotation, targetPropellantRotation, _rotationSpeed * Time.deltaTime);
    }
}
