using UnityEngine;
using System.Collections;

public class GameObjectMove : MonoBehaviour {

	Vector3 calVector;
	float speed = 1.0f;

	void FixedUpdate () {
	
		calVector = transform.position;
		calVector.x += Input.GetAxis("Horizontal") * speed;
		calVector.z += Input.GetAxis("Vertical") * speed;

		transform.position = calVector;
	}
}
