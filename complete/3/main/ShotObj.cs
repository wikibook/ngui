using UnityEngine;
using System.Collections;

public class ShotObj : MonoBehaviour
{

    protected float attackPower = 1;
    protected Vector3 initPos;
    protected bool isWork = false;

    public void InitShotObj(float setupAttackPower)
    {
        attackPower = setupAttackPower;
    }

    // 사용중이지 않을 때 돌아갈 위치 저장.
    public void InitReturnPosition(Vector3 setupInitPos)
    {
        initPos = setupInitPos;
    }
    
    // 충돌처리를 허가할 때 사용.
    public void TurnOnTrigger()
    {
        isWork = true;
    }
    
    // 발사 게임 오브젝트를 초기화한다.
    public void ResetShotObj()
    {
        transform.position = initPos;
        isWork = false;
        rigidbody2D.velocity = Vector3.zero;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 사용중이지 않을 때 충돌처리를 막는다.
        if(!isWork) return;
        
        // 적 캐릭터 인 경우, 공격하여 피해를 가한다.
        if (other.CompareTag("enemy") || other.CompareTag("boss"))
        {
            // 공격 후 게임 오브젝트 제거.
            AttackAndRemove(other);
        }
        // 게임 플레이 화면 외부로 진입했을 때 초기 위치로 돌아가도록 한다.
        else if (other.CompareTag("invisibleArea"))
        {
            ResetShotObj();
        }
    }
    
    protected void AttackAndRemove(Collider2D other)
    {
        IDamageable damageTarget = 
            (IDamageable)other.GetComponent(typeof(IDamageable));
        damageTarget.Damage(attackPower);
        // 공격 후 초기화. 
        ResetShotObj();
    }


}
