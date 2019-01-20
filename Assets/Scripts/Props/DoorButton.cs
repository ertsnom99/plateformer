using UnityEngine;

public class DoorButton : Button
{
    [Header("Door")]
    [SerializeField]
    private DoorControlSetting[] m_doorsControl;

    protected override void OnUse()
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
