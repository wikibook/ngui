using UnityEngine;
using System.Collections;

public partial class LobbyGM : MonoBehaviour {

    public StoreBaseUnit itemUnit;
    public GameObject readyRootObj;


    public void OpenReady()
    {
        if(itemUnit.isReady == false)
        {
            string amountText = "0";
            if(GameData.Instance.itemList.Count>0 &&
               GameData.Instance.itemList.Exists(x=> x.itemNo == 302))
            {
                for(int i=0;i<GameData.Instance.itemList.Count;++i)
                {
                    if(GameData.Instance.itemList[i].itemNo == 302)
                    {
                        amountText = GameData.Instance.itemList[i].amount.ToString();
                        break;
                    }
                }
            }

            itemUnit.Init(302, "purchaseRessuraction",
                          GameData.Instance.priceDic[302].price,
                          amountText);
        }
        readyRootObj.SetActive(true);
    }

    public void CloseReady()
    {
        readyRootObj.SetActive(false);
    }

    public void StartGame()
    {
        loadScreenObj.SetActive(true);

        StartCoroutine(RequestStartGame());
    }

    IEnumerator RequestStartGame()
    {
        string url = string.Format(GameData.Instance.urlPrefix, "checkStartGame");
        WWWForm form = new WWWForm();
        int userKeyNo = PlayerPrefs.GetInt("UserKeyNo");
        form.AddField("userKeyNo", userKeyNo);

        // 즉시 부활 데이터 처리.
        int ressurrection = -1;
        int ressurrectionIndex = -1;
        if(GameData.Instance.itemList.Count>0)
        {
            if(GameData.Instance.itemList.Exists(x=>x.itemNo == 302))
            {
                ressurrectionIndex 
                    = GameData.Instance.itemList.FindIndex(x=>x.itemNo == 302);
                ItemData tempItemData 
                    = GameData.Instance.itemList[ressurrectionIndex];

                ressurrection = tempItemData.use?1:0;
            }
        }
        form.AddField("resurrection", ressurrection);
        
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
                // 하트가 부족한 경우.
                loadScreenObj.SetActive(false);
                PopupDialog(
                    "하트가 부족합니다.\n하트를 구매하시겠습니까?",
                    LobbyGM.DialogType.two,
                    GameData.Instance.store.OpenHeartShop);
                break;
            case "done0":
                // 하트 업데이트.
                responseCode = www.text.Substring(5, 3);
                GameData.Instance.userdata.hearts 
                    = System.Convert.ToInt32(responseCode);

                // 로그인 타임 업데이트.
                responseCode = www.text.Substring(8, 10);
                GameData.Instance.userdata.loginTime 
                    = System.Convert.ToDouble(responseCode);

                // 즉시 부활 아이템 처리.
                if(ressurrection!=-1)
                {
                    responseCode = www.text.Substring(18, 1);
                    ItemData tempResurrection 
                        = GameData.Instance.itemList[ressurrectionIndex];
                    tempResurrection.use = (responseCode=="1");
                    responseCode = www.text.Substring(19);
                    tempResurrection.amount 
                        = System.Convert.ToInt32(responseCode);

                    GameData.Instance.itemList[ressurrectionIndex]
                    = tempResurrection;
                }

                // GameData에 즉시 부활 아이템 사용 여부 업데이트.
                GameData.Instance.useResurrection = (ressurrection==1);

                Application.LoadLevelAsync("PlayScene");
                break;
            }
        }
    }
}
