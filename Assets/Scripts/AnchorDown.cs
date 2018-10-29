using UnityEngine;

public class AnchorDown : MonoBehaviour
{
    private void OnTriggerStay2D(Collider2D other)
    {
        CharacterTypeAController typeA = other.GetComponent<CharacterTypeAController>();

        if (typeA && typeA.IsGrounded)
        {
            Debug.Log(other.gameObject.name);
            other.GetComponent<Rigidbody2D>().AddForce(Vector2.down * 5.0f, ForceMode2D.Force);
            return;
        }

        CharacterTypeBController typeB = other.GetComponent<CharacterTypeBController>();

        Debug.Log(Time.time + ": " + typeB);
        if (typeB && typeB.IsGrounded)
        {
            typeB.AddVerticalVelocity(Vector2.down * 5.0f);
            return;
        }
    }
}
