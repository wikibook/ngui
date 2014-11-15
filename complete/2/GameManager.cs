using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum GameState {ready, idle, gameover, wait}

public class GameManager : MonoBehaviour {

	public static GameManager instance;

	public GameState nowGameState = GameState.ready; //게임 상황 판단.
	
	public UISlider timeBarSlider; //타임바.
	public UILabel timeBarText; //타임바의 시간을 표시할 라벨.
	public UISprite readySprite; //준비 메시지.
	public UILabel scoreLabel; //점수를 표시할 라벨.

	//결과창.
	public GameObject ResultPopupWindow;
	public UILabel ResultScoreLabel;

	float mainTimer = 0; //게임 시간을 저장.

	int score = 0; //게임 점수 저장.

	public List<float> ppoMolyProbability = new List<float>();
	public List<float> molyWaitTime = new List<float>(); //두더지의 출현 후 대기 시간을 구간별로 설정한다.
	public List<float> molyAppearTime = new List<float>(); //두더지 출현 시간을 구간별로 설정한다.
	public List<int> molyAppearCount = new List<int>(); //구간별로 동시에 출현 가능한 두더지 숫자를 설정한다.
	public List<MolyUnit> molyUnitList = new List<MolyUnit>();
	
	float molySpawnRandom = 0;
	int molySpawnCount = 0;
	int molySelectRandom = 0;
	int listIndex = 0;

	//효과음 재생에 필요한 필드.
	public AudioClip readyClip;
	public AudioClip goClip;
	public AudioClip ppoHitClip;
	public AudioClip ppuHitClip;

	public AudioSource audioSource;


	int ComboCount = 0; //현재 콤보 숫자를 기억.

	public bool haveFeverMode = false; //피버모드가 작동중인지 판단할 때 사용.
	float fever = 0; //피버가 채워지는 현재 값을 기억.

	public UILabel comboText; //콤보를 표현하는 라벨.
	public UILabel feverText; //피버모드를 알리는 라벨.
	public UISlider feverSlider; //피버를 표현하는 Progress Bar를 조절하는 슬라이더.



	void Awake ()
	{
		//정적변수 초기화
		if( instance == null)
		{
			instance = this;
		}
	}

	void OnEnable ()
	{
		InitReady();
	}

	void Update () 
	{
		switch(nowGameState)
		{
		case GameState.idle:
			//deltaTime 누적.
			mainTimer += Time.deltaTime;
			if( mainTimer >= 60.0f )
			{
				//게임 종료.
				mainTimer = 60;
				nowGameState = GameState.gameover;

				CancelInvoke("RandomMolySpawn");
				CancelInvoke("RepeatAddListIndex");

				//결과창 표시.
				ResultPopupWindow.SetActive(true);
				ResultScoreLabel.text = score.ToString();
			}
			//timeBar를 점차 비운다.
			timeBarSlider.value = (60.0f - mainTimer) / 60.0f; 
			//timeBarText는 60에서부터 1초단위로 줄어든다.
			timeBarText.text = string.Format("{0:f0}", (60.0f - mainTimer));
			break;
		}
	}

	public void InitReady ()
	{
		//이미 작동중인 ResetCombo메소드 Invoke가 있으면 취소하는 구문.
		if( IsInvoking("ResetCombo") )
		{
			CancelInvoke("ResetCombo");
		}
		//이미 작동중인 ResetFever메소드 Invoke가 있으면 취소하는 구문.
		if( IsInvoking("ResetFever") )
		{
			CancelInvoke("ResetFever");
		}

		//콤보와 피버 초기화.
		ResetCombo();
		ResetFever();

		//결과창 비활성화.
		ResultPopupWindow.SetActive(false);
		//게임 스테이트 초기화.
		nowGameState = GameState.ready;
		//점수 초기화.
		score = 0;
		scoreLabel.text = "0";

		//스프라이트 초기화.
		readySprite.spriteName = "Ready";
		readySprite.MakePixelPerfect();
		//스프라이트 활성화.
		readySprite.gameObject.SetActive(true);
		//2초 후 ReadyToGo 실행.
		Invoke("ReadyToGo", 2.0f);

		//ready효과음 재생.
		audioSource.PlayOneShot(readyClip);
	}

	void ReadyToGo ()
	{
		//스프라이트 변경.
		readySprite.spriteName = "Go";
		readySprite.MakePixelPerfect();
		//1초 후 GoToIdle 실행.
		Invoke("GoToIdle", 1.0f);

		//go효과음 재생.
		audioSource.PlayOneShot(goClip);
	}

	void GoToIdle()
	{
		//스프라이트 비활성화.
		readySprite.gameObject.SetActive(false);
		//게임 진행 상태로 변경.
		mainTimer = 0;
		nowGameState = GameState.idle;

		listIndex = 0;
		InvokeRepeating("RandomMolySpawn", 0.01f, 1f);
		InvokeRepeating("RepeatAddListIndex", 10f, 10f);
	}

	public void AddScore(int addScore)
	{
		//점수가 0보다 클 때(빨간색 두더지를 클릭했을 때) 작동.
		if( addScore > 0)
		{
			ComboCount++; //콤보 증가.
			score += (addScore * ComboCount); //콤보를 점수에 반영.

			//콤보 숫자를 콤보 라벨에 표현.
			comboText.text = string.Format("{0}[ba4926]COMBOS[-]", ComboCount);
			comboText.gameObject.SetActive(true); //콤보 라벨을 화면에 나타나도록 한다.

			//이미 작동중인 Invoke가 있으면 취소하는 구문.
			if( IsInvoking("ResetCombo") )
			{
				CancelInvoke("ResetCombo");
			}
			//콤보 발생이 없을 때 2초 후에 콤보를 초기화한다.
			Invoke ("ResetCombo", 2.0f);
		}
		else
		{
			score += addScore;
		}

		//빨간색 두더지를 클릭 했을 때와 피버모드가 작동중이지 않을 때 작동.
		if( addScore > 0 && haveFeverMode == false)
		{
			//피버 증가.
			fever += 0.05f;
			if( fever >= 1.0f)
			{
				fever = 1.0f;
				haveFeverMode = true; //피버모드 작동.
				feverText.gameObject.SetActive(true);
				//5초후 피버모드 초기화한다.
				Invoke("ResetFever", 5.0f);
			}
			//피버를 화면에 나타나도록 한다.
			feverSlider.value = fever;
		}
		
		//최소값처리.
		if( score < 0)
		{
			score = 0;
		}
		scoreLabel.text = score.ToString();
	}

	void ResetCombo()
	{
		ComboCount = 0; //콤보 초기화.
		comboText.gameObject.SetActive(false); //콤보 라벨 화면에서 나타나지 못하게 한다.
	}

	void ResetFever()
	{
		//피버 초기화.
		fever = 0;
		haveFeverMode = false; 
		feverText.gameObject.SetActive(false);
		feverSlider.value = 0; //피버를 화면에서 빈 칸으로 보이도록 한다.
	}

	void RandomMolySpawn()
	{
		if( nowGameState != GameState.idle )
		{
			return;
		}

		//동시 출현 가능한 두더지 숫자를 랜덤하게 선택.
		molySpawnCount = Random.Range(0, molyAppearCount[listIndex]);
		//동시 출현 가능한 두더지 숫자만큼 반복하여 실행.
		for(int i = 0; i<= molySpawnCount; i++)
		{
			molySpawnRandom = Random.Range(0f, 100f);
			molySelectRandom = Random.Range(0, 16);


			//idle상태의 MolyUnit 선택.
			while( molyUnitList[molySelectRandom].nowMolyState != MolyState.idle )
			{
				molySelectRandom = Random.Range(0, 16);
			}

			//MolyUnit 작동.
			if( molySpawnRandom <= ppoMolyProbability[listIndex])
			{
				//두더지의 스프라이트 타입과 대기 시간을 입력하여 선택된 두더지를 작동시킨다.
				molyUnitList[molySelectRandom].StartUseMoly(SpriteType.Ppo, 
				                                            molyWaitTime[listIndex]);
			}
			else
			{
				molyUnitList[molySelectRandom].StartUseMoly(SpriteType.Ppu, 
				                                            molyWaitTime[listIndex]);
			}
		}
	}

	void RepeatAddListIndex()
	{
		//게임 스테이트 판단하여 idle이 아닌 경우 실행하지 못하도록 한다.
		if( nowGameState != GameState.idle )
		{
			return;
		}

		listIndex++;//listIndex 증가.
		//예외처리:리스트의 마지막 항목이 있는 위치를 지나치면 마지막 항목을 가르키도록 한다.
		if( listIndex >= ppoMolyProbability.Count )
		{
			listIndex = ppoMolyProbability.Count -1;
		}

		//작동중인 Invoke로 RandomMolySpawn이 작동중인지 확인.
		if( IsInvoking("RandomMolySpawn") )
		{
			CancelInvoke("RandomMolySpawn");
		}
		//구간별로 두더지 출현 시간이 반영되도록 한다.
		InvokeRepeating("RandomMolySpawn", 0.01f, molyAppearTime[listIndex]);
	}

	public void PlayMolyHitSound(SpriteType isPpo)
	{
		switch(isPpo)
		{
		case SpriteType.Ppo:
			audioSource.PlayOneShot( ppoHitClip );
			break;
		case SpriteType.Ppu:
			audioSource.PlayOneShot( ppuHitClip );
			break;
		}
	}
}
