using UnityEngine;

public class PossessionPowerUp : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag(GameManager.PlayerTag))
        {
            col.GetComponent<PlayerController>().SetCanUsePossession(true);

            Destroy(gameObject);
        }
    }
}
