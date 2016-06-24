using UnityEngine;
using System.Collections;

public class TurretProjectile : Projectile {
    void Update()
    {
        if (HitSomething())
        {
            InflictDamage();
            Destroy(gameObject);
        }
        if (range > 0 && target)
        {
            float positionChange = Time.deltaTime * velocity;
            range -= positionChange;
            transform.forward = target.transform.position - transform.position;
            transform.forward = transform.forward.normalized;
            transform.position += (positionChange * transform.forward);
        }
        else
        {
            Destroy(gameObject);
        }
    }

}