using UnityEngine;

public class Respawn : MonoBehaviour
{
    [SerializeField]
    private Transform m_respawnPos;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (m_respawnPos && col.CompareTag(GameManager.PlayerTag))
        {
            col.transform.position = m_respawnPos.position;
        }
    }
}
