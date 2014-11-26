using UnityEngine;
using System.Collections;

public class Coin : MonoBehaviour {

    Animator animator;
    Vector3 removePosition = Vector3.right*30;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // 애니메이션을 작동하도록 한다.
    public void StartCoinAnimation(Vector3 setRemovePosition = default(Vector3))
    {
        Debug.Log(gameObject.name + transform.position);
        if( setRemovePosition != Vector3.zero)
        {
            removePosition = setRemovePosition;
        }
        animator.SetTrigger("startAnimation");
    }

    //애니메이션이 종료되면 호출된다.
    void EndCoinAnimation()
    {
        transform.parent.position = removePosition;
    }
}
