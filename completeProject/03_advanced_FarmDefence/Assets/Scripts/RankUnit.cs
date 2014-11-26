using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RankUnit : MonoBehaviour {

    // 순위, 이름, 점수, 하트 선물 남은 시간 표시.
    public UILabel rankLabel, nameLabel, scoreLabel, heartTimeLabel;
    // 사용자 이미지.
    public UITexture userImg;
    // 하트 선물 가능을 나타낸다.
    public UISprite fillHeart;
    
    // 친구의 데이터를 저장.
    FriendData fData;

    // 하트 선물 가능 여부 및 시간 계산용.
    bool isPossibleSendHeart = false;
    double remainSendTime;
    int remainSec, remainMin;

    // RankUnit 초기화.
    public void Init(FriendData friendData, int rankNo, bool isMyData)
    {
        // 이미 작동중인 경우 예외처리.
        if(IsInvoking("RefreshRemainHeartSendTime")) 
            CancelInvoke("RefreshRemainHeartSendTime");
        
        // 친구정보 저장.
        fData = friendData;
        
        rankLabel.text = rankNo.ToString();
        nameLabel.text = fData.name;
        scoreLabel.text = fData.score.ToString();

        // 페이스북 ID가 있는지 체크.
        if(fData.facebook.CompareTo("0") != 0)
        {
            //ID가 있는 경우. 프로필 이미지 요청.
            string requestImgQuery 
                = string.Format("/{0}/picture?width=80&height=80", fData.facebook);
            FB.API(requestImgQuery, Facebook.HttpMethod.GET, ProfileImgCallBack);
        }
        else
        {
            // 기본 이미지로 나타나도록 함.
            userImg.mainTexture = GameData.Instance.lobbyGM.normalUserImg;
        }
        
        if(!isMyData)
        {
            fillHeart.transform.parent.gameObject.SetActive(true);
            // 하트 선물 가능여부 체크.
            remainSendTime = GameData.Instance.serverLoadedTime - fData.sendTime;
            if(remainSendTime >= 3600)
            {
                isPossibleSendHeart = true;
                fillHeart.enabled = true;
                heartTimeLabel.enabled = false;
            }
            else
            {
                isPossibleSendHeart = false;
                fillHeart.enabled = false;
                heartTimeLabel.enabled = true;
                // 하트 선물이 가능할 때까지 남은 시간을 계산하여 처리한다.
                if(IsInvoking("RefreshRemainHeartSendTime")) 
                    CancelInvoke("RefreshRemainHeartSendTime");
                RefreshRemainHeartSendTime();
                InvokeRepeating("RefreshRemainHeartSendTime", 1.0f, 1.0f);
            }
        }
        else
        {
            fillHeart.transform.parent.gameObject.SetActive(false);
        }
    }

    // 프로필 이미지를 로딩한 후 실행된다.
    void ProfileImgCallBack(FBResult result)
    {
        if (result.Error != null){
            Debug.Log(result.Error);
            // 에러가 난 경우 기본 이미지로 나타나도록 함.
            userImg.mainTexture = GameData.Instance.lobbyGM.normalUserImg;
            return;
        }
        // 받아온 프로필 이미지 적용.
        userImg.mainTexture = (Texture2D)result.Texture;
    }

    // 하트 선물이 가능한 시간까지 남은 시간을 계산하여 표시.
    void RefreshRemainHeartSendTime()
    {
        remainSendTime = 3600 
            - (GameData.Instance.serverLoadedTime 
               - fData.sendTime);
        if(remainSendTime <= 0)
        {
            if(IsInvoking("RefreshRemainHeartSendTime")) 
                CancelInvoke("RefreshRemainHeartSendTime");
            
            // 시간이 다 된경우 서버에 이를 점검.
            heartTimeLabel.text = "00:00";
            StartCoroutine(CheckPossibleSendHeart());
            return;
        }
        
        remainSec = System.Convert.ToInt32(remainSendTime % 60);
        remainMin = System.Convert.ToInt32((remainSendTime - remainSec)/60);
        heartTimeLabel.text = string.Format("{0:00}:{1:00}", remainMin, remainSec);
    }

    IEnumerator CheckPossibleSendHeart()
    {
        int userKeyNo = PlayerPrefs.GetInt("UserKeyNo");
        
        WWWForm form = new WWWForm();
        form.AddField("userKeyNo", userKeyNo);
        form.AddField("friendUserKeyNo", fData.friend);
        form.AddField("friendTableNo", fData.no);
        
        string url = string.Format(
            GameData.Instance.urlPrefix, 
            "checkPossibleSendHeart");
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
            case "reCal":
                // 서버와 계산이 틀려서 다시 계산을 시작해야하는 경우.
                responseCode = www.text.Substring(5, 10);
                GameData.Instance.serverLoadedTime 
                    = System.Convert.ToDouble(responseCode);
                responseCode = www.text.Substring(15);
                fData.sendTime 
                    = System.Convert.ToDouble(responseCode);
                
                isPossibleSendHeart = false;
                InvokeRepeating("RefreshRemainHeartSendTime", 1.0f, 1.0f);
                break;
            case "done0":
                // 하트 전달이 가능한 경우.
                isPossibleSendHeart = true;
                fillHeart.enabled = true;
                heartTimeLabel.enabled = false;
                break;
            }
        }
    }

    // 하트 선물 버튼 클릭 시 작동.
    public void ClickSendHeart()
    {
        // 여러번 클릭되는 것을 방지.
        if(!isPossibleSendHeart) return;
        isPossibleSendHeart = false;
        //하트 선물 요청.
        StartCoroutine(RequestSendHeart());
    }

    IEnumerator RequestSendHeart()
    {
        int userKeyNo = PlayerPrefs.GetInt("UserKeyNo");
        
        WWWForm form = new WWWForm();
        form.AddField("userKeyNo", userKeyNo);
        form.AddField("userID", GameData.Instance.userdata.name);
        form.AddField("friendUserKeyNo", fData.friend);
        form.AddField("friendTableNo", fData.no);
        
        string url = string.Format(
            GameData.Instance.urlPrefix, 
            "postSendHeart");
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
            case "done0":
                responseCode = www.text.Substring(5, 10);
                fData.sendTime 
                    = System.Convert.ToDouble(responseCode);

                responseCode = www.text.Substring(15);
                GameData.Instance.serverLoadedTime
                    = System.Convert.ToDouble(responseCode);

                remainSendTime = 3600;
                fillHeart.enabled = false;
                heartTimeLabel.enabled = true;
                // 하트 선물이 가능할 때까지 남은 시간을 계산하여 처리한다.
                if(IsInvoking("RefreshRemainHeartSendTime")) 
                    CancelInvoke("RefreshRemainHeartSendTime");
                RefreshRemainHeartSendTime();
                InvokeRepeating("RefreshRemainHeartSendTime", 1.0f, 1.0f);
                break;
            }
        }
    }
}
