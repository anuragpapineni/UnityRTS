using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;

public class Building : WorldObject {
	public float maxBuildProgress;
	protected Queue< string > buildQueue;
	private float currentBuildProgress = 0.0f;
	private Vector3 spawnPoint;
	protected Vector3 rallyPoint;
	public Texture2D rallyPointImage;
	public Texture2D sellImage;
	private bool needsBuilding = false;
    public bool isDefault = false;
    private float[] bounds2D;

    protected override void Awake() {
        base.Awake();
        buildQueue = new Queue< string >();
	}
    private void SetSpawnPoint()
    {
        float spawnX = selectionBounds.center.x + transform.forward.x * selectionBounds.extents.x + transform.forward.x * 10;
        float spawnZ = selectionBounds.center.z + transform.forward.z + selectionBounds.extents.z + transform.forward.z * 10;
        spawnPoint = new Vector3(spawnX, 0.0f, spawnZ);
        rallyPoint = spawnPoint;
    }       
	 
	protected override void Start () {
	    base.Start();
        if (isDefault)
            addToBuildings();
        SetSpawnPoint();
	}
	 
	protected override void Update () {
	    base.Update();
	    ProcessBuildQueue();
	}
	 
	protected override void OnGUI() {
	    base.OnGUI();
	    if(needsBuilding) DrawBuildProgress();
	}

	protected void CreateUnit(string unitName) {
        int unitcost=ResourceManager.GetUnit(unitName).GetComponent<Unit>().cost;
        if (unitcost < player.GetResource(ResourceType.Money))
        {
            player.AddResource(ResourceType.Money, -unitcost);
            buildQueue.Enqueue(unitName);
        }
	}

	protected void ProcessBuildQueue() {
    	if(buildQueue.Count > 0) {
        	currentBuildProgress += Time.deltaTime * ResourceManager.BuildSpeed;
        	if(currentBuildProgress > maxBuildProgress) {
            	if(player) player.AddUnit(buildQueue.Dequeue(), spawnPoint, rallyPoint, transform.rotation, this);
            		currentBuildProgress = 0.0f;
        	}
    	}
	}

	
	public string[] getBuildQueueValues() {
    	string[] values = new string[buildQueue.Count];
    	int pos=0;
    	foreach(string unit in buildQueue) values[pos++] = unit;
    	return values;
	}
 
	public float getBuildPercentage() {
    	return currentBuildProgress / maxBuildProgress;
	}

	public override void SetSelection(bool selected, Rect playingArea) {
	    base.SetSelection(selected, playingArea);
	    if(player) {
	        RallyPoint flag = player.GetComponentInChildren< RallyPoint >();
	        if(selected) {
	            if(flag && player.human && spawnPoint != ResourceManager.InvalidPosition && rallyPoint != ResourceManager.InvalidPosition && !UnderConstruction()) {
	                flag.transform.localPosition = rallyPoint;
	                flag.transform.forward = transform.forward;
	                flag.Enable();
	            }
	        } else {
	            if(flag && player.human) flag.Disable();
	        }
	    }
	}

    public void SetSelectionNoRally(bool selected, Rect playingArea)
    {
        base.SetSelection(selected, playingArea);
    }

	public virtual bool hasSpawnPoint() {
	    return spawnPoint != ResourceManager.InvalidPosition && rallyPoint != ResourceManager.InvalidPosition;
	}

	public override void MouseClick(GameObject hitObject, Vector3 hitPoint, Player controller) {
	    base.MouseClick(hitObject, hitPoint, controller);
	    //only handle iput if owned by a human player and currently selected
	    if(player && player.human && currentlySelected) {
	        if(hitObject.name == "Ground") {
	            SetRallyPoint(hitPoint);
	        }
	    }
	}

	public void SetRallyPoint(Vector3 position) {
	    rallyPoint = position;
	    if(player && player.human && currentlySelected) {
	        RallyPoint flag = player.GetComponentInChildren< RallyPoint >();
	        if(flag) flag.transform.localPosition = rallyPoint;
	    }
	}

	public void Sell() {
	    if(player) player.AddResource(ResourceType.Money, sellValue);
	    if(currentlySelected) SetSelection(false, playingArea);
	    Destroy(this.gameObject);
	}

    public void addToBuildings()
    {
        bounds2D = new float[4];
        bounds2D[0] = selectionBounds.min.x;
        bounds2D[1] = selectionBounds.max.x;
        bounds2D[2] = selectionBounds.min.z;
        bounds2D[3] = selectionBounds.max.z;
        int x1 = (int)(bounds2D[0] * .1f) + 8;
        int z1 = (int)(bounds2D[2] * .1f) + 8;
        int x2 = (int)(bounds2D[1] * .1f) + 8;
        int z2 = (int)(bounds2D[3] * .1f) + 8;
        Unit.buildings[x1, z1].Add(bounds2D);
        if (x1 != x2)
        {
            Unit.buildings[x2, z1].Add(bounds2D);
            if (z1 != z2)
                Unit.buildings[x2, z2].Add(bounds2D);

        }
        if (z1 != z2)
            Unit.buildings[x1, z2].Add(bounds2D);
    }

    public void removeFromBuildings()
    {	
    	if(bounds2D !=  null){
	        int x1 = (int)(bounds2D[0] * .1f) + 8;
	        int z1 = (int)(bounds2D[2] * .1f) + 8;
	        int x2 = (int)(bounds2D[1] * .1f) + 8;
	        int z2 = (int)(bounds2D[3] * .1f) + 8;
	        Unit.buildings[x1, z1].Remove(bounds2D);
	        if (x1 != x2)
	        {
	            Unit.buildings[x2, z1].Remove(bounds2D);
	            if (z1 != z2)
	                Unit.buildings[x2, z2].Remove(bounds2D);

	        }
	        if (z1 != z2)
	            Unit.buildings[x1, z2].Remove(bounds2D);
        }
    }
    public void StartConstruction()
    {
            CalculateBounds();
            addToBuildings();
            needsBuilding = true;
            hitPoints = 0;
            player.AddResource(ResourceType.Money, -cost);
    }

    public void OnDestroy()
    {
        base.OnDestroy();
        removeFromBuildings();
    }

	private void DrawBuildProgress() {
	    GUI.skin = ResourceManager.SelectBoxSkin;
	    Rect selectBox = WorkManager.CalculateSelectionBox(selectionBounds, playingArea);
	    //Draw the selection box around the currently selected object, within the bounds of the main draw area
	    GUI.BeginGroup(playingArea);
	    CalculateCurrentHealth(0.5f, 0.99f);
	    DrawHealthBar(selectBox, "Building ...");
	    GUI.EndGroup();
	}

	public bool UnderConstruction() {
	    return needsBuilding;
	}
	 
	public void Construct(int amount) {
	    hitPoints += amount;
	    if(hitPoints >= maxHitPoints) {
	        hitPoints = maxHitPoints;
	        needsBuilding = false;
	        RestoreMaterials();
	        SetTeamColor();
            SetSpawnPoint();
            AudioSource finishedSound = GetComponentInParent<AudioSource>();
            if(finishedSound != null)
            	finishedSound.Play();
        }
	}

    public override bool IsActive { get { return !needsBuilding; } }
}
	