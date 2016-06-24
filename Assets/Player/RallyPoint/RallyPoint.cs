using UnityEngine;
using System.Collections;

public class RallyPoint : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void Enable () {
	    Renderer[] renderers = GetComponentsInChildren< Renderer >();
	    foreach(Renderer renderer in renderers) renderer.enabled = true;
	}
	 
	public void Disable () {
	    Renderer[] renderers = GetComponentsInChildren< Renderer >();
	    foreach(Renderer renderer in renderers) renderer.enabled = false;
	}
	
}
