using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    [Header("Door")]
    [SerializeField]
    private DoorControlSetting[] m_doorsControl;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            for (int i = 0; i < m_doorsControl.Length; i++)
            {
                if (m_doorsControl[i].openState)
                {
                    m_doorsControl[i].doorControlled.Open();
                }
                else
                {
                    m_doorsControl[i].doorControlled.Close();
                }
            }
        }
    }
}
