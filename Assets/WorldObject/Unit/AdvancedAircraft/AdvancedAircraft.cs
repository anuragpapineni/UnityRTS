using UnityEngine;
using System.Collections;
using RTS;

public class AdvancedAircraft : Unit
{

    private Quaternion aimRotation;

    // Use this for initialization
    void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
        if (aiming)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, aimRotation, weaponAimSpeed);
            CalculateBounds();
            //sometimes it gets stuck exactly 180 degrees out in the calculation and does nothing, this check fixes that
            Quaternion inverseAimRotation = new Quaternion(-aimRotation.x, -aimRotation.y, -aimRotation.z, -aimRotation.w);
            if (transform.rotation == aimRotation || transform.rotation == inverseAimRotation)
            {
                aiming = false;
            }
        }
    }

    public override bool CanAttack()
    {
        return true;
    }

    protected override void UseWeapon()
    {
        base.UseWeapon();
        Vector3 gun1SpawnPoint = transform.position;
        gun1SpawnPoint.x += (2.5f * transform.forward.x);
        //WARNING: Y does not align perfectly with the barrel, but if you do align it, it flies over other unit's heads and does not count as a hit
        gun1SpawnPoint.y += 1.5f;
        gun1SpawnPoint.z += (1.5f * transform.forward.z);
        GameObject gameObject = (GameObject)Instantiate(ResourceManager.GetWorldObject("AdvancedAircraftProjectile"), gun1SpawnPoint, transform.rotation);
        Projectile projectile = gameObject.GetComponentInChildren<Projectile>();
        projectile.SetRange(0.9f * weaponRange);
        projectile.damage = 20;
        projectile.velocity = 20;
        projectile.SetTarget(target);
    }

    protected override void AimAtTarget()
    {
        base.AimAtTarget();
        aimRotation = Quaternion.LookRotation(target.transform.position - transform.position);
    }
}
