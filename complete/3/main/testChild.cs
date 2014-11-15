using UnityEngine;
using System.Collections;

public class testChild : testParent, IDamage {

	public override void InitString()
	{
		printStr = "child class";
	}

//	public void Damaged(float damage)
//	{
//		Debug.Log("damaged child");
//	}
}
