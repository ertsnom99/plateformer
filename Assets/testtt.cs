using UnityEngine;

public class testtt : MonoBehaviour
{
    [SerializeField]
    protected Collider2D LeftPlayerSpawn;
    protected ContactFilter2D LeftPlayerSpawnContactFilter;

    protected Collider2D[] OverlapResults = new Collider2D[4];

    private void Awake()
    {
        LeftPlayerSpawnContactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(LeftPlayerSpawn.gameObject.layer));
        LeftPlayerSpawnContactFilter.useLayerMask = true;
        LeftPlayerSpawnContactFilter.useTriggers = false;
    }

    private void Update()
    {
        OverlapResults[0] = null;
        LeftPlayerSpawn.OverlapCollider(LeftPlayerSpawnContactFilter, OverlapResults);
        Debug.Log(OverlapResults[0]);
    }
}
