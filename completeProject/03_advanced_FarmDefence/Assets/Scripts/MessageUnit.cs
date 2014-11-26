using UnityEngine;
using System.Collections;

public class MessageUnit : MonoBehaviour {

    int messageTableKeyNo = 0, messageTypeNo = 0;
    public UILabel messageLabel;
    public GameObject submitBtnObj, cancelBtnObj;

    /// <summary>
    /// 메시지를 초기화.
    /// </summary>
    /// <param name="keyNo">usermessage테이블 키 번호.</param>
    /// <param name="typeNo">메시지 타입.</param>
    /// <param name="msg">메시지 내용.</param>
    public void Init(int keyNo, int typeNo, string msg)
    {
        messageTableKeyNo = keyNo;
        messageTypeNo = typeNo;

        messageLabel.text = msg;

        submitBtnObj.SetActive(true);
        switch(messageTypeNo)
        {
        case 10 :
            cancelBtnObj.SetActive(true);
            break;
        default:
            cancelBtnObj.SetActive(false);
            break;
        }
    }

    // 수락 버튼 클릭.
    public void ClickSubmit()
    {
        GameData.Instance.lobbyGM.loadScreenObj.SetActive(true);
        // 서버에 요청.
        StartCoroutine(RequestSubmit());
    }

    // 취소 버튼 클릭.  (친구 추가 거부 시 사용)
    public void ClickCancel()
    {
        submitBtnObj.SetActive(false);
        cancelBtnObj.SetActive(false);

        GameData.Instance.lobbyGM.loadScreenObj.SetActive(true);
        StartCoroutine(RequestCancel());
    }

    IEnumerator RequestSubmit()
    {
        int userKeyNo = PlayerPrefs.GetInt("UserKeyNo");
        
        WWWForm form = new WWWForm();
        form.AddField("userKeyNo", userKeyNo);
        form.AddField("msgTableKeyNo", messageTableKeyNo);
        form.AddField("msgTypeNo", messageTypeNo);
        
        string url = string.Format(
            GameData.Instance.urlPrefix, 
            "deleteMessage");
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
            case "none0":
                // 메시지 데이터가 존재하지는 않는 경우.
                // 메시지 삭제, 메시지창 재설정.
                GameData.Instance.msgBox.DeleteMessage(messageTableKeyNo);
                GameData.Instance.msgBox.RefreshMessageBox(true);
                break;
            case "done0":
                // 메시지 처리 완료.
                if(messageTypeNo < 10)
                {
                    responseCode = www.text.Substring(5);
                    GameData.Instance.msgBox.UpdateData(messageTypeNo, responseCode);
                    GameData.Instance.lobbyGM.UpdateCoreData();
                }
                else
                {
                    responseCode = www.text.Substring(5);
                    int friendNo = System.Convert.ToInt32(responseCode);
                    //친구 데이터 업데이트.
                    int friendIndex
                        = GameData.Instance
                            .friendList.FindIndex(x=>x.friend == friendNo);
                    FriendData tempFriendData
                        = GameData.Instance.friendList[friendIndex];
                    tempFriendData.state = 2;
                    GameData.Instance.friendList[friendIndex] = tempFriendData;
                }

                GameData.Instance.msgBox.DeleteMessage(messageTableKeyNo);
                GameData.Instance.msgBox.RefreshMessageBox(true);
                break;
            }
        }
    }

    IEnumerator RequestCancel()
    {
        int userKeyNo = PlayerPrefs.GetInt("UserKeyNo");
        
        WWWForm form = new WWWForm();
        form.AddField("userKeyNo", userKeyNo);
        form.AddField("msgTableKeyNo", messageTableKeyNo);
        form.AddField("msgTypeNo", messageTypeNo);
        
        string url = string.Format(
            GameData.Instance.urlPrefix, 
            "deleteMessageWithOutAction");
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
            case "none0":
                // 메시지 데이터가 존재하지는 않는 경우.
                // 메시지 삭제, 메시지창 재설정.
                GameData.Instance.msgBox.DeleteMessage(messageTableKeyNo);
                GameData.Instance.msgBox.RefreshMessageBox(true);
                break;
            case "done0":
                responseCode = www.text.Substring(5);
                int friendNo = System.Convert.ToInt32(responseCode);
                // 친구 데이터 삭제.
                int friendIndex
                    = GameData.Instance.friendList.FindIndex(x=>x.friend == friendNo);
                GameData.Instance.friendList.RemoveAt(friendIndex);

                GameData.Instance.msgBox.DeleteMessage(messageTableKeyNo);
                GameData.Instance.msgBox.RefreshMessageBox(true);
                break;
            }
        }
    }
}
