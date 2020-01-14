using System.Collections;
using UnityEngine;

public class EnterHouseTrigger : MonoBehaviour
{
    [SerializeField]
    private House _house;
    [SerializeField]
    private float _enterDelay = 0.2f;

    private void Awake()
    {
        if (!_house)
        {
            Debug.LogError("No house was set for " + GetType() + " script of " + gameObject.name + "!");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(GameManager.PlayerTag))
        {
            GameObject player = collision.gameObject;
            _house.OpenDoor();

            StartCoroutine(EnterHouse(player));
        }
    }

    private IEnumerator EnterHouse(GameObject player)
    {
        yield return new WaitForSeconds(_enterDelay);

        _house.CLoseDoor();
        player.SetActive(false);

        GameManager.Instance.QuitApplication();
    }
}
