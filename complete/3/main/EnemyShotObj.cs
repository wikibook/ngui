using UnityEngine;
using System.Collections;

public class EnemyShotObj : ShotObj
{
	void OnTriggerEnter2D(Collider2D other)
	{
		// 장애물과 충돌 시, 공격하여 피해를 가한다.
		if( other.CompareTag("obstacle"))
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
}

