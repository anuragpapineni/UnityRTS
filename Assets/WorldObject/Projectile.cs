using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour
{

    public float velocity = 1;
    public int damage = 1;

    public float range = 1;
    public WorldObject target;

    void Update()
    {
        if (HitSomething())
        {
            InflictDamage();
            Destroy(gameObject);
        }
        if (range > 0)
        {
            float positionChange = Time.deltaTime * velocity;
            range -= positionChange;
            transform.position += (positionChange * transform.forward);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetRange(float range)
    {
        this.range = range;
    }

    public void SetTarget(WorldObject target)
    {
        this.target = target;
    }

    public bool HitSomething()
    {
        if (target && target.GetSelectionBounds().Contains(transform.position)) return true;
        return false;
    }

    public void InflictDamage()
    {
        if (target) target.TakeDamage(damage);
    }
}