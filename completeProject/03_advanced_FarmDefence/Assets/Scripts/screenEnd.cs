using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class screenEnd : MonoBehaviour {

	public Camera mainCam;
	public GameObject leftCenterObj;

	int leftCenterY;
	Vector3 leftCenterVector;

	void OnEnable () {
		leftCenterY = Screen.height / 2;

		leftCenterVector = mainCam.ScreenToWorldPoint( new Vector3(0, leftCenterY, 10));

		leftCenterObj.transform.position = leftCenterVector;
	}
}
