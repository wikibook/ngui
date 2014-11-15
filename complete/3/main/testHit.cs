using UnityEngine;
using System.Collections;

public class testHit : MonoBehaviour
{

    public bool isMove = false;

    void OnTriggerEnter2D (Collider2D col)
    {
        Debug.Log("HO?");
    }

    void FixedUpdate()
    {
        if (isMove)
        {
            transform.position += (Vector3.left * 0.1f);
        }
    }
}
