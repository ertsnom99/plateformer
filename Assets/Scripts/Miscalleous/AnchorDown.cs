using UnityEngine;

public class AnchorDown : MonoBehaviour
{
    public float m_downForce = 5.0f;

    private void OnTriggerStay2D(Collider2D other)
    {
        CharacterTypeAController typeA = other.GetComponent<CharacterTypeAController>();

        if (typeA && typeA.IsGrounded)
        {
            Debug.Log(other.gameObject.name);
            other.GetComponent<Rigidbody2D>().AddForce(Vector2.down * m_downForce, ForceMode2D.Force);
            return;
        }

        PlatformerMovement typeB = other.GetComponent<PlatformerMovement>();
        
        if (typeB && typeB.IsGrounded)
        {
            typeB.AddVerticalVelocity(-m_downForce);
            return;
        }
    }
}
