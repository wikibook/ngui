using UnityEngine;
using System.Collections;

public class StoreUpgradeUnit : StoreBaseUnit {

    // 최대 레벨 제한.
    bool CheckMaxLv()
    {
        if(productID > 3000)
        {
            if(GameData.Instance.userdata.moneyLv >= 40) return false;
        }
        else if(productID > 2000)
        {
            if(GameData.Instance.userdata.defLv >= 40) return false;
        }
        else
        {
            if(GameData.Instance.userdata.attLv >= 40) return false;
        }

        return true;
    }

    public override void ClickPurchase()
    {
        if( CheckMaxLv() == false) 
        {
            GameData.Instance.lobbyGM.PopupDialog(
                "최고 레벨 달성\n더이상 업그레이드 할 수 없습니다", 
                LobbyGM.DialogType.one);
            return;
        }

        if(nowState != StoreUnitState.ready) return;
        nowState = StoreUnitState.wait;
        GameData.Instance.lobbyGM.loadScreenObj.SetActive(true);
        StartCoroutine(RequestUpgrade());
    }

    protected IEnumerator RequestUpgrade()
    {
        WWWForm form = new WWWForm();
        int userKeyNo = PlayerPrefs.GetInt("UserKeyNo");
        form.AddField("userKeyNo", userKeyNo);
        form.AddField("pID", productID);
        
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
            case "none1":
                // 코인이 부족한 경우.
                nowState = StoreUnitState.ready;
                GameData.Instance.lobbyGM.loadScreenObj.SetActive(false);
                GameData.Instance.lobbyGM.PopupDialog(
                    "코인이 부족합니다.\n코인을 구매하시겠습니까?", 
                    LobbyGM.DialogType.two,
                    GameData.Instance.store.OpenCoinShop);
                break;
            case "none2":
                // 보석이 부족한 경우.
                nowState = StoreUnitState.ready;
                GameData.Instance.lobbyGM.loadScreenObj.SetActive(false);
                GameData.Instance.lobbyGM.PopupDialog(
                    "보석이 부족합니다.\n보석을 구매하시겠습니까?", 
                    LobbyGM.DialogType.two,
                    GameData.Instance.store.OpenGemShop);
                break;
            default:
                // 처리 완료.
                nowState = StoreUnitState.ready;
                GameData.Instance.lobbyGM.loadScreenObj.SetActive(false);
                GameData.Instance.lobbyGM.PopupDialog(
                    "업그레이드가 완료되었습니다.", 
                    LobbyGM.DialogType.one);
                
                GameData.Instance.store.ConvertXmlToUpgradeData(www.text);

                // 상향된 업그레이드 반영.
                string nowLv = "1";
                if(productID>3000)
                {
                    productID = 3000 + GameData.Instance.userdata.moneyLv;
                    nowLv = GameData.Instance.userdata.moneyLv.ToString();
                }
                else if(productID>2000)
                {
                    productID = 2000 + GameData.Instance.userdata.defLv;
                    nowLv = GameData.Instance.userdata.defLv.ToString();
                }
                else
                {
                    productID = 1000 + GameData.Instance.userdata.attLv;
                    nowLv = GameData.Instance.userdata.attLv.ToString();
                }
                UpdateLabels(GameData.Instance.priceDic[productID].price, nowLv);
                break;
            }
        }
    }
}
