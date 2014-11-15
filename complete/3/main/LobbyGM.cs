using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public partial class LobbyGM : MonoBehaviour {
    
    // 사용자 기본 데이터용 라벨.
    public UILabel gems_mainLabel, gems_subLabel, 
    coin_mainLabel, coin_subLabel, heartsLabel, 
    attLvLabel, defLvLabel;
    // 하트 표현용 게임 오브젝트.
    public List<GameObject> heartObjs = new List<GameObject>();

    // 데이터 로딩 시 나타나게할 게임 오브젝트.
    public GameObject loadScreenObj;

    // 페이스북 사용자가 아닌 경우 사용될 기본 이미지.
    public Texture normalUserImg;

    void Awake()
    {
        GameData.Instance.lobbyGM = this;
    }

    void OnDestory()
    {
        GameData.Instance.lobbyGM = null;
    }

    void OnEnable()
    {
        if(GameData.Instance.userdata.facebook.CompareTo("0") != 0 
           && !FB.IsLoggedIn) 
        {
            FB.Login(callback:LobbyInit);
        }
        else
        {
            LobbyInit(null);
        }

    }

    void LobbyInit(FBResult result)
    {
        UpdateCoreData();
        MakeFriendRank();
        
        if(GameData.Instance.isPrepareGame)
        {
            OpenReady();
        }
    }

    #region UpdateCoreData
    // 사용자 기본 데이터를 인터페이스에 반영.
    public void UpdateCoreData()
    {
        gems_mainLabel.text = GameData.Instance.userdata.gems.ToString();
        gems_subLabel.text = gems_mainLabel.text;
        
        coin_mainLabel.text = GameData.Instance.userdata.coins.ToString();
        coin_subLabel.text = coin_mainLabel.text;
        
        // 하트 표시.
        UpdateHearts();
        
        attLvLabel.text = GameData.Instance.userdata.attLv.ToString();
        defLvLabel.text = GameData.Instance.userdata.defLv.ToString();
    }
    
    // 하트 표시에 사용.
    public void UpdateHearts()
    {
        // 하트가 5개보다 많은 경우.
        if(GameData.Instance.userdata.hearts >= 5)
        {
            if(IsInvoking("RefreshRemainHeartFillTime")) 
                CancelInvoke("RefreshRemainHeartFillTime");

            for(int i=0; i<5;++i)
            {
                heartObjs[i].SetActive(true);
            }
            int remainHearts = GameData.Instance.userdata.hearts - 5;
            heartsLabel.text = string.Format("+{0}", remainHearts);
        }
        // 하트가 5개보다 적은 경우.
        else
        {
            for(int i=0; i<GameData.Instance.userdata.hearts;++i)
            {
                heartObjs[i].SetActive(true);
            }
            int startPos = (GameData.Instance.userdata.hearts<0) 
                ? 0 : GameData.Instance.userdata.hearts;
            
            for(int i=startPos; i<5; ++i)
            {
                heartObjs[i].SetActive(false);
            }
            
            if(IsInvoking("RefreshRemainHeartFillTime")) 
                CancelInvoke("RefreshRemainHeartFillTime");
            RefreshRemainHeartFillTime();
            InvokeRepeating("RefreshRemainHeartFillTime", 1.0f, 1.0f);
        }
    }
    
    double remainFillTime = 0;
    int remainSec, remainMin;
    // 다음 하트가 충전될때까지의 시간 표현.
    void RefreshRemainHeartFillTime()
    {
        remainFillTime = 600 
            - (GameData.Instance.serverLoadedTime 
               - GameData.Instance.userdata.loginTime);
        if(remainFillTime <= 0)
        {
            if(IsInvoking("RefreshRemainHeartFillTime")) 
                CancelInvoke("RefreshRemainHeartFillTime");
            
            // 하트가 충전될 시간이 되었으므로 서버와 통신하여 최신화.
            heartsLabel.text = "00:00";
            StartCoroutine(RequestFillHeart());
            return;
        }
        
        remainSec = System.Convert.ToInt32(remainFillTime % 60);
        remainMin = System.Convert.ToInt32((remainFillTime - remainSec)/60);
        heartsLabel.text = string.Format("{0:00}:{1:00}", remainMin, remainSec);
    }
    
    // 서버를 통해서 하트가 새로 충전된 것을 확인.
    IEnumerator RequestFillHeart()
    {
        int userKeyNo = PlayerPrefs.GetInt("UserKeyNo");
        
        WWWForm form = new WWWForm();
        form.AddField("userKeyNo", userKeyNo);
        
        string url = string.Format(GameData.Instance.urlPrefix, "getFillHeart");
        WWW www = new WWW(url, form);
        yield return www;
        
        if( www.isDone && www.error == null)
        {
            string responseCode = www.text.Substring(0, 5);
            switch(responseCode)
            {
            case "query":
                // 서버에서 SQL 쿼리 에러가 발생한 경우.
                #if UNITY_EDITOR
                Debug.Log(www.text);
                #endif
                break;
            default :
                System.Xml.XmlDocument xDoc = new System.Xml.XmlDocument();
                xDoc.LoadXml(www.text);
                
                System.Xml.XmlNode xNode 
                    = xDoc.DocumentElement.SelectSingleNode("result");
                
                GameData.Instance.userdata.hearts 
                    = System.Convert.ToInt32(xNode["hearts"].InnerText);
                GameData.Instance.userdata.loginTime
                    = System.Convert.ToDouble(xNode["loginTime"].InnerText);
                GameData.Instance.serverLoadedTime
                    = System.Convert.ToDouble(xNode["serverTime"].InnerText);
                
                UpdateHearts();
                break;
            }
        }
    }
    #endregion

    #region Rank
    List<RankUnit> rankList = new List<RankUnit>();
    List<FriendData> tempFriendList = new List<FriendData>();
    //
    public Transform rankAddRoot;
    //
    public GameObject rankUnitObj;

    int preScore = 0;

    // RankUnit 처리하여 순위 생성.
    void MakeFriendRank()
    {
        // 점수에 따라서 정렬하기 전에 리스트 초기화.
        tempFriendList.Clear();
        // 친구 목록만 선택.
        tempFriendList = GameData.Instance.friendList.FindAll(x => x.state==2);
        // 사용자 데이터도 친구로 등록.
        FriendData myData = new FriendData();
        myData.score = GameData.Instance.userdata.highScore;
        myData.state = -1;
        myData.name = GameData.Instance.userdata.name;
        //
        myData.facebook = GameData.Instance.userdata.facebook;
        tempFriendList.Add(myData);
        
        // 점수에 따라 정렬.
        tempFriendList.Sort(
            (FriendData x, FriendData y) => y.score.CompareTo(x.score));
        // 새로운 RankUnit 추가.
        AddNewRankUnit(rankList.Count, tempFriendList.Count);

//        int unitCounter = tempFriendList.Count - rankList.Count;
//
//        if(unitCounter > 0)
//        {
//            SetupRankUnit(rankList.Count, 1);
//            //add
//            AddNewRankUnit(rankList.Count, tempFriendList.Count);
//        }
//        else if(unitCounter < 0)
//        {
//            SetupRankUnit(tempFriendList.Count, 1);
//            //remove
//            rankList.RemoveRange(tempFriendList.Count, 
//                                 rankList.Count-tempFriendList.Count);
//        }
//        else if( rankList.Count > 0 )
//        {
//            SetupRankUnit(rankList.Count, 1);
//        }
//        else
//        {
//            AddNewRankUnit(0, tempFriendList.Count);
//        }
    }

    // 새로운 RankUnit 생성하여 추가.
    void AddNewRankUnit(int startIndex, int endIndex)
    {
        int rank = startIndex+1;
        int continueRankNo = rank;
        for(int i=startIndex;i<endIndex;++i)
        {
            // 프리팹 생성.
            GameObject makeRankUnit
                = Instantiate(rankUnitObj, 
                              rankAddRoot.position, 
                              Quaternion.identity) as GameObject;
            makeRankUnit.gameObject.name 
                = (tempFriendList[i].state == 2) ?
                    string.Format("Rank{0:000}", i+1) : "Rank000";
            makeRankUnit.transform.parent = rankAddRoot;
            makeRankUnit.transform.localScale = Vector3.one;
            makeRankUnit.transform.localPosition = Vector3.zero;
            
            // 점수가 같다면 순위 유지.
            rank = (preScore!=tempFriendList[i].score) ? continueRankNo : rank;
            preScore = tempFriendList[i].score;
            ++continueRankNo;
            
            // RankUnit 초기화.
            RankUnit rankUnit = makeRankUnit.GetComponent<RankUnit>();
            rankUnit.Init(tempFriendList[i], rank, (tempFriendList[i].state == -1));
            rankList.Add(rankUnit);
        }
    }

    void SetupRankUnit(int endIndex, int rankStart)
    {
        int rank = rankStart;
        int continueRankNo = rank;
        for(int i=0;i<endIndex;++i)
        {
            rankList[i].gameObject.name = string.Format("Rank{0:000}", i+1);

            rank = (preScore!=tempFriendList[i].score) ? continueRankNo : rank;
            preScore = tempFriendList[i].score;
            ++continueRankNo;

            rankList[i].Init(GameData.Instance.friendList[i], rank, (tempFriendList[i].state == -1));
        }
    }
    #endregion
}