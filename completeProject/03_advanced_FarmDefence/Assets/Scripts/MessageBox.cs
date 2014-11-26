using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MessageBox : MonoBehaviour {

    public GameObject subMenuRootObj, friendRootObj, messageRootObj, newMarkObj;
    public UILabel pageLabel;
    // 메시지 유닛을 등록.
    public List<MessageUnit> messageUnits 
        = new List<MessageUnit>();

    Dictionary<int, string> msgType 
    = new Dictionary<int, string>{ 
        {10, "{0}님이 친구 요청을 했습니다"},
        {1, "{0}님이 코인 {1}개를 보냈습니다"},
        {2, "{0}님이 보석 {1}개를 보냈습니다"},
        {3, "{0}님이 하트 {1}개를 보냈습니다"}
    };

    int startIndex, endIndex, tempIndex, prePage, totalPage, nowPageNo = 0;
    string msgContents = "";
    public string findFriendName = "";

    void Awake()
    {
        GameData.Instance.msgBox = this;
    }

    void OnDestroy()
    {
        GameData.Instance.msgBox = null;
    }

    void OnEnable()
    {
        CheckHaveMessage();
    }

    public void OpenMessageBox()
    {
        subMenuRootObj.SetActive(true);
        friendRootObj.SetActive(false);
        messageRootObj.SetActive(true);

        // 페이지 셋업.
        GameData.Instance.lobbyGM.loadScreenObj.SetActive(true);
        PrepareMessageBox(nowPageNo);
        GameData.Instance.lobbyGM.loadScreenObj.SetActive(false);
    }

    public void CloseMessageBox()
    {
        subMenuRootObj.SetActive(false);
    }

    // 메시지 데이터 삭제.
    public void DeleteMessage(int tableKeyNo)
    {
        if( GameData.Instance.messageList
           .Exists(x=>x.no == tableKeyNo) )
        {
            int msgIndex 
                = GameData.Instance.messageList
                    .FindIndex(x=>x.no == tableKeyNo);
            GameData.Instance.messageList.RemoveAt(msgIndex);
        }
    }

    // 메시지 타입에 따른 코인, 보석, 하트 업데이트.
    public void UpdateData(int messageTypeNo, string amount)
    {
        int nowAmount = System.Convert.ToInt32(amount);
        switch(messageTypeNo)
        {
        case 1:
            GameData.Instance.userdata.coins = nowAmount;
            break;
        case 2:
            GameData.Instance.userdata.gems = nowAmount;
            break;
        case 3:
            GameData.Instance.userdata.hearts = nowAmount;
            break;
        }
    }

    void CheckHaveMessage()
    {
        newMarkObj.SetActive(
            GameData.Instance.messageList.Count > 0);
    }

    public void ClickLeft()
    {
        prePage = nowPageNo;
        --nowPageNo;
        if( nowPageNo < 0 )
        {
            nowPageNo = 0;
            return;
        }

        if(prePage == nowPageNo) return; 

        GameData.Instance.lobbyGM.loadScreenObj.SetActive(true);
        PrepareMessageBox(nowPageNo);
        GameData.Instance.lobbyGM.loadScreenObj.SetActive(false);
    }

    public void ClickRight()
    {
        prePage = nowPageNo;
        ++nowPageNo;
        tempIndex = nowPageNo * 4 + 1;
        if( tempIndex > GameData.Instance.messageList.Count)
        {
            --nowPageNo;
            return;
        }

        if(prePage == nowPageNo) return; 

        GameData.Instance.lobbyGM.loadScreenObj.SetActive(true);
        PrepareMessageBox(nowPageNo);
        GameData.Instance.lobbyGM.loadScreenObj.SetActive(false);
    }

    // 메시지 창을 다시 셋팅.
    public void RefreshMessageBox(bool turnOffLoading = false)
    {
        PrepareMessageBox(nowPageNo);
        if(turnOffLoading)
            GameData.Instance.lobbyGM.loadScreenObj.SetActive(false);
    }

    // 전체 메시지 페이지를 알아낸다.
    int GetTotalPage()
    {
        int remain = GameData.Instance.messageList.Count%4;

        totalPage = (GameData.Instance.messageList.Count-remain)/4 
            + ((remain > 0) ? 1 : 0);
        totalPage = totalPage == 0 ? 1 : totalPage;
        return totalPage;
    }

    // 친구 추가 요청 메시지로 전환.
    public void GotoFriendMessage()
    {
        if(GameData.Instance.messageList.Exists(
            x=>x.sender == findFriendName && x.msgType == 10))
        {
            tempIndex = GameData.Instance.messageList.FindIndex(
                x=>x.sender == findFriendName && x.msgType == 10);
            int remain = tempIndex%4;
            int pageNo = (tempIndex-remain)/4 + ((remain>0)?1:0);

            nowPageNo = pageNo;
            OpenMessageBox();
        }
    }

    // 메시지창 셋팅.
    void PrepareMessageBox(int targetPageNo)
    {
        CheckHaveMessage();

        nowPageNo = targetPageNo;
        tempIndex = nowPageNo * 4 + 1;
        if( tempIndex > GameData.Instance.messageList.Count )
        {
            --nowPageNo;
            if(nowPageNo < 0 ) nowPageNo = 0;
        }

        string pageText = string.Format("({0}/{1})", nowPageNo+1, GetTotalPage() );
        pageLabel.text = pageText;

        // 시작과 끝 인덱스 계산.
        startIndex = nowPageNo * 4;
        tempIndex = startIndex + 4;
        endIndex 
            = (tempIndex) <= GameData.Instance.messageList.Count 
                ? tempIndex 
                : GameData.Instance.messageList.Count;

        //메시지 유닛을 초기화.
        int messageIndex = 0;
        for(int i=startIndex; i<endIndex; ++i)
        {
            msgContents 
                = (GameData.Instance.messageList[i].msgType == 10) ?
                    string.Format(msgType[GameData.Instance.messageList[i].msgType], 
                                  GameData.Instance.messageList[i].sender)
                    : string.Format(msgType[GameData.Instance.messageList[i].msgType], 
                                    GameData.Instance.messageList[i].sender,
                                    GameData.Instance.messageList[i].amount);
            messageUnits[messageIndex].Init(
                GameData.Instance.messageList[i].no,
                GameData.Instance.messageList[i].msgType, 
                msgContents);
            messageUnits[messageIndex].gameObject.SetActive(true);
            ++messageIndex;
        }

        // 필요없는 메시지 유닛은 화면에 나타나지 않도록 처리.
        if(endIndex < tempIndex)
        {
            for(int i=endIndex;i<tempIndex;++i)
            {
                messageUnits[messageIndex].gameObject.SetActive(false);
                ++messageIndex;
            }
        }
    }
}
