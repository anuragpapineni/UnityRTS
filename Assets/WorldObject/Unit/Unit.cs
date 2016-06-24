using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;
using System;

public class Unit : WorldObject {
    protected bool moving, rotating;

    private Vector3 destination;
    private Vector3 pathpoint;
    private Quaternion targetRotation;
    private GameObject destinationTarget;
    private Stack<Vector3> path = new Stack<Vector3>();
    public static List<float[]>[,] buildings = new List<float[]>[16, 16];
    public static readonly Vector3[] directions = {2*new Vector3(2.5527f, 0, 2.5527f),
                                            2*new Vector3(-2.5527f, 0, -2.5527f),
                                            2*new Vector3(2.5527f, 0, -2.5527f),
                                            2*new Vector3(-2.5527f, 0, 2.5527f),
                                            2*new Vector3(3.0f, 0, 0),
                                            2*new Vector3(-3.0f, 0, 0),
                                            2*new Vector3(0, 0, 3.0f),
                                            2*new Vector3(0, 0, -3.0f)};
    public static readonly Vector3[] directions2 = {new Vector3(2.5527f, 0, 2.5527f),
                                            new Vector3(-2.5527f, 0, -2.5527f),
                                            new Vector3(2.5527f, 0, -2.5527f),
                                            new Vector3(-2.5527f, 0, 2.5527f),
                                            new Vector3(3.0f, 0, 0),
                                            new Vector3(-3.0f, 0, 0),
                                            new Vector3(0, 0, 3.0f),
                                            new Vector3(0, 0, -3.0f)};

    private Vector3 goalDirection;
    public float moveSpeed, rotateSpeed;
    protected AudioSource[] audios;

    /*** Game Engine methods, all can be overridden by subclass ***/

    protected override void Awake() {
        base.Awake();
    }

    protected override void Start() {
        base.Start();
        audios = GetComponents<AudioSource>();
    }

    protected override void Update() {
        base.Update();
        if (moving){
            MakeMove();
            if(!audios[0].isPlaying)
                audios[0].Play();
        }else{
            audios[0].Stop();
        }
    }

    protected override void OnGUI() {
        base.OnGUI();
    }

    public virtual void SetBuilding(Building building) {
        //specific initialization for a unit can be specified here
    }

    public override void MouseClick(GameObject hitObject, Vector3 hitPoint, Player controller) {
        base.MouseClick(hitObject, hitPoint, controller);
        //only handle input if owned by a human player and currently selected
        if (player && player.human && currentlySelected) {
            if (hitObject.name == "Ground" && hitPoint != ResourceManager.InvalidPosition) {
                attacking = false;
                float x = hitPoint.x;
                //makes sure that the unit stays on top of the surface it is on
                float y = hitPoint.y + transform.position.y;
                float z = hitPoint.z;
                Vector3 destination = new Vector3(x, y, z);
                if (isValidPosition(destination))
                    StartMove(destination);
            }
        }
    }

    public virtual void StartMove(Vector3 destination) {
        this.destination = destination;
        goalDirection = (destination - transform.position).normalized * 6.0f;

        if ( idastar() && path.Count>0)
            pathpoint = path.Pop();
        targetRotation = Quaternion.LookRotation(destination - transform.position);
        moving = true;
    }

    public void StartMove(Vector3 destination, GameObject destinationTarget) {
        StartMove(destination);
        this.destinationTarget = destinationTarget;
    }

    private void TurnToTarget() {
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed);

        CalculateBounds();
        //sometimes it gets stuck exactly 180 degrees out in the calculation and does nothing, this check fixes that
        Quaternion inverseTargetRotation = new Quaternion(-targetRotation.x, -targetRotation.y, -targetRotation.z, -targetRotation.w);
        if (transform.rotation == targetRotation || transform.rotation == inverseTargetRotation) {
            rotating = false;
            moving = true;
        }
    }
    public bool idastar()
    {
        path.Clear();
        float bound = Vector3.Distance(transform.position, destination)*1.3f;
        float t = 0;
        for (int i = 0; i<20; i++)
        {
            t = search(transform.position, 0, bound);
            if (t == -1.0f)
            {
                return true;
            }
            else if (float.IsPositiveInfinity(t))
            { 
                return false;
            }
            bound = t;
        }

        Debug.Log("failed to find path");
        return false;
    }

    private float search(Vector3 node, float g, float bound)
    {
        float f = g + Vector3.Distance(node, destination);
        if (f > bound+.01)
            return f;
        if (isGoal(node)) {
            path.Push(node);
            return -1;
        }
        float min = float.PositiveInfinity;
        float t;
        List<Vector3> successors;
        if (bound>20)
            successors = getSuccessors(node);
        else
            successors = getSuccessors2(node);
        Vector3 midpoint;
        foreach (Vector3 successor in successors)
        {
            midpoint = (node + successor) / 2;
            if (isValidPosition(midpoint))
            {
                if (isValidPosition(successor))
                {
                    if (bound>20)
                        t = search(successor, g + 6.0f, bound);
                    else
                        t = search(successor, g + 3.0f, bound);
                    if (t == -1)
                    {
                        path.Push(node);
                        return -1;
                    }
                    if (t < min)
                        min = t;
                }
                else if (isGoal(midpoint))
                {
                    path.Push(midpoint);
                    return -1;
                }
            }
        }
        return min;
    }

    private List<Vector3> getSuccessors(Vector3 node)
    {
        List<Vector3> successors = new List<Vector3>(9);
        successors.Add(node + goalDirection);
        foreach (Vector3 direction in directions)
        {
            successors.Add(node + direction);
        }
        //successors.Add(node + (destination - node).normalized*3.0f);

        return successors;
    }

    private List<Vector3> getSuccessors2(Vector3 node)
    {
        List<Vector3> successors = new List<Vector3>(9);
        successors.Add(node + goalDirection*.5f);
        foreach (Vector3 direction in directions2)
        {
            successors.Add(node + direction);
        }
        //successors.Add(node + (destination - node).normalized*3.0f);

        return successors;
    }
    public bool isGoal(Vector3 pos)
    {
        return Vector3.Distance(pos, destination) < 5.8f;
    }

    public bool isValidPosition(Vector3 pos)
    {
        if (pos.x > 80 || pos.x < -80 || pos.z > 80 || pos.z < -80)
            return false;
        int x = ((int)pos.x)/10+8;
        int z = ((int)pos.z)/10+8;
        foreach (float[] bounds2D in buildings[x, z])
        {
            if (contains2D(bounds2D[0], bounds2D[1], bounds2D[2], bounds2D[3], pos + new Vector3(1.5f, 0, 1.5f))||
                contains2D(bounds2D[0], bounds2D[1], bounds2D[2], bounds2D[3], pos + new Vector3(-1.5f, 0, -1.5f)) ||
                contains2D(bounds2D[0], bounds2D[1], bounds2D[2], bounds2D[3], pos + new Vector3(1.5f, 0, -1.5f)) ||
                contains2D(bounds2D[0], bounds2D[1], bounds2D[2], bounds2D[3], pos + new Vector3(-1.5f, 0, 1.5f)))
                return false;
        }
        return true;
    }

    private bool contains2D(float minx, float maxx, float minz, float maxz, Vector3 position)
    {
        return position.x > minx && position.x < maxx && position.z > minz && position.z < maxz;
    }

    private void MakeMove()
    {
        if (path.Count>0&&pathpoint != Vector3.zero&&transform.position == pathpoint)
            pathpoint = path.Pop();
        Vector3 newposition;
        if (isGoal(transform.position))
        {
            targetRotation = Quaternion.LookRotation(destination - transform.position);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed);
            newposition = Vector3.MoveTowards(transform.position, destination, Time.deltaTime * moveSpeed);
            if (isValidPosition(destination))
                transform.position = newposition;
            else
            {
                moving = false;
                movingIntoPosition = false;

            }
        }
        else if (pathpoint != Vector3.zero)
        {
            if (transform.position!=pathpoint)
                targetRotation = Quaternion.LookRotation(pathpoint - transform.position);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed);
            newposition = Vector3.MoveTowards(transform.position, pathpoint, Time.deltaTime * moveSpeed);
            if (isValidPosition(pathpoint)&& isValidPosition((pathpoint+transform.position)*0.5f))
                transform.position = newposition;
            else
            {
                if (isValidPosition(destination)&& idastar())
                {
                    pathpoint = path.Pop();
                }
                else
                {
                    moving = false;
                    movingIntoPosition = false;

                }
            }
        }
        if (transform.position==destination)
        {
            moving = false;
            movingIntoPosition = false;
        }
        CalculateBounds();
    }

    protected override void UseWeapon(){
        base.UseWeapon();
        if(audios.Length > 1 && audios[1] != null && !audios[1].isPlaying){
            audios[1].Play();
        }
    }
}
