using UnityEngine;
using System.Collections;

public class FarmerTouchControl : MonoBehaviour {

	//마우스 클릭으로 입력된 좌표를 공간 좌표로 변환하는데 사용.
	public Camera mainCamera;
	//발사할 게임 오브젝트.
	public GameObject fireObj;
	//새총을 발사할 지점.
	public Transform firePoint;
	//새총을 발사할 방향.
	Vector3 fireDirection;
	//발사 속도.
	public float fireSpeed = 3;

	//발사 가능 여부 판단.
	bool enableAttack = true;
	//마지막 사용자 입력 위치 저장.
	Vector3 lastInputPosition;

	//Vector3 계산에 사용.
	Vector3 tempVector3;
	//Vector2 계산에 사용.
	Vector2 tempVector2 = new Vector2();
	//새총 발사에 사용되는 오브젝트 처리.
	GameObject tempObj;

	Animator animator;
	// 발사되는 게임 오브젝트의 ShotObj 스크립트 처리.
	ShotObj shotObjScript;

    // shot gameobject pool
    GameObjectPool objPool;
    Vector3 spawnPos = new Vector3(0, 50, 0);

    // 레벨에 따른 공격력 저장.
    int attDamage = 0;

	void Awake()
	{
		animator = GetComponent<Animator>();
	}

    void OnEnable()
    {
        InitGameObjectPool();

        // 공격력 계산.
        attDamage 
            = 1 + GameData.Instance.ConvertUpgradeLvToAddValue(
                GameData.Instance.userdata.attLv);
    }

	void Update()
	{
		// 마우스 왼쪽 버튼 입력이 발생했을 때.
		if( Input.GetMouseButton(0) )
		{
			// 마우스 입력 위치를 저장.
			lastInputPosition = Input.mousePosition;

			// 공격 가능 여부를 판단.
			if( enableAttack )
			{
				// 공격 애니메이션으로 전환. 
				animator.SetTrigger("fire");
			}
		}
	}

	// 발사 위치와 마우스 입력 위치를 잊는 푸른색 선을 그린다.
	void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawLine( firePoint.position, tempVector3);
	}

	void Fire(Vector3 inputPosition)
	{
		// 입력 위치(inputPosition)를 카메라가 바라보는 영역 안의 월드 좌표(절대 좌표)로 변환.
		tempVector3 = mainCamera.ScreenToWorldPoint(inputPosition);
		tempVector3.z = 0;
		// 벡터의 뺄셈 후 방향만 지닌 단위 벡터로 변경.
		fireDirection = tempVector3 - firePoint.position;
		fireDirection = fireDirection.normalized;

        // 발사할 오브젝트.
        shotObjScript = null;
        if( !objPool.NextGameObject(out tempObj) )
        {
            tempObj = Instantiate(
                fireObj, 
                spawnPos, 
                Quaternion.Euler(Vector3.up * 90)
                ) as GameObject;
            tempObj.name = tempObj.name + objPool.lastIndex;
            objPool.AddGameObject(tempObj);
            
            shotObjScript = tempObj.GetComponent<ShotObj>();
            shotObjScript.InitReturnPosition(spawnPos);
        }

        if(shotObjScript == null)
        {
            shotObjScript = tempObj.GetComponent<ShotObj>();
        }

        tempObj.transform.position = firePoint.position;
        // 발사한 오브젝트 속도 계산.
        tempVector2.Set(fireDirection.x, fireDirection.y);
        tempVector2 = tempVector2 * fireSpeed;
        // 속도 적용.
        tempObj.rigidbody2D.velocity = tempVector2;

        // 공격력을 전달한다.
        if(attDamage==0)
        {
            attDamage 
                = 1 + GameData.Instance.ConvertUpgradeLvToAddValue(
                    GameData.Instance.userdata.attLv);
        }
        shotObjScript.InitShotObj(attDamage);
        shotObjScript.TurnOnTrigger();
	}

	void FireTrigger()
	{
		// 발사 애니메이션이 진행되어 새총 발사를 하게 될 때 발사를 처리한다.
		Fire(lastInputPosition);
	}
	
	void FireEnd()
	{
		// 발사 애니메이션이 종료될 때, 공격 가능하도록 변경.
		enableAttack = true;
	}

    // 게임 오브젝트 풀 초기화.
    void InitGameObjectPool()
    {
        spawnPos += GameData.Instance.gamePlayManager
            .gameObjectPoolPosition.transform.position;
        //원거리에서 공격할 수 있도록 구현.
        objPool = new GameObjectPool(
            spawnPos.x,
            tempObj);

        // 발사 오브젝트 생성. 
        for(int i=0;i<20;++i)
        {
            shotObjScript = null;
            GameObject makeObj = 
                Instantiate(
                fireObj, 
                spawnPos, 
                Quaternion.Euler(Vector3.up * 90)
                ) as GameObject;
            makeObj.name = makeObj.name + i;
            objPool.AddGameObject(makeObj);
            
            shotObjScript = makeObj.GetComponent<ShotObj>();
            shotObjScript.InitReturnPosition(spawnPos);
        }
    }
}
