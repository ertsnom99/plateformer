using UnityEngine;

public class BounceControl : MonoBehaviour
{
    [Header("Controls")]
    [SerializeField]
    private bool m_useKeyboard;

    private void Update()
    {
        // Only update when time isn't stop
        if (Time.deltaTime > .0f)
        {
            // Get the inputs used during this frame
            bool use = Input.GetButtonDown("Fire2");
            float vertical = Input.GetAxisRaw("Vertical");
            float horizontal = Input.GetAxisRaw("Horizontal");

            if (use && (vertical != .0f || horizontal != .0f))
            {
                Vector2 force = new Vector2(horizontal, vertical).normalized;
                Debug.Log(force);
            }
        }
    }

    private Inputs FetchInputs()
    {
        Inputs inputs = new Inputs();

        if (m_useKeyboard)
        {
            // Inputs from the keyboard
            inputs.vertical = Input.GetAxisRaw("Vertical");
            inputs.horizontal = Input.GetAxisRaw("Horizontal");
            inputs.jump = Input.GetButtonDown("Jump");
            inputs.releaseJump = Input.GetButtonUp("Jump");
            inputs.dash = Input.GetButtonDown("Dash");
            inputs.releaseDash = Input.GetButtonUp("Dash");
        }
        else
        {
            // TODO: Create inputs specific to the controler
            // Inputs from the controler
            inputs.vertical = Input.GetAxisRaw("Vertical");
            inputs.horizontal = Input.GetAxisRaw("Horizontal");
            inputs.jump = Input.GetButtonDown("Jump");
            inputs.releaseJump = Input.GetButtonUp("Jump");
            inputs.dash = Input.GetButtonDown("Dash");
            inputs.releaseDash = Input.GetButtonUp("Dash");
        }

        return inputs;
    }
}
