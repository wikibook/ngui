using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public partial class TitleGM : MonoBehaviour {
    
    int loadRetryCounter = 0;
    
    // 사용자 데이터를 로딩.
    public void LoadUserData()
    {
        StartCoroutine(RequestUserData());
    }

    // 인앱 제품 정보 로딩. 
    void LoadInappeData()
    {
        StartCoroutine(
            RequestDataToServer<InappItemData>(
            "getInappItems", 0.15f, "inapp", "InappItemData",
            GameData.Instance.inappItemList,
            LoadPriceData)
            );
    }

    // 가격 정보 로딩.
    void LoadPriceData()
    {
        StartCoroutine(
            RequestDataToServer<PriceData>(
            "getPrices", 0.2f, "price", "PriceData", "codeNo",
            GameData.Instance.priceDic,
            LoadItemData)
            );
    }

    // 보유한 아이템 로딩.
    void LoadItemData()
    {
        StartCoroutine(
            RequestDataToServer<ItemData>(
            "getUserItem", 0.3f, "useritem", "ItemData",
            GameData.Instance.itemList,
            LoadFriendData)
            );
    }
    
    // 친구 데이터 로딩.
    void LoadFriendData()
    {
        StartCoroutine(
            RequestDataToServer<FriendData>(
            "getFriendListWithFB", 0.6f, "friendlist", "FriendData",
            GameData.Instance.friendList,
            LoadMessageData)
            );
    }
    
    // 메시지 데이터 로딩.
    void LoadMessageData()
    {
        StartCoroutine(
            RequestDataToServer<MessageData>(
            "getUserMessage", 0.9f, "usermessage", "MessageData",
            GameData.Instance.messageList,
            GoToLobbyScene)
            );
    }
    
    // 로비씬으로 전환.
    void GoToLobbyScene()
    {
        progressBar.value = 1.0f;
        Application.LoadLevelAsync("LobbyScene");
    }
    
    // 문제가 발생했을 때 고객서비스를 제공하는 페이지로 연결한다.
    void OpenCustomerServicePage()
    {
        Application
            .OpenURL(
                string.Format(
                GameData.Instance.urlPrefix, 
                "postCustomerService") );
    }

    // 데이터를 읽을 때 사용할  www 생성.
    WWW LoadDataFromServer(string targetPage)
    {
        int userKeyNo = PlayerPrefs.GetInt("UserKeyNo");
        
        WWWForm form = new WWWForm();
        form.AddField("userKeyNo", userKeyNo);
        
        string url = string.Format(GameData.Instance.urlPrefix, targetPage);
        WWW www = new WWW(url, form);
        return www;
    }
    
    // 사용자 데이터를 서버로부터  로딩.
    IEnumerator RequestUserData()
    {
        WWW www = LoadDataFromServer("getUserCoreDataWithFacebook");

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
            case "none0":
                if( loadRetryCounter > 2 )
                {
                    // error : 사용자 데이터가 없는 경우.
                    PopupWarningMessage(
                        "사용자 데이터가 없습니다.\n운영자에게 문의하세요.", 
                        OpenCustomerServicePage);
                }
                else
                {
                    // error : 데이터를 다시 로딩하는 경우.
                    PopupWarningMessage(
                        "데이터를 다시 로딩합니다", 
                        LoadUserData);
                    ++loadRetryCounter;
                }
                break;
            default:
                // 프로그레스바 업데이트.
                progressBar.value = 0.15f;
                // xml 형태의 string을 데이터로 전환.
                GameData.Instance.ConvertUserCore(www.text);
                // 다음 데이터를 로딩한다.
                LoadInappeData();
                break;
            }
        }
    }
    
    /// <summary>
    /// 아이템, 친구, 메시지 데이터를 서버로부터 로딩할 때 사용.
    /// </summary>
    /// <param name="targetPage">페이지 이름.</param>
    /// <param name="progressPercent">로딩 진행 퍼센트. 0~1사이의 실수.</param>
    /// <param name="rootNode">xml 루트 노드 이름.</param>
    /// <param name="dataNode">각 데이터를 단위별로 저장하는 노드 이름.</param>
    /// <param name="targetList">데이터를 저장할 리스트.</param>
    /// <param name="nextStep">작업 종료 후 다음 단계로 처리할 메소드명.</param>
    /// <typeparam name="T">ItemData, FriendData 등 데이터를 저장할 구조체.</typeparam>
    IEnumerator RequestDataToServer<T>(
        string targetPage, 
        float progressPercent,
        string rootNode,
        string dataNode,
        List<T> targetList,
        System.Action nextStep = null)
    {
        WWW www = LoadDataFromServer(targetPage);
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
            default:
                // xml 형태의 string 데이터를 변환.
                GameData.Instance.ConvertData(
                    www.text, rootNode, dataNode, targetList);
                
                // 프로그레스바 업데이트.
                progressBar.value = progressPercent;

                //  작업 종료 후 이후 작업을 진행한다.
                if( nextStep != null) nextStep();
                break;
            }
        }
    }

    //  dictionary로 처리되야하는 PriceData 등을 담당.
    IEnumerator RequestDataToServer<T>(
        string targetPage, 
        float progressPercent,
        string rootNode,
        string dataNode,
        string noNode,
        Dictionary<int, T> targetDic,
        System.Action nextStep = null)
    {
        WWW www = LoadDataFromServer(targetPage);
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
            default:
                // xml 형태의 string 데이터를 변환.
                GameData.Instance.ConvertData(
                    www.text, rootNode, dataNode, noNode, targetDic);
                
                // 프로그레스바 업데이트.
                progressBar.value = progressPercent;
                
                //  작업 종료 후 이후 작업을 진행한다.
                if( nextStep != null) nextStep();
                break;
            }
        }
    }
}