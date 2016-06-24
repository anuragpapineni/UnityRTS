using UnityEngine;
using System.Collections;
using RTS;
using System.Collections.Generic;


public class UserInput : MonoBehaviour {
	private Player player;
    private Vector3 marqueeLeft, marqueeRight;

	// Use this for initialization
	void Start () {
        player = transform.root.GetComponent< Player >();
	}
	
	// Update is called once per frame
	void Update () {
		if(player && player.human) {
            if (Input.GetKeyDown(KeyCode.Escape)) OpenPauseMenu();
            if (Input.GetKeyDown(KeyCode.Space) && player.IsFindingBuildingLocation()) { player.RotateBuilding(); }
            MoveCamera();
		    MouseActivity();
		}
	}

    protected virtual void OnGUI()
    {
        if (marqueeLeft != Vector3.zero)
            DrawQuad(new Rect(marqueeLeft.x, Screen.height- marqueeLeft.y, marqueeRight.x - marqueeLeft.x, -marqueeRight.y + marqueeLeft.y), new Color(50, 50, 255));
    }

    float sign(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
    }

    bool PointInTriangle(Vector2 pt, Vector2 v1, Vector2 v2, Vector2 v3)
    {
        bool b1, b2, b3;

        b1 = sign(pt, v1, v2) <= 0.0f;
        b2 = sign(pt, v2, v3) <= 0.0f;
        b3 = sign(pt, v3, v1) <= 0.0f;

        return ((b1 == b2) && (b2 == b3));
    }

    void DrawQuad(Rect position, Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        color.a = 0.2f;
        texture.SetPixel(0, 0, color);
        //texture.alphaIsTransparency = true;
        texture.Apply();
        GUI.skin.box.normal.background = texture;
        GUI.Box(position, GUIContent.none);
    }

    private void OpenPauseMenu()
    {
        Time.timeScale = 0.0f;
        GetComponentInChildren<PauseMenu>().enabled = true;
        GetComponent<UserInput>().enabled = false;
        Cursor.visible = true;
        ResourceManager.MenuOpen = true;
    }

    private void MouseActivity() {
	    if(Input.GetMouseButtonDown(0)) 
	    	LeftMouseClick();
	    else if(Input.GetMouseButtonDown(1)) 
	    	RightMouseClick();
        else if(Input.GetMouseButton(0)&& Input.GetKey(KeyCode.LeftShift) && marqueeLeft!=Vector3.zero)
        {
            marqueeRight = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0) && Input.GetKey(KeyCode.LeftShift) && marqueeLeft != Vector3.zero)
        {
            setSelectedObjects();
            marqueeLeft = Vector3.zero;
            marqueeRight = Vector3.zero;
        }
        else if (Input.GetMouseButtonUp(0)){
            marqueeLeft = Vector3.zero;
            marqueeRight = Vector3.zero;
        }
	    MouseHover();
	}

	private void MoveCamera() {
 		float xpos = Input.mousePosition.x;
		float ypos = Input.mousePosition.y;
		Vector3 movement = new Vector3(0,0,0);

		//horizontal camera movement
		if(xpos >= 0 && xpos < ResourceManager.ScrollWidth) {
		    movement.x -= ResourceManager.ScrollSpeed;
		} else if(xpos <= Screen.width && xpos > Screen.width - ResourceManager.ScrollWidth) {
		    movement.x += ResourceManager.ScrollSpeed;
		}
		 
		//vertical camera movement
		if(ypos >= 0 && ypos < ResourceManager.ScrollWidth) {
		    movement.z -= ResourceManager.ScrollSpeed;
		} else if(ypos <= Screen.height && ypos > Screen.height - ResourceManager.ScrollWidth) {
		    movement.z += ResourceManager.ScrollSpeed;
		}

		//make sure movement is in the direction the camera is pointing
		//but ignore the vertical tilt of the camera to get sensible scrolling
		movement = Camera.main.transform.TransformDirection(movement);
		movement.y = 0;

		//calculate desired camera position based on received input
		Vector3 origin = Camera.main.transform.position;
		Vector3 destination = origin;
		destination.x += movement.x;
		destination.y += movement.y;
		destination.z += movement.z;

		//limit away from ground movement to be between a minimum and maximum distance
		if(destination.y > ResourceManager.MaxCameraHeight) {
		    destination.y = ResourceManager.MaxCameraHeight;
		} else if(destination.y < ResourceManager.MinCameraHeight) {
		    destination.y = ResourceManager.MinCameraHeight;
		}

		//if a change in position is detected perform the necessary update
		if(destination != origin) {
		    Camera.main.transform.position = Vector3.MoveTowards(origin, destination, Time.deltaTime * ResourceManager.ScrollSpeed);
		}
	}

    private void setSelectedObjects()
    {
        if (marqueeLeft != marqueeRight)
        {
            WorldObject[] objects = FindObjectsOfType(typeof(Unit)) as WorldObject[];
            float[] xvals = new float[4];
            float[] yvals = new float[4];
            Vector3 c1 = WorkManager.GroundPoint(marqueeRight);
            Vector3 c2 = WorkManager.GroundPoint(marqueeLeft);
            Vector3 c3 = WorkManager.GroundPoint(new Vector3(marqueeLeft.x, marqueeRight.y, 0));
            Vector3 c4 = WorkManager.GroundPoint(new Vector3(marqueeRight.x, marqueeLeft.y, 0));
            Vector2 v1 = new Vector2(c1.x, c1.z);
            Vector2 v2 = new Vector2(c2.x, c2.z);
            Vector2 v3 = new Vector2(c3.x, c3.z);
            Vector2 v4 = new Vector2(c4.x, c4.z);
            foreach (WorldObject obj in objects)
            {
                Vector2 testpos = new Vector2(obj.transform.position.x, obj.transform.position.z);
                if (PointInTriangle(testpos, v2, v1, v3) || PointInTriangle(testpos, v3, v2, v4))
                {
                    player.SelectedObjects.Add(obj);
                    obj.SetSelection(true, player.hud.GetPlayingArea());
                }
            }
        }
    }
	private void LeftMouseClick() {
		//Deselect();
	    if(player.hud.MouseInBounds()) {
	    	if(player.IsFindingBuildingLocation()) {
				if(player.CanPlaceBuilding()) player.StartConstruction();
			}
            else if (Input.GetKey(KeyCode.LeftShift)){
                Deselect();
                marqueeLeft = Input.mousePosition;
                marqueeRight = Input.mousePosition;
            }
			else{
		        GameObject hitObject = WorkManager.FindHitObject(Input.mousePosition);
		        Vector3 hitPoint = WorkManager.FindHitPoint(Input.mousePosition);
		        if(hitObject && hitPoint != ResourceManager.InvalidPosition) {
                    WorldObject[] temp = player.SelectedObjects.ToArray();
                    foreach (WorldObject obj in temp) {
		            	obj.MouseClick(hitObject, hitPoint, player);
		            }
		            Deselect();
		            if(hitObject.name!="Ground") {
		                WorldObject worldObject = hitObject.transform.parent.GetComponent< WorldObject >();
		                if(worldObject) {
		                    //we already know the player has no selected object
		                    player.SelectedObjects.Add(worldObject);
		                    worldObject.SetSelection(true, player.hud.GetPlayingArea());
		                }
		            }
		            else if (temp.Length>0){
                        player.SelectedObjects = new List<WorldObject>(temp);
                        foreach (WorldObject obj in temp)
                        {
                            if (obj)
                                obj.SetSelection(true, player.hud.GetPlayingArea());
                            else
                                player.SelectedObjects.Remove(obj);
                        }
                    }
		        }
	    	}
	    }
	}

	private void Deselect() {
	    if(player.hud.MouseInBounds() && !Input.GetKey(KeyCode.LeftControl)) {
            foreach (WorldObject obj in player.SelectedObjects)
            {
                obj.SetSelection(false, player.hud.GetPlayingArea());
            }
            player.SelectedObjects.Clear();
        }
	}


	private void MouseHover() {
	    if(player.hud.MouseInBounds()) {
	        if(player.IsFindingBuildingLocation()) {
	            player.FindBuildingLocation();
	        }
    	}
	}


	private void RightMouseClick() {
	    if(player.hud.MouseInBounds() && !Input.GetKey(KeyCode.LeftAlt) && player.SelectedObjects.Count>0) {
	        if(player.IsFindingBuildingLocation()) {
	            player.CancelBuildingPlacement();
	        } else {
                GameObject hitObject = WorkManager.FindHitObject(Input.mousePosition);
                if (hitObject.name != "Ground" && Input.GetKey(KeyCode.LeftControl))
                {
                    WorldObject worldObject = hitObject.transform.parent.GetComponent<WorldObject>();
                    if (worldObject)
                    {
                        //we already know the player has no selected object
                        player.SelectedObjects.Remove(worldObject);
                        worldObject.SetSelection(false, player.hud.GetPlayingArea());
                    }
                }
                else 
                    Deselect();
	        }
	    }
	}


}
