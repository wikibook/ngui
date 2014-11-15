using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// XML 사용을 위해서 추가.
using System.Xml; 
using System.Xml.Serialization;

public enum GameState
{
    ready,
    idle,
    gameOver,
    wait,
    loading
}

public partial class GamePlayManager : MonoBehaviour, IDamageable
{
    //게임 상황 판단.
    public GameState nowGameState = GameState.ready; 

    // 생성할 Enemy 게임 오브젝트 리스트
    public List<GameObject> spawnEnemyObjs = new List<GameObject>();
    // 적 생성할 위치 저장.
    List<Vector3> spawnPositions = new List<Vector3>{
        new Vector3(12, 2.7f, 0), new Vector3(12, 0.26f, 0), 
        new Vector3(12, -2.2f, 0), new Vector3(12, -4.7f, 0)};
    // 농장 HP
    float farmCurrentHP = 1;
    float farmLimitHP = 1;
    // 게임 시작 후 경과 시간
    float timeElapsed = 0;
    // 획득한 점수 저장.
    int score = 0;
    // 처치한 적 숫자 기록.
    int deadEnemys = 0;
    // 획득한 코인 숫자 저장.
    int getCoins = 0;

    // 게임 오브젝트 풀에 들어가는 게임 오브젝트의 최초로 생성되는 위치.
    public Transform gameObjectPoolPosition;
    // 게임 오브젝트 풀 딕셔너리.
    Dictionary<string, GameObjectPool> gameObjectPools = 
        new Dictionary<string, GameObjectPool>();

    // 적 생성 데이터 저장.
    List<EnemyWaveData> enemyWaveDatas = new List<EnemyWaveData>();
    int currentEnemyWaveDataIndexNo = 0;
    public UISlider farmHPSlier;
    public UILabel scoreLb;
    public UILabel waveLb;

    // 적 체력 표시 유저 인터페이스 생성에 사용한다.
    public GameObject enemyHPBar;
    public Transform enemyHPBarRoot;
    // 적 체력 표시 유저 인터페이스 할당에 사용한다.
    public UIPanel enemyHPBarPanel;
    public Camera enemyHPBarCam;

    // 코인 프리팹을 등록하는데 사용.
    public GameObject coinObj;
    public UILabel coinLb;

    // 결과창 게임 오브젝트.
    public GameObject resultWindow;
    // 결과창에 사용되는 UILabel
    public UILabel resultHighScoreLb, resultNowScoreLb, 
        resultWaveLb, resultDeadEnemysLb, resultGetCoinsLb;

    //결과창이 나타날 때 적 캐릭터를 정지하는 목적으로 사용된다.
    public event System.Action OnFreeze;

    public GameObject resurrectionEffectObj;
    public UIPlayTween bossEffectObj;

    // 생성할 위치값을 생성할 유닛 수로 치환.
    Dictionary<int, int> positionToAmount = new Dictionary<int, int> {
        { 1, 1}, { 2, 1}, { 4, 1}, { 8, 1},
        { 3, 2}, { 5, 2}, { 6, 2}, { 9, 2}, {10, 2}, {12, 2},
        { 7, 3}, {11, 3}, {13, 3}, {14, 3},
        {15, 4}
    };

    void Awake()
    {
        // 스크립트 연결.
        GameData.Instance.gamePlayManager = this;
    }

    void OnDestroy()
    {
        // 스크립트 연결 해제.
        GameData.Instance.gamePlayManager = null;
    }

    void OnEnable()
    {
        InitGameObjectPools();

        LoadEnemyWaveDataFromXML();

        //
        farmCurrentHP 
            = 300 + GameData.Instance.ConvertUpgradeLvToAddValue(
                GameData.Instance.userdata.defLv);
        farmLimitHP = farmCurrentHP;
    }

    // 적 캐릭터 별로 20개씩 게임 오브젝트를 생성하여 게임 오브젝트 풀에 등록한다.
    void InitGameObjectPools()
    {
        for (int i=0; i<spawnEnemyObjs.Count; i++)
        {
            CreateGameObject(spawnEnemyObjs [i], 20, gameObjectPoolPosition);
        }

        //
        SetupAllEnemyFreeze();

        // 적 체력 표시 유저 인터페이스 생성 및 등록.
        CreateGameObject(enemyHPBar, 20, enemyHPBarRoot, Vector3.one);

        // 적 캐릭터의 dead 애니메이션이 종료된 후 일정 확율로 나타나게될 코인.
        CreateGameObject(coinObj, 20, gameObjectPoolPosition);
    }

    void CreateGameObject(GameObject targetObj,
                          int amount,
                          Transform parent,
                          Vector3 localScale=default(Vector3))
    {
        // 게임 오브젝트 풀 생성.
        GameObjectPool tempGameObjectPool = 
            new GameObjectPool(gameObjectPoolPosition.transform.position.x,
                               targetObj);
        for (int j=0; j<amount; j++)
        {
            // 게임 오브젝트 생성.
            GameObject tempObj = 
                Instantiate(
                            targetObj, 
                            gameObjectPoolPosition.position, 
                            Quaternion.identity
            ) as GameObject;
            tempObj.name = targetObj.name + j;
            tempObj.transform.parent = parent;
            if (localScale != Vector3.zero)
            {
                tempObj.transform.localScale = localScale;
            }
            // 게임 오브젝트를 게임 오브젝트 풀에 등록.
            tempGameObjectPool.AddGameObject(tempObj);
        }
        gameObjectPools.Add(targetObj.name, tempGameObjectPool);
    }

    public void AddScore(int addScore)
    {
        if (nowGameState == GameState.ready
            || nowGameState == GameState.gameOver)
            return;
        score += addScore;

        #if UNITY_EDITOR
        Debug.Log(score);
        #endif

        //획득한 점수를 화면에 표시.
        scoreLb.text = score.ToString();
    }

    public void Damage(float damageTaken)
    {
        if (nowGameState == GameState.gameOver)
            return;
        farmCurrentHP -= damageTaken;

        #if UNITY_EDITOR
        Debug.Log(farmCurrentHP);
        #endif

        if (farmCurrentHP <= 0)
        {
            // 즉시 부활 아이템 사용 여부 판단.
            if(GameData.Instance.useResurrection)
            {
                GameData.Instance.useResurrection = false;
                farmCurrentHP = (farmLimitHP/2);
                // 부활을 알리는 효과 추가.
                resurrectionEffectObj.SetActive(true);
            }
            else
            {
                nowGameState = GameState.gameOver;
                // 결과창 표시.
                OpenResult();
            }
        }
        //농장 체력 표시.
        farmHPSlier.value = farmCurrentHP / farmLimitHP;
    }

    //XML을 읽어서 enemyWaveDatas에 저장한다.
    void LoadEnemyWaveDataFromXML()
    {
        //이미 데이터를 로딩했다면 다시 로딩하지 못하도록 예외처리.
        if (enemyWaveDatas != null && enemyWaveDatas.Count > 0)
            return;

        // XML파일을 읽는다.
        TextAsset xmlText = Resources.Load("EnemyWaveData") as TextAsset;
        // XML 파일을 문서 객체 모델(DOM)로 전환한다.
        XmlDocument xDoc = new XmlDocument();
        xDoc.LoadXml(xmlText.text);
        // XML파일 안에 EnemyWaveData란 XmlNode를 모두 읽어드린다.
        XmlNodeList nodeList = xDoc.DocumentElement.SelectNodes("EnemyWaveData");

        XmlSerializer serializer = new XmlSerializer(typeof(EnemyWaveData));
        // 역질렬화를 통해 EnemyWaveData 구조체로 변경하여 enemyWaveDatas  멤버 필드에 저장한다.
        for (int i=0; i<nodeList.Count; i++)
        {
            EnemyWaveData enemyWaveData = 
                (EnemyWaveData)serializer.Deserialize(new XmlNodeReader(nodeList [i]));
            enemyWaveDatas.Add(enemyWaveData);
        }
    }

    void SpawnEnemy(EnemyWaveData enemyData)
    {
        int positionPointer = 1;
        int shiftPosition = 0;
        // 생성할 위치 값으로 생성할 유닛 수 판단.
        enemyData.amount = positionToAmount[enemyData.spawnPosition];
        //웨이브 표시.
        waveLb.text = enemyData.waveNo.ToString();

        // 생성해야하는 숫자만큼 loop
        for (int i=0; i< enemyData.amount; i++)
        {
            // 생성할 위치 선택.
            while ((positionPointer & enemyData.spawnPosition) < 1)
            {
                shiftPosition++;
                positionPointer = 1 << shiftPosition;
            }
            // 오브젝트 풀에 사용가능한 게임 오브젝트가 있는지 점검.
            GameObject currentSpawnGameObject;
            if (!gameObjectPools [enemyData.type]
               .NextGameObject(out currentSpawnGameObject))
            {
                // 사용가능한 게임 오브젝트가 없다면 생성하여 추가한다.
                currentSpawnGameObject = 
                    Instantiate(
                        gameObjectPools [enemyData.type].spawnObj,
                        gameObjectPoolPosition.transform.position,
                        Quaternion.identity) as GameObject;
                
                currentSpawnGameObject.transform.parent = gameObjectPoolPosition;
                currentSpawnGameObject.name = 
                    enemyData.type + gameObjectPools [enemyData.type].lastIndex;
                gameObjectPools [enemyData.type].AddGameObject(currentSpawnGameObject);
            }
            currentSpawnGameObject.transform.position = 
                spawnPositions [shiftPosition];

            //선택된 적 캐릭터를 초기화하여 작동시킨다.
            currentSpawnGameObject.tag = enemyData.tagName;
            Enemy currentEnemy = currentSpawnGameObject.GetComponent<Enemy>();
            currentEnemy.InitEnemy(enemyData.HP, enemyData.AD, enemyData.MS);
            shiftPosition++;

            // 게임 오브젝트 풀에서 사용가능한 적 체력 표시 인터페이스가 있는지 체크.
            GameObject currentEnemyHPBar;
            if (!gameObjectPools [enemyHPBar.name]
               .NextGameObject(out currentEnemyHPBar))
            {
                // 사용가능한 게임 오브젝트가 없다면 생성하여 추가한다.
                currentEnemyHPBar =
                    Instantiate(
                        enemyHPBar,
                        gameObjectPoolPosition.transform.position,
                        Quaternion.identity) as GameObject;

                currentEnemyHPBar.transform.parent = enemyHPBarRoot;
                currentEnemyHPBar.transform.localScale = Vector3.one;
                currentEnemyHPBar.name = 
                    enemyHPBar.name + gameObjectPools [enemyHPBar.name].lastIndex;
                gameObjectPools [enemyHPBar.name].AddGameObject(currentEnemyHPBar);
            }
            // 적 체력 표시 인터페이스 할당.
            UISlider tempEnemyHPBarSlider = 
                currentEnemyHPBar.GetComponent<UISlider>();
            currentEnemy.InitHPBar(
                tempEnemyHPBarSlider, 
                enemyHPBarPanel, 
                enemyHPBarCam);

            if (enemyData.tagName == "boss")
            {
                // 적 보스 캐릭터가 등장했다는 표시를 띄운다.
                bossEffectObj.gameObject.SetActive(true);
                bossEffectObj.resetOnPlay = true;
                bossEffectObj.Play(true);
            }
        }
    }

    void Update()
    {
        switch (nowGameState)
        {
        case GameState.ready:
            // 게임이 시작되면 3초간 사용자가에게 준비시간을 제공.
            timeElapsed += Time.deltaTime;
            if (timeElapsed >= 3.0f)
            {
                timeElapsed = 0;
                SetupGameStateToIdle();
            }
            break;
        case GameState.wait:
        case GameState.idle:
            // 두 상태 모두 게임이 진행중이므로 경과 시간을 증가시킨다.
            timeElapsed += Time.deltaTime;
            break;
        case GameState.gameOver:
            break;
        }
    }
    
    public void SetupGameStateToIdle()
    {
        // 게임 스테이트를 idle로 변경.
        nowGameState = GameState.idle;
        // 해제되지 못한 Invoke를 해제하고 새롭게 설정.
        if (IsInvoking("CheckSpawnEnemy"))
        {
            CancelInvoke("CheckSpawnEnemy");
        }
        InvokeRepeating("CheckSpawnEnemy", 0.5f, 2.0f);
    }
    
    void CheckSpawnEnemy()
    {
        // idle 상태가 아니라면 더이상 진행되지 못하도록 에러처리.
        if (nowGameState != GameState.idle)
            return;
        
        //적 생성 데이터 전체가 소모되었다면 게임을 종료하도록 한다.
        if (currentEnemyWaveDataIndexNo >= enemyWaveDatas.Count)
        {
            nowGameState = GameState.gameOver;
            CancelInvoke("CheckSpawnEnemy");
            // 결과창 표시.
            OpenResult();
            return;
        }
        // 적을 생성한다.
        SpawnEnemy(enemyWaveDatas [currentEnemyWaveDataIndexNo]);
        // 생성된 적이 boss인 경우 적 생성을 멈춘다.
        if (enemyWaveDatas [currentEnemyWaveDataIndexNo].tagName == "boss")
        {
            nowGameState = GameState.wait;
            CancelInvoke("CheckSpawnEnemy");
        }
        currentEnemyWaveDataIndexNo++;
    }

    // 코인을 생성할 포지션을 spawnPosition 매개 변수로 전달하여 사용.
    public void SpawnCoin(Vector3 spawnPosition)
    {
        GameObject currentCoin;
        if (!gameObjectPools [coinObj.name]
            .NextGameObject(out currentCoin))
        {
            // 사용가능한 게임 오브젝트가 없다면 생성하여 추가한다.
            currentCoin =
                Instantiate(
                    coinObj,
                    gameObjectPoolPosition.transform.position,
                    Quaternion.identity) as GameObject;
            
            currentCoin.transform.parent = enemyHPBarRoot;
            currentCoin.name = 
                coinObj.name + gameObjectPools [coinObj.name].lastIndex;
            gameObjectPools [enemyHPBar.name].AddGameObject(currentCoin);
        }

        Coin coinScript = currentCoin.transform.GetChild(0).GetComponent<Coin>();
        coinScript.StartCoinAnimation(gameObjectPoolPosition.position);

        currentCoin.transform.position = spawnPosition;
    }

    // 처치한 적과 획득한 코인 저장.
    public void AddDeadEnemyAndCoin(int getCoin = 0)
    {
        // 처치한 적 숫자 증가.
        deadEnemys++;
        // 획득한 코인 숫자 증가.
        getCoins += getCoin;
        if(getCoin > 0)
        {
            // 유저 인터페이스에 반영.
            coinLb.text = getCoins.ToString();
        }
    }

    // 결과창을 나타나게 한다.
    public void OpenResult()
    {
        // 재 생성 방지.
        if(resultWindow.activeInHierarchy) return;

        // 적 생성 구문 해제.
        if (IsInvoking("CheckSpawnEnemy"))
        {
            CancelInvoke("CheckSpawnEnemy");
        }

        // 최고 점수를 나타낼 수 있도록 해야한다.
        resultHighScoreLb.text 
            = GameData.Instance.userdata.highScore.ToString();

        resultNowScoreLb.text = score.ToString();
        resultWaveLb.text = waveLb.text;
        resultDeadEnemysLb.text = deadEnemys.ToString();

        // 보너스 골드 계산.
        if(GameData.Instance.userdata.moneyLv > 0)
        {
            int bonusCoins = System.Convert.ToInt32(
                (float)getCoins * ((float)GameData.Instance.userdata.moneyLv/40f));
            resultGetCoinsLb.text 
                = string.Format("{0} (bonus +{1})",getCoins, bonusCoins);
            getCoins += bonusCoins;
        }
        else
        {
            resultGetCoinsLb.text = getCoins.ToString();
        }

        // 결과창을 나타나게 한다.
        resultWindow.SetActive(true);

        // 이벤트에 등록된 메소드가 있는지 체크.
        if(OnFreeze !=null)
        {
            OnFreeze();
        }
    }

    //모든 적 캐릭터의FreezeEnemy 메소드를 OnFreeze에 등록.
    void SetupAllEnemyFreeze()
    {
        int j=0;
        GameObject tempObj;
        Enemy tempEnemyScript;
        for(int i=0; i<spawnEnemyObjs.Count; ++i)
        {
            j=0;
            while(j< gameObjectPools[ spawnEnemyObjs[i].name].lastIndex)
            {
                if(gameObjectPools[ spawnEnemyObjs[i].name].GetObject(j, out tempObj))
                {
                    tempEnemyScript = tempObj.GetComponent<Enemy>();
                    //enemy 스크립트의 FreezeEnemy 메소드를 등록.
                    OnFreeze += tempEnemyScript.FreezeEnemy;
                }
                ++j;
            }
        }
    }
}

public class GameObjectPool
{

    int poolNowIndex = 0;
    int count = 0;
    float spawnPositionX = 0;
    public GameObject spawnObj;
    List<GameObject> pool = new List<GameObject>();

    // 생성자.
    public GameObjectPool(float positionX, GameObject initSpawnObj)
    {
        spawnPositionX = positionX;
        spawnObj = initSpawnObj;
    }
    
    // 게임 오브젝트를 풀에 추가한다.
    public void AddGameObject(GameObject addGameObject)
    {
        pool.Add(addGameObject);
        count++;
    }
    
    // 사용중이지 않은 게임 오브젝트를 선택한다.
    public bool NextGameObject(out GameObject returnObject)
    {
        int startIndexNo = poolNowIndex;
        if( lastIndex == 0 )
        {
            returnObject = default(GameObject);
            return false;
        }

        while (pool[poolNowIndex].transform.position.x < spawnPositionX)
        {
            poolNowIndex++;
            poolNowIndex = (poolNowIndex >= count) ? 0 : poolNowIndex;
            // 사용가능한 게임 오브젝트가 없을 때.
            if (startIndexNo == poolNowIndex)
            {
                returnObject = default(GameObject);
                return false;
            }
        }
        returnObject = pool [poolNowIndex];
        return true;
    }

    public int lastIndex
    {
        get
        {
            return pool.Count;
        }
    }

    // 해당 인덱스의 게임 오브젝트가 존재하는 경우 반환.
    public bool GetObject(int index, out GameObject obj)
    {
        if( lastIndex < index || pool[index] == null)
        {
            obj = default(GameObject);
            return false;
        }
        obj = pool[index];
        return true;
    }
}

[XmlRoot]
public struct EnemyWaveData
{
    [XmlAttribute("waveNo")]
    public int
        waveNo;
    [XmlElement]
    public string
        type;
    [XmlElement]
    public int
        amount;
    [XmlElement]
    public int
        spawnPosition;
    [XmlElement]
    public string
        tagName;
    [XmlElement]
    public float
        MS;
    [XmlElement]
    public float
        AD;
    [XmlElement]
    public float
        HP;
}
