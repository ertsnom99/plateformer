using UnityEngine;

public class EnableControlTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag(GameManager.PlayerTag))
        {
            col.GetComponent<PlayerCharacter>().Controller.EnableControl(true);
        }
    }
}
