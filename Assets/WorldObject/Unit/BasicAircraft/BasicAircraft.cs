using UnityEngine;
using System.Collections;
using RTS;

public class BasicAircraft : Unit {

    private Quaternion aimRotation;

    // Use this for initialization
    void Start () {
        base.Start();
	}
	
	// Update is called once per frame
	void Update () {
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
        Vector3 gun2SpawnPoint = gun1SpawnPoint;
        gun1SpawnPoint.x += (.75f * transform.forward.x);
        gun1SpawnPoint.y += 1.0f;
        gun1SpawnPoint.z += (1.5f * transform.forward.z);
        gun2SpawnPoint.x += (2.0f * transform.forward.x);
        gun2SpawnPoint.y += 1.0f;
        gun2SpawnPoint.z += (1.5f * transform.forward.z);
        GameObject gameObject = (GameObject)Instantiate(ResourceManager.GetWorldObject("BasicAircraftProjectile"), gun1SpawnPoint, transform.rotation);
        GameObject gameObject2 = (GameObject)Instantiate(ResourceManager.GetWorldObject("BasicAircraftProjectile"), gun2SpawnPoint, transform.rotation);
        Projectile projectile = gameObject.GetComponentInChildren<Projectile>();
        Projectile projectile2 = gameObject2.GetComponentInChildren<Projectile>();
        projectile.SetRange(0.9f * weaponRange);
        projectile2.SetRange(0.9f * weaponRange);
        projectile.SetTarget(target);
        projectile2.SetTarget(target);
    }

    protected override void AimAtTarget()
    {
        base.AimAtTarget();
        aimRotation = Quaternion.LookRotation(target.transform.position - transform.position);
    }
}
