using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class FindFriend : MonoBehaviour {
    
    public GameObject subMenuRootObj, friendRootObj, messageRootObj;
    public UILabel inputLabel;
    
    public void OpenFindFriend()
    {
        // 추천 친구 요청.
        GameData.Instance.lobbyGM.PopupDialog(
            "추천 친구 찾는 중…",
            LobbyGM.DialogType.none);

        StartCoroutine(RequestRecommendedFriend());

        subMenuRootObj.SetActive(true);
        friendRootObj.SetActive(true);
        messageRootObj.SetActive(false);
    }
    
    public void CloseFindFriend()
    {
        subMenuRootObj.SetActive(false);
    }
    
    // 아이디를 통해서 친구를 찾는다.
    public void ClickInputID()
    {
        // 아이디 길이를 체크한다.
        string inputID = inputLabel.text; 
        if( !(inputID.Length >= 3 && inputID.Length <= 14) )
        {
            GameData.Instance.lobbyGM.PopupDialog(
                "아이디는 3~14글자로 입력해야합니다", 
                LobbyGM.DialogType.one);
            return;
        }
        
        if(inputID == GameData.Instance.userdata.name)
        {
            GameData.Instance.lobbyGM.PopupDialog(
                "자신의 아이디입니다. \n다른 아이디를 입력하세요.",
                LobbyGM.DialogType.one);
            return;
        }
        
        GameData.Instance.lobbyGM.PopupDialog(
            "친구 아이디 찾는 중… ", 
            LobbyGM.DialogType.none);
        
        // 서버에 아이디를 전달한다.
        StartCoroutine( RequestFindFriend() );
    }
    
    IEnumerator RequestFindFriend()
    {
        WWWForm form = new WWWForm();
        int userKeyNo = PlayerPrefs.GetInt("UserKeyNo");
        form.AddField("userKeyNo", userKeyNo);
        form.AddField("userID", GameData.Instance.userdata.name);
        form.AddField("friendID", inputLabel.text);
        
        string url = string.Format(GameData.Instance.urlPrefix, "postFriend");
        WWW www = new WWW(url, form);
        
        yield return www;
        
        if( www.isDone && www.error == null)
        {
            // 전달받은 데이터의 앞 5글자를 분리하여 결과 코드로 분석.
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
                // 해당 아이디가 없는 경우.
                GameData.Instance.lobbyGM.PopupDialog(
                    "존재하지 않는 아이디입니다.",
                    LobbyGM.DialogType.one);
                break;
            case "have0":
                // 이미 친구 요청을 보낸 경우.
                GameData.Instance.lobbyGM.PopupDialog(
                    "이미 친구 요청을 하셨습니다.",
                    LobbyGM.DialogType.one);
                break;
            case "have1":
                // 이미 친구 요청을 받은 경우.
                // 메시지를 바로 확인할 수 있는 메소드 등록.
                GameData.Instance.msgBox.findFriendName = inputLabel.text;
                GameData.Instance.lobbyGM.PopupDialog(
                    "친구 요청을 이미 받은 상태입니다. 메시지를 확인하시겠습니까? ",
                    LobbyGM.DialogType.two, 
                    GameData.Instance.msgBox.GotoFriendMessage);  
                break;
            case "have2":
                // 이미 친구 인 경우.
                GameData.Instance.lobbyGM.PopupDialog(
                    "친구로 등록된 아이디입니다.\n 다른 아이디를 입력해주세요.",
                    LobbyGM.DialogType.one);
                break;
            case "done0":
                string msg = string.Format("{0}님께 요청을 보냈습니다", inputLabel.text);
                GameData.Instance.lobbyGM.PopupDialog(
                        msg, LobbyGM.DialogType.one);
                    break;
            }
        }
    }


    #region Recommended Friend
    public List<UILabel> recommendedLabels = new List<UILabel>();

    List<string> recommendedDatas = new List<string>();
    XmlDocument xDoc = new XmlDocument();

    public void ClickRecommended01()
    {
        SendFriend(0);
    }

    public void ClickRecommended02()
    {
        SendFriend(1);
    }

    public void ClickRecommended03()
    {
        SendFriend(2);
    }

    void SendFriend(int index)
    {
        inputLabel.text = recommendedDatas[index];

        StartCoroutine(RequestFindFriend());
    }

    // 추천 친구를 서버에 요청.
    IEnumerator RequestRecommendedFriend()
    {
        WWWForm form = new WWWForm();
        int userKeyNo = PlayerPrefs.GetInt("UserKeyNo");
        form.AddField("userKeyNo", userKeyNo);

        string url 
            = string.Format(GameData.Instance.urlPrefix, 
                            "getRecommendedFriend");
        WWW www = new WWW(url, form);

        yield return www;

        if( www.isDone && www.error == null)
        {

            // 전달받은 데이터의 앞 5글자를 분리하여 결과 코드로 분석.
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
                recommendedDatas.Clear();
                xDoc.LoadXml(www.text);
                XmlNodeList nodeList
                    = xDoc.DocumentElement.SelectSingleNode("recommendFriend")
                        .SelectNodes("RecommendedData");

                // 항목이 존재할때만 처리.
                if( nodeList.Count > 0 )
                {
                    for(int i=0; i<nodeList.Count;++i)
                    {
                        recommendedDatas.Add(nodeList[i].InnerText);
                    }

                    for(int i=0; i<recommendedDatas.Count; ++i)
                    {
                        recommendedLabels[i].transform.parent
                            .gameObject.SetActive(true);
                        recommendedLabels[i].text = recommendedDatas[i];
                    }

                    if( recommendedDatas.Count < 3 )
                    {
                        for(int i= recommendedDatas.Count;i<3;++i)
                        {
                            recommendedLabels[i].transform.parent
                                .gameObject.SetActive(false);
                        }
                    }
                }
                else
                {
                    for(int i=0; i<3; ++i)
                    {
                        recommendedLabels[i].transform.parent
                            .gameObject.SetActive(false);
                    }
                }
                GameData.Instance.lobbyGM.ClickCloseDialog();
                break;
            }
        }
    }
    #endregion
}