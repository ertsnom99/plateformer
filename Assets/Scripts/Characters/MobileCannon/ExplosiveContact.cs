using UnityEngine;

public class ExplosiveContact : Explodable
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Explode();
    }
}
