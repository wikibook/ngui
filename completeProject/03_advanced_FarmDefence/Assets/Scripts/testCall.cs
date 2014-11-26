using UnityEngine;
using System.Collections;

public class testCall : MonoBehaviour {

	public testParent tp;
	public testChild tc;

	IDamage test;

	void OnEnable()
	{
//		tp.Test();
		test = (IDamage)tp.GetComponent(typeof(IDamage));
		test.Damaged(1);

		test = (IDamage)tc.GetComponent(typeof(IDamage));
		test.Damaged(1);
	}
}
