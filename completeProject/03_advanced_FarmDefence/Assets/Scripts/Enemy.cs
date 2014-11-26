using UnityEngine;
using System.Collections;
using System;

public enum EnemyState
{
    none,
    move,
    attack,
    damaged,
    dead
}

public class Enemy : MonoBehaviour, IDamageable
{
    // 적 상태.
    EnemyState currentState = EnemyState.none;
    // LineCast에 사용될 위치.
    public Transform frontPosition;
    protected RaycastHit2D isObstacle;
    // 이동 속도.
    public float moveSpeed = 1.0f;
    // 체력.
    protected float currentHP;
    protected float maxHP;
    // 공격 가능여부 저장.
    protected bool enableAttack = true;
    protected float attackPower = 10;
    protected float damagedPower;
    protected Animator animator;

    public UISlider hpBarSlider;
    public GameObject hpBarObj;
    public Camera uiCam;
    public UIPanel hpBarPanel;
    Vector3 hpBarCalVec3;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        #if UNITY_EDITOR
        currentHP = 2;
        #endif
    }

    protected virtual void SetupEnemy()
    {
    }

    public void InitEnemy(float setupMaxHP, 
                          float setupAttackPower, 
                          float setupMoveSpeed)
    {
        // walk 애니메이션을 재생하도록 한다. 
        animator.SetTrigger("isAlive");
        // HP와 공격력, 이동속도를 설정한다.
        maxHP = setupMaxHP;
        currentHP = setupMaxHP;
        attackPower = setupAttackPower;
        moveSpeed = setupMoveSpeed;
        // 캐릭터 상태를 변경하여 이동을 시작하도록 한다.
        currentState = EnemyState.move;
        // isAlive 트리거를 초기화해서 dead 애니메이션 종료 후 walk 애니메이션 바로 전환되는 것을 방지.
        animator.ResetTrigger("isAlive");
    }

    RaycastHit2D ray;

    void FixedUpdate()
    {
        switch (currentState)
        {
        case EnemyState.none:
            // 이동 중지.
            rigidbody2D.velocity = Vector2.zero;
            break;
        case EnemyState.move:
            // 장애물이 있는지 Linecast로 검출.
            isObstacle = Physics2D.Linecast(
                transform.position, frontPosition.position,
                1 << LayerMask.NameToLayer("Obstacle"));
            if (isObstacle)
            {
                // 장애물을 만나면 공격 애니메이션으로 전환.
                if (enableAttack)
                {
                    currentState = EnemyState.attack;
                    // Animator에 등록한 attack Trigger를 작동.
                    animator.SetTrigger("attack");
                }
            }
            else
            {
                // 장애물이 없다면 이동.
                rigidbody2D.velocity = new Vector2(-moveSpeed, 
                                                   rigidbody2D.velocity.y);
            }
            if (uiCam != null)
            {
                RepositionHPBar();
            }
            break;
        case EnemyState.attack:
            rigidbody2D.velocity = Vector2.zero;
            break; 
        case EnemyState.damaged:
            rigidbody2D.velocity = Vector2.zero;
            break;
        case EnemyState.dead:
            rigidbody2D.velocity = Vector2.zero;
            break;
        }
    }

    void AttackAnimationEnd()
    {
        if( currentState == EnemyState.attack)
        {
            currentState = EnemyState.move;
        }
    }

    public virtual void Attack()
    {
        //농장에 피해를 가한다. 

        RaycastHit2D findObstacle = Physics2D.Linecast(
            transform.position, frontPosition.position,
            1 << LayerMask.NameToLayer("Obstacle"));
        if (findObstacle)
        {
            IDamageable damageTarget =
                (IDamageable)findObstacle.transform.GetComponent(typeof(IDamageable));
            damageTarget.Damage(attackPower);
        }
    }

    public void Damage(float damageTaken)
    {
        // dead 나 none 상태일 때 진행되지 않도록 한다.
        if (currentState == EnemyState.dead || currentState == EnemyState.none)
        {
            if( IsInvoking("ChangeStateToMove") )
            {
                CancelInvoke("ChangeStateToMove");
            }
            return;
        }
            
        // 충돌 후 일정 시간 동안 이동 정지.
        currentState = EnemyState.damaged;
        if( IsInvoking("ChangeStateToMove") )
        {
            CancelInvoke("ChangeStateToMove");
        }
        Invoke("ChangeStateToMove", 0.3f);

        // currentHP를 소진한다.
        currentHP -= damageTaken;
        // 체력 표시를 감소시킨다.
        hpBarSlider.value = (float)currentHP/(float)maxHP;

        // 현재 체력이 0과 같거나 작다면 
        if (currentHP <= 0)
        {
            currentHP = 0;
            // 체력 표시를 모두 제거한다.
            hpBarSlider.value = 0;

            enableAttack = false;
            currentState = EnemyState.dead;
            // dead 애니메이션 재생
            animator.SetTrigger("isDead");

            if( IsInvoking("ChangeStateToMove") )
            {
                CancelInvoke("ChangeStateToMove");
            }

            // 점수 증가.
            GameData.Instance.gamePlayManager.AddScore(10);
            // 적 보스가 사망하면 다시 적을 생성할 수 있도록 처리한다.
            if (gameObject.tag == "boss")
            {
                GameData.Instance.gamePlayManager.SetupGameStateToIdle();
            }
        }
        else
        {
            animator.SetTrigger("damaged");
        }
    }

    void ChangeStateToMove()
    {
        currentState = EnemyState.move;
    }

    public void InitHPBar(UISlider targetHPBar,
                          UIPanel targetPanel, 
                          Camera targetCam)
    {
        // 멤버 필드 할당.
        hpBarSlider = targetHPBar;
        hpBarObj = hpBarSlider.gameObject;
        hpBarPanel = targetPanel;
        uiCam = targetCam;
        // 오브젝트 풀에서 제외되도록 초기값 임시 수정.
        hpBarObj.transform.localPosition = Vector3.left * 1000;
        // hpbar를 켠다.
        TurnOnOffHPBar(true);
    }

    protected void RepositionHPBar()
    {
        // 적 위치가 카메라 상에서 어느 위치인지 계산.
        hpBarCalVec3 = uiCam.WorldToScreenPoint(transform.position);
        hpBarCalVec3.z = 0;

        if(GameData.Instance.targetWidth == 0)
        {
            GameData.Instance.targetWidth = hpBarPanel.width*
                (GameData.Instance.targetHeight/hpBarPanel.height);
            Debug.Log(GameData.Instance.targetWidth);
        }

        // UIPanel의 크기를 고려하여 상대적인 위치를 적용. 
        hpBarCalVec3.x = 
            (hpBarCalVec3.x / Screen.width) * GameData.Instance.targetWidth;
        hpBarCalVec3.y = 
           (hpBarCalVec3.y / Screen.height) * GameData.Instance.targetHeight;
        hpBarObj.transform.localPosition = hpBarCalVec3;
    }

    public void TurnOnOffHPBar(bool isTurnOn = false)
    {
        // hpbar를 끄고 켠다.
        hpBarObj.SetActive(isTurnOn);
    }

    public void DeadEnd()
    {
        // 임의의 확율로 코인을 생성한다.
        int makePossibleCoin = UnityEngine.Random.Range(0, 10);
        if(makePossibleCoin >= 7)
        {
            GameData.Instance.gamePlayManager.SpawnCoin(transform.position);
            //처치한 적 숫자 및 획득한 코인 숫자 전달.
            GameData.Instance.gamePlayManager.AddDeadEnemyAndCoin(10);
        }
        else
        {
            //처치한 적 숫자 전달.
            GameData.Instance.gamePlayManager.AddDeadEnemyAndCoin();
        }

        // 적 캐릭터를 초기 위치로 이동시킨다.
        currentState = EnemyState.none;
        transform.position = 
            GameData.Instance.gamePlayManager.gameObjectPoolPosition.position;

        // hpbar를 반환한다.
        hpBarObj.transform.position = 
            GameData.Instance.gamePlayManager.enemyHPBarRoot.position;
        hpBarObj = null;
    }

    // idle 애니메이션만 재생하는 상태로 변경.
    public void FreezeEnemy()
    {
        currentState = EnemyState.none;
        if( IsInvoking("ChangeStateToMove") )
        {
            CancelInvoke("ChangeStateToMove");
        }
    }

}
