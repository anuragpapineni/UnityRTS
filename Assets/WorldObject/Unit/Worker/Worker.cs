using UnityEngine;
 
public class Worker : Unit {
 
    public int buildSpeed;
 
    private Building currentProject;
    private bool building = false;
    private float amountBuilt = 0.0f;
 
    /*** Game Engine methods, all can be overridden by subclass ***/
 
    protected override void Start () {
        base.Start();
        actions = new string[] {"Refinery", "WarFactory", "Hangar", "Airfield", "Wall"};
    }
 
    protected override void Update () {
        base.Update();
        if(!moving && !rotating) {
            if(building && currentProject && currentProject.UnderConstruction()) {
                amountBuilt += buildSpeed * Time.deltaTime;
                int amount = Mathf.FloorToInt(amountBuilt);
                if(amount > 0) {
                    amountBuilt -= amount;
                    currentProject.Construct(amount);
                    if(!currentProject.UnderConstruction()) building = false;
                }
            }
        }
    }
 
    /*** Public Methods ***/
 
    public override void SetBuilding (Building project) {
        base.SetBuilding (project);
        currentProject = project;
        Bounds bounds = currentProject.GetSelectionBounds();
        Bounds selfBounds = GetSelectionBounds();
        Vector3[] walls = new Vector3[4];
        walls[0] = new Vector3(bounds.center.x, 0, bounds.min.z - selfBounds.extents.z);
        walls[1] = new Vector3(bounds.center.x, 0, bounds.max.z + selfBounds.extents.z);
        walls[2] = new Vector3(bounds.min.x - selfBounds.extents.x, 0, bounds.center.z);
        walls[3] = new Vector3(bounds.max.x + selfBounds.extents.x, 0, bounds.center.z);
        Vector3 dest = walls[0];
        float shortest = Vector3.Distance(walls[0], transform.position);
        for(int i = 1; i < 4; i++)
        {
            float distance = Vector3.Distance(walls[i], transform.position);
            if(distance < shortest){
                dest = walls[i];
                shortest = distance;
            }
        }
        StartMove(dest, currentProject.gameObject);
        building = true;
    }
 
    public override void PerformAction (string actionToPerform) {
        base.PerformAction (actionToPerform);
        CreateBuilding(actionToPerform);
    }
 
    public override void StartMove(Vector3 destination) {
        base.StartMove(destination);
        amountBuilt = 0.0f;
        building = false;
    }
 
    private void CreateBuilding(string buildingName) {
        Vector3 buildPoint = new Vector3(transform.position.x, transform.position.y, transform.position.z + 10);
        if(player) player.CreateBuilding(buildingName, buildPoint, this, playingArea);
    }

    public override void MouseClick (GameObject hitObject, Vector3 hitPoint, Player controller) {
        bool doBase = true;
        //only handle input if owned by a human player and currently selected
        if(player && player.human && currentlySelected && hitObject && hitObject.name!="Ground") {
            Building building = hitObject.transform.parent.GetComponent< Building >();
            if(building) {
                if(building.UnderConstruction()) {
                    SetBuilding(building);
                    doBase = false;
                }
            }
        }
        if(doBase) base.MouseClick(hitObject, hitPoint, controller);
    }
}