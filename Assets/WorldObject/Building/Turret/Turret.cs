using UnityEngine;
using System.Collections;
using RTS;
using System.Collections.Generic;

public class Turret : Building
{

    private Quaternion aimRotation;
    public float detectionRange = 20.0f;
    protected List<WorldObject> nearbyObjects;
    private float timeSinceLastDecision = 0.0f, timeBetweenDecisions = 0.05f;

    private AudioSource bombSound;

    protected override void Start()
    {
        base.Start();
        bombSound = GetComponent<AudioSource>();
        //detectionRange = weaponRange;
    }

    protected virtual bool ShouldMakeDecision()
    {
        if (!attacking && !movingIntoPosition && !aiming)
        {
            //we are not doing anything at the moment
            if (timeSinceLastDecision > timeBetweenDecisions)
            {
                timeSinceLastDecision = 0.0f;
                return true;
            }
            timeSinceLastDecision += Time.deltaTime;
        }
        return false;
    }

    protected virtual void DecideWhatToDo()
    {
        //determine what should be done by the world object at the current point in time
        Vector3 currentPosition = transform.position;
        nearbyObjects = FindNearbyObjects(currentPosition, detectionRange);
        if (CanAttack())
        {
            List<WorldObject> enemyObjects = new List<WorldObject>();
            foreach (WorldObject nearbyObject in nearbyObjects)
            {
                if (nearbyObject.GetPlayer() != player) enemyObjects.Add(nearbyObject);
            }
            WorldObject closestObject = FindNearestWorldObjectInListToPosition(enemyObjects, currentPosition);
            if (closestObject) BeginAttack(closestObject);
        }
    }

    public static WorldObject FindNearestWorldObjectInListToPosition(List<WorldObject> objects, Vector3 position)
    {
        if (objects == null || objects.Count == 0) return null;
        WorldObject nearestObject = objects[0];
        float distanceToNearestObject = Vector3.Distance(position, nearestObject.transform.position);
        for (int i = 1; i < objects.Count; i++)
        {
            float distanceToObject = Vector3.Distance(position, objects[i].transform.position);
            if (distanceToObject < distanceToNearestObject)
            {
                distanceToNearestObject = distanceToObject;
                nearestObject = objects[i];
            }
        }
        return nearestObject;
    }


    protected override void Update()
    {
        base.Update();
        if (ShouldMakeDecision()) DecideWhatToDo();
        if (aiming)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, aimRotation, weaponAimSpeed);
            CalculateBounds();
            //sometimes it gets stuck exactly 180 degrees out in the calculation and does nothing, this check fixes that
            Quaternion inverseAimRotation = new Quaternion(-aimRotation.x, -aimRotation.y, -aimRotation.z, -aimRotation.w);
            //aiming = false;
            //transform.rotation = aimRotation;
            if (transform.rotation == aimRotation || transform.rotation == inverseAimRotation)
            {
                aiming = false;
            }
        }
    }

    public static List<WorldObject> FindNearbyObjects(Vector3 position, float range)
    {
        Collider[] hitColliders = Physics.OverlapSphere(position, range);
        HashSet<int> nearbyObjectIds = new HashSet<int>();
        List<WorldObject> nearbyObjects = new List<WorldObject>();
        for (int i = 0; i < hitColliders.Length; i++)
        {
            Transform parent = hitColliders[i].transform.parent;
            if (parent)
            {
                WorldObject parentObject = parent.GetComponent<WorldObject>();
                if (parentObject && !nearbyObjects.Contains(parentObject))
                {
                    nearbyObjects.Add(parentObject);
                }
            }
        }
        return nearbyObjects;
    }

    public override bool CanAttack()
    {
        if (UnderConstruction() || hitPoints == 0) return false;
        return true;
    }

    protected override void UseWeapon()
    {
        base.UseWeapon();
        Vector3 spawnPoint = transform.position;
        spawnPoint.x += (2.6f * transform.forward.x);
        spawnPoint.y += 1.0f;
        spawnPoint.z += (2.6f * transform.forward.z);
        GameObject gameObject = (GameObject)Instantiate(ResourceManager.GetWorldObject("TurretProjectile"), spawnPoint, transform.rotation);
        Projectile projectile = gameObject.GetComponentInChildren<Projectile>();
        projectile.SetRange(0.9f * weaponRange);
        projectile.SetTarget(target);

        bombSound.Play();
    }

    protected override void AimAtTarget()
    {
        base.AimAtTarget();
        aimRotation = Quaternion.LookRotation(target.transform.position - transform.position);
    }
}