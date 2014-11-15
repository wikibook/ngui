using UnityEngine;
using System.Collections;

public enum MolyState {idle, move, wait, hited}
public enum SpriteType {Ppo, Ppu}

public class MolyUnit : MonoBehaviour {

	public MolyState nowMolyState = MolyState.idle; //두더지의 현재 상태.

	SpriteType nowMolySpriteType = SpriteType.Ppo; //현재 스프라이트 타입.
	
	public UISprite molySprite;
	public GameObject effectObj;

	//움직임을 관장할 2개의 TweenPostion
	TweenPosition molyTweenPos;
	TweenPosition effectTweenPos;
	
	Vector3 molyFromPos = new Vector3(0, -100, 0); //Moly 게임 오브젝트의 최초 위치.

	//위쪽으로 이동을 마치고 사용자의 입력을 기다릴 때 사용할 대기용 타이머.
	public float waitTimer = 0;
	float waitTimeFact = 0.5f;

	//hit이후 효과를 보여주는 시간을 처리할 타이머.
	public float hitAfterTimer = 0;
	float hitAfterTimeFact = 0.5f;

	void OnEnable ()
	{
		molyTweenPos = molySprite.GetComponent<TweenPosition>();
		effectTweenPos = effectObj.GetComponent<TweenPosition>();

		//molyTweenPos 초기화
		molyTweenPos.from = molyFromPos;
		molyTweenPos.to = Vector3.zero;
		molyTweenPos.eventReceiver = gameObject;
		molyTweenPos.enabled = false;

		effectTweenPos.enabled = false;
	}
	
	void FixedUpdate()
	{
		switch( nowMolyState )
		{
		case MolyState.hited:
			//터치 입력 이후 대기 시간 처리.
			hitAfterTimer += Time.fixedDeltaTime;
			if(hitAfterTimer >= hitAfterTimeFact)
			{
				hitAfterTimer = 0;
				MolyMove(false);
			}
			break;
		case MolyState.wait:
			//두더지 대기 중 시간 처리.
			waitTimer += Time.fixedDeltaTime;
			if( waitTimer >= waitTimeFact)
			{
				waitTimer = 0;
				MolyMove(false);
			}
			break;
		}
	}

	public void StartUseMoly(SpriteType spriteType=SpriteType.Ppo, 
	                         float waitTime = 0.5f)
	{
		//idle이 아닐 때 실행 종료.
		if( nowMolyState != MolyState.idle) return;
		//두더지 대기시간
		waitTimeFact = waitTime;
		nowMolySpriteType = spriteType;

		//두더지 스프라이트 변경.
		switch(spriteType)
		{
		case SpriteType.Ppo:
			molySprite.spriteName = "ppo";
			break;
		case SpriteType.Ppu:
			molySprite.spriteName = "ppu";
			break;
		}
		molySprite.MakePixelPerfect();
		//두더지 아래에서 위로 움직이게 설정.
		MolyMove(true);
	}

	void MolyMove(bool goUpside=true)
	{
		//두더지 스테이트 변경.
		nowMolyState = MolyState.move; 

		switch( goUpside )
		{
		case true: //아래에서 위로 움직인다.
			molyTweenPos.from = molyFromPos;
			molyTweenPos.to = Vector3.zero;
			break;
		case false: //위에서 아래로 움직인다.
			molyTweenPos.from = Vector3.zero;
			molyTweenPos.to = molyFromPos;
			effectObj.SetActive(false);
			break;
		}
		molyTweenPos.ResetToBeginning();
		molyTweenPos.callWhenFinished = "FinishMove";
		molyTweenPos.enabled = true;
	}

	void FinishMove()
	{
		if ( molyTweenPos.from == Vector3.zero )
		{
			nowMolyState = MolyState.idle;
		}
		else
		{
			nowMolyState = MolyState.wait;
		}
		molyTweenPos.enabled = false;
	}


	void OnPress()
	{
		switch( nowMolyState )
		{
		case MolyState.wait:
			//타이머 초기화.
			waitTimer = 0;
			hitAfterTimer = 0;
			//스테이트 변경.
			nowMolyState = MolyState.hited;
			HitedMoly();
			break;
		}
	}

	void HitedMoly()
	{
		//스프라이트 변경
		switch( nowMolySpriteType )
		{
		case SpriteType.Ppo:
			molySprite.spriteName = "ppo_hit";
			if( GameManager.instance.haveFeverMode)
			{
				//피버모드 점수.
				GameManager.instance.AddScore( 100 );
			}
			else
			{
				//점수 증가.
				GameManager.instance.AddScore( 10 );
			}
			break;
		case SpriteType.Ppu:
			molySprite.spriteName = "ppu_hit";
			//점수 감소.
			GameManager.instance.AddScore( -5 );
			break;
        }
		//hit 효과음 재생.
		GameManager.instance.PlayMolyHitSound( nowMolySpriteType );

		molySprite.MakePixelPerfect();
		//이펙트 발생
		effectTweenPos.gameObject.SetActive(true);
		effectTweenPos.ResetToBeginning();
		effectTweenPos.enabled = true;
    }
}
