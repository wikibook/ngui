using UnityEngine;
using System.Collections;

public class testParent : MonoBehaviour, IDamage {

	protected string printStr = "";

	void OnEnable()
	{
		InitString();
		PrintString();
	}
	// 상속을 통해서 변경하고자하는 메소드.
	public virtual void InitString()
	{
		printStr = "parent class";
	}

	void PrintString()
	{
		Debug.Log(printStr);
	}

	public void Damaged(float damage)
	{
		Debug.Log("damaged parent");
	}
}
//
