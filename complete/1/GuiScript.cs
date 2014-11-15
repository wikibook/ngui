using UnityEngine;
using System.Collections;

public class GuiScript : MonoBehaviour {

	bool _initFlag = false; 
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	void OnGUI() {
		if(!_initFlag)
		{
			if (GUI.Button (new Rect (10,10,150,100), "Start Button"))
			{
				_initFlag = true;
			}
		}
		else
		{
			GUI.Label (new Rect (10, 10, 100, 20), "Hello Unity!");
		}
	}
}
