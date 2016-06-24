using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using RTS;

public class HUD : MonoBehaviour {

	private Dictionary< ResourceType, int > resourceValues, resourceLimits;
	private const int ICON_WIDTH = 32, ICON_HEIGHT = 32, TEXT_WIDTH = 128, TEXT_HEIGHT = 32;
	public Texture2D[] resources;
	private Dictionary< ResourceType, Texture2D > resourceImages;

	public GUISkin resourceSkin, ordersSkin, selectBoxSkin;
	private const int ORDERS_BAR_WIDTH = 150, RESOURCE_BAR_HEIGHT = 40;
	private const int SELECTION_NAME_HEIGHT = 15;
	private Player player;

	private WorldObject lastSelection;
	private float sliderValue;
	public Texture2D buttonHover, buttonClick;

	private const int BUILD_IMAGE_WIDTH = 64, BUILD_IMAGE_HEIGHT = 64;
	private int buildAreaHeight = 0;
	private const int BUTTON_SPACING = 7;
	private const int SCROLL_BAR_WIDTH = 22;
	private const int BUILD_IMAGE_PADDING = 8;
    public float timer = 300.0f;
	public Texture2D buildFrame, buildMask;

	public Texture2D smallButtonHover, smallButtonClick;
	public Texture2D healthy, damaged, critical;

	private AudioSource sellSound;


	// Use this for initialization
	void Start () {
		sellSound = GetComponent<AudioSource>();
		resourceValues = new Dictionary< ResourceType, int >();
		resourceLimits = new Dictionary< ResourceType, int >();
		player = transform.root.GetComponent< Player >();
		ResourceManager.StoreSelectBoxItems(selectBoxSkin, healthy, damaged, critical);
		resourceImages = new Dictionary< ResourceType, Texture2D >();
		for(int i = 0; i < resources.Length; i++) {
		    switch(resources[i].name) {
		        case "money":
		            resourceImages.Add(ResourceType.Money, resources[i]);
		            resourceValues.Add(ResourceType.Money, 0);
		            resourceLimits.Add(ResourceType.Money, 0);
		            break;
		        case "power":
		            resourceImages.Add(ResourceType.Power, resources[i]);
		            resourceValues.Add(ResourceType.Power, 0);
		            resourceLimits.Add(ResourceType.Power, 0);
		            break;
		        default: break;
		    }
		}
		buildAreaHeight = Screen.height - RESOURCE_BAR_HEIGHT - SELECTION_NAME_HEIGHT - 2 * BUTTON_SPACING;
	}
	
	// Update is called once per frame
	void OnGUI () {
		if(player && player.human) {
		
		    DrawOrdersBar();
		    DrawResourcesBar();
		}
        timer -= Time.deltaTime;
        if (timer < 0)
        {
            OpenPauseMenu();
        }
        GUI.Box(new Rect(Screen.width - TEXT_WIDTH, 0, TEXT_WIDTH, TEXT_HEIGHT), "Time: " + (int)timer);
    }

    private void OpenPauseMenu()
    {
        Time.timeScale = 0.0f;
        GetComponent<LossMenu>().enabled = true;
        GetComponentInParent<UserInput>().enabled = false;
        Cursor.visible = true;
        ResourceManager.MenuOpen = true;
    }

    private void DrawOrdersBar() {
	    GUI.skin = ordersSkin;
	    GUI.BeginGroup(new Rect(Screen.width - ORDERS_BAR_WIDTH - BUILD_IMAGE_WIDTH, RESOURCE_BAR_HEIGHT, ORDERS_BAR_WIDTH + BUILD_IMAGE_WIDTH, Screen.height - RESOURCE_BAR_HEIGHT));
		GUI.Box(new Rect(BUILD_IMAGE_WIDTH + SCROLL_BAR_WIDTH, 0, ORDERS_BAR_WIDTH, Screen.height - RESOURCE_BAR_HEIGHT), "");


	    string selectionName = "";
        int count = player.SelectedObjects.Count;
		if(count>0&&player.SelectedObjects[count-1]) {
		    selectionName = player.SelectedObjects[count-1].objectName;
			if(player.SelectedObjects[count-1].IsOwnedBy(player)) {
			    //reset slider value if the selected object has changed
			    if(lastSelection && lastSelection != player.SelectedObjects[count-1]) sliderValue = 0.0f;
			    if(player.SelectedObjects[count-1].IsActive && count ==1) DrawActions(player.SelectedObjects[count-1].GetActions());
			    //store the current selection
			    lastSelection = player.SelectedObjects[count-1];
			    Building selectedBuilding = lastSelection.GetComponent< Building >();
				if(selectedBuilding) {
    				DrawBuildQueue(selectedBuilding.getBuildQueueValues(), selectedBuilding.getBuildPercentage());
					DrawStandardBuildingOptions(selectedBuilding);
				}
			}
		}
		if(!selectionName.Equals("")) {
		    int leftPos = BUILD_IMAGE_WIDTH + SCROLL_BAR_WIDTH / 2;
		    int topPos = buildAreaHeight + BUTTON_SPACING;
		    GUI.Label(new Rect(leftPos, topPos, ORDERS_BAR_WIDTH, SELECTION_NAME_HEIGHT), selectionName);
		}
	    GUI.EndGroup();
	}

	private void DrawResourcesBar() {
	    GUI.skin = resourceSkin;
	    GUI.BeginGroup(new Rect(0,0,Screen.width,RESOURCE_BAR_HEIGHT));
	    GUI.Box(new Rect(0,0,Screen.width,RESOURCE_BAR_HEIGHT),"");
	    int topPos = 4, iconLeft = 4, textLeft = 20;
		DrawResourceIcon(ResourceType.Money, iconLeft, textLeft, topPos);
		iconLeft += TEXT_WIDTH;
		textLeft += TEXT_WIDTH;
		//DrawResourceIcon(ResourceType.Power, iconLeft, textLeft, topPos);
	    GUI.EndGroup();
	}

	private void DrawResourceIcon(ResourceType type, int iconLeft, int textLeft, int topPos) {
	    Texture2D icon = resourceImages[type];
	    string text = resourceValues[type].ToString() + "/" + resourceLimits[type].ToString();
	    GUI.DrawTexture(new Rect(iconLeft, topPos, ICON_WIDTH, ICON_HEIGHT), icon);
	    GUI.Label (new Rect(textLeft, topPos, TEXT_WIDTH, TEXT_HEIGHT), text);
	}

	public bool MouseInBounds() {
	    //Screen coordinates start in the lower-left corner of the screen
	    //not the top-left of the screen like the drawing coordinates do
	    Vector3 mousePos = Input.mousePosition;
	    bool insideWidth = mousePos.x >= 0 && mousePos.x <= Screen.width - ORDERS_BAR_WIDTH;
	    bool insideHeight = mousePos.y >= 0 && mousePos.y <= Screen.height - RESOURCE_BAR_HEIGHT;
	    return insideWidth && insideHeight;
	}

	public Rect GetPlayingArea() {
	    return new Rect(0, RESOURCE_BAR_HEIGHT, Screen.width - ORDERS_BAR_WIDTH, Screen.height - RESOURCE_BAR_HEIGHT);
	}

	public void SetResourceValues(Dictionary< ResourceType, int > resourceValues, Dictionary< ResourceType, int > resourceLimits) {
	    this.resourceValues = resourceValues;
	    this.resourceLimits = resourceLimits;
	}

	private void DrawActions(string[] actions) {
	    GUIStyle buttons = new GUIStyle();
	    buttons.hover.background = buttonHover;
	    buttons.active.background = buttonClick;
	    GUI.skin.button = buttons;
	    int numActions = actions.Length;
	    //define the area to draw the actions inside
	    GUI.BeginGroup(new Rect(BUILD_IMAGE_WIDTH, 0, ORDERS_BAR_WIDTH, buildAreaHeight));
	    //draw scroll bar for the list of actions if need be
	    if(numActions >= MaxNumRows(buildAreaHeight)) DrawSlider(buildAreaHeight, numActions / 2.0f);
	    //display possible actions as buttons and handle the button click for each
	    for(int i = 0; i < numActions; i++) {
	        int column = i % 2;
	        int row = i / 2;
	        Rect pos = GetButtonPos(row, column);
	        Texture2D action = ResourceManager.GetBuildImage(actions[i]);
	        if(action) {
	            //create the button and handle the click of that button
	            if(GUI.Button(pos, action)) {
	                if(player.SelectedObjects.Count>0) player.SelectedObjects[0].PerformAction(actions[i]);
	            }
	        }
	    }
	    GUI.EndGroup();
	}

	private int MaxNumRows(int areaHeight) {
	    return areaHeight / BUILD_IMAGE_HEIGHT;
	}
	 
	private Rect GetButtonPos(int row, int column) {
	    int left = SCROLL_BAR_WIDTH + column * BUILD_IMAGE_WIDTH;
	    float top = row * BUILD_IMAGE_HEIGHT - sliderValue * BUILD_IMAGE_HEIGHT;
	    return new Rect(left, top, BUILD_IMAGE_WIDTH, BUILD_IMAGE_HEIGHT);
	}
	 
	private void DrawSlider(int groupHeight, float numRows) {
	    //slider goes from 0 to the number of rows that do not fit on screen
	    sliderValue = GUI.VerticalSlider(GetScrollPos(groupHeight), sliderValue, 0.0f, numRows - MaxNumRows(groupHeight));
	}

	private Rect GetScrollPos(int groupHeight) {
	    return new Rect(BUTTON_SPACING, BUTTON_SPACING, SCROLL_BAR_WIDTH, groupHeight - 2 * BUTTON_SPACING);
	}

	private void DrawBuildQueue(string[] buildQueue, float buildPercentage) {
	    for(int i = 0; i < buildQueue.Length; i++) {
	        float topPos = i * BUILD_IMAGE_HEIGHT - (i+1) * BUILD_IMAGE_PADDING;
	        Rect buildPos = new Rect(BUILD_IMAGE_PADDING, topPos, BUILD_IMAGE_WIDTH, BUILD_IMAGE_HEIGHT);
	        GUI.DrawTexture(buildPos, ResourceManager.GetBuildImage(buildQueue[i]));
	        GUI.DrawTexture(buildPos, buildFrame);
	        topPos += BUILD_IMAGE_PADDING;
	        float width = BUILD_IMAGE_WIDTH - 2 * BUILD_IMAGE_PADDING;
	        float height = BUILD_IMAGE_HEIGHT - 2 * BUILD_IMAGE_PADDING;
	        if(i==0) {
	            //shrink the build mask on the item currently being built to give an idea of progress
	            topPos += height * buildPercentage;
	            height *= (1 - buildPercentage);
	        }
	        GUI.DrawTexture(new Rect(2 * BUILD_IMAGE_PADDING, topPos, width, height), buildMask);
	    }
	}

	private void DrawStandardBuildingOptions(Building building) {
	    GUIStyle buttons = new GUIStyle();
	    buttons.hover.background = smallButtonHover;
	    buttons.active.background = smallButtonClick;
	    GUI.skin.button = buttons;
	    int leftPos = BUILD_IMAGE_WIDTH + SCROLL_BAR_WIDTH + BUTTON_SPACING;
	    int topPos = buildAreaHeight - BUILD_IMAGE_HEIGHT / 2;
	    int width = BUILD_IMAGE_WIDTH / 2;
	    int height = BUILD_IMAGE_HEIGHT / 2;
	    /*if(building.hasSpawnPoint()) {
	        if(GUI.Button(new Rect(leftPos, topPos, width, height), building.rallyPointImage)) {
	        	leftPos += width + BUTTON_SPACING;
	        }
	        if(GUI.Button(new Rect(leftPos, topPos, width, height), building.sellImage)) {
			    building.Sell();
			}
	    }*/
	    if(GUI.Button(new Rect(leftPos, topPos, width, height), building.sellImage)) {
	    	sellSound.Play();
			building.Sell();
		}
	}

}
