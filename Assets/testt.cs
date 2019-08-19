using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testt : MonoBehaviour
{
    [SerializeField]
    protected Collider2D Collider;
    public Collider2D Trigger;

    [SerializeField]
    protected float ShellRadius = 0.05f;

    protected ContactFilter2D ContactFilter;
    protected RaycastHit2D[] HitBuffer = new RaycastHit2D[16];
    protected List<RaycastHit2D> HitBufferList = new List<RaycastHit2D>(16);

    private void Awake()
    {
        // Tells to use the layer settings from the Physics2D settings (the matrix)
        ContactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        ContactFilter.useLayerMask = true;
        ContactFilter.useTriggers = false;
        ContactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
    }

    private void FixedUpdate()
    {
        HitBuffer = new RaycastHit2D[16];
        int count = Collider.Cast(Vector2.down, ContactFilter, HitBuffer, 0.02f + ShellRadius);

        foreach (RaycastHit2D hit in HitBuffer)
        {
            if (hit.collider)
            {
                Debug.DrawLine(hit.point, hit.point + hit.normal * 100.0f, Color.red);
            }
        }
        
        Collider2D[] triggerColliders = new Collider2D[4];
        Debug.Log(Trigger.OverlapCollider(ContactFilter, triggerColliders));
    }
}
