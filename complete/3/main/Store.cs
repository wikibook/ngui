using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class Store : MonoBehaviour {

    // 루트 게임 오브젝트.
    public GameObject rootObj, mainStoreRootObj, heartStoreRootObj;
    public UIToggle upgradeToggle, gemsToggle, coinToggle;

    // 상점에서 사용할 StoreBaseUnit
    public List<StoreBaseUnit> upgradeUnits = new List<StoreBaseUnit>();
    public List<StoreBaseUnit> gemUnits = new List<StoreBaseUnit>();
    public List<StoreBaseUnit> coinUnits = new List<StoreBaseUnit>();

    public StoreBaseUnit heartUnit;

    int forceOpenShop = -1;
    int tempInt, unitPointer=0;
    string url, tempStr;
    XmlDocument xDoc = new XmlDocument();

    void Awake()
    {
        GameData.Instance.store = this;
    }

    void OnDestory()
    {
        GameData.Instance.store = null;
    }

    void OnEnable()
    {
        Invoke("InitGoogleIAB", 1.0f);
    }

    void InitGoogleIAB()
    {
        GameData.Instance.googleIAB.PurchaseCompletedUnmanagedAction 
            = PurchaseCompletedUnmanagedItem;

        GameData.Instance.googleIAB.Init();
    }

    public void PurchaseCompletedUnmanagedItem(string result)
    {
        GameData.Instance.userdata.gems 
            = System.Convert.ToInt32(result);

        GameData.Instance.lobbyGM.UpdateCoreData();

        GameData.Instance.lobbyGM.PopupDialog(
            "보석이 구매되었습니다",
            LobbyGM.DialogType.one);
    }

    public void OpenStore()
    {
        if( rootObj.activeInHierarchy == false)
        {
            rootObj.SetActive(true);
            mainStoreRootObj.SetActive(true);
            heartStoreRootObj.SetActive(false);
        }
    }

    public void CloseStore()
    {
        rootObj.SetActive(false);
    }

    public void OpenCoinShop()
    {
        forceOpenShop = 2;
        OpenStore(2);
    }
    
    public void OpenGemShop()
    {
        forceOpenShop = 1;
        OpenStore(1);
    }

    public void OpenHeartShop()
    {
        rootObj.SetActive(true);
        mainStoreRootObj.SetActive(false);
        heartStoreRootObj.SetActive(true);

        if(heartUnit.isReady == false)
        {
            heartUnit.Init(301, "purchaseHearts",
                           GameData.Instance.priceDic[301].price,
                           GameData.Instance.priceDic[301].amount);
        }
    }

    void OpenStore(int targetPart)
    {
        OpenStore();

        switch(targetPart)
        {
        default:
            upgradeToggle.value = true;
            break;
        case 1:
            gemsToggle.value = true;
            break;
        case 2:
            coinToggle.value = true;
            break;
        }
    }

    public void ChangeToUpgrade()
    {
        if(upgradeToggle.value)
        {
            ChangeShop(0, 1000, 
                       GameData.Instance.userdata.attLv,
                       "upgradeAtt");
            ChangeShop(1, 2000, 
                       GameData.Instance.userdata.defLv,
                       "upgradeDef");
            ChangeShop(2, 3000, 
                       GameData.Instance.userdata.moneyLv,
                       "upgradeMoney");
            CheckForceOpenShop();
        }
    }

    public void ChangeToGems()
    {
        if(gemsToggle.value)
        {
            ChangeShop(gemUnits, 101, "");
            CheckForceOpenShop();
        }
    }

    public void ChangeToCoins()
    {
        if(coinToggle.value)
        {
            ChangeShop(coinUnits, 201, "purchaseCoins");
            CheckForceOpenShop();
        }
    }

    void CheckForceOpenShop()
    {
        if(forceOpenShop > -1)
        {
            switch(forceOpenShop)
            {              
            default:
                forceOpenShop = -1;
                upgradeToggle.value = true;
                break;
            case 1:
                forceOpenShop = -1;
                gemsToggle.value = true;
                break;
            case 2:
                forceOpenShop = -1;
                coinToggle.value = true;
                break;
            }
        }
    }

    void ChangeShop(int targetNo, int startNo, int nowLv, string targetUrl)
    {
        unitPointer = startNo + nowLv;
        upgradeUnits[targetNo].Init(unitPointer, targetUrl,
                                    GameData.Instance.priceDic[unitPointer].price,
                                    nowLv.ToString());
    }

    void ChangeShop(List<StoreBaseUnit> targetList, int startNo, string targetUrl)
    {
        if( !targetList[0].isReady )
        {
            for(int i=0;i<targetList.Count;++i)
            {
                unitPointer = startNo+i;
                tempStr 
                    = string.Format("bonus +{0}%", 
                                    GameData.Instance.priceDic[unitPointer].bonus);
                targetList[i].Init(unitPointer, targetUrl, 
                                 GameData.Instance.priceDic[unitPointer].price, 
                                 GameData.Instance.priceDic[unitPointer].amount,
                                 tempStr);
            }
        }
    }

    // 코인, 보석, 하트 데이터를 usercore에 반영.
    public void ConvertXmlToUserCoreData(string xmlData)
    {
        xDoc.LoadXml(xmlData);

        XmlElement element = xDoc.DocumentElement;

        if(element.SelectSingleNode("gems") != null)
        {
            GameData.Instance.userdata.gems
                = System.Convert.ToInt32(element.SelectSingleNode("gems").InnerText);
        }
        if(element.SelectSingleNode("coins") != null)
        {
            GameData.Instance.userdata.coins
                = System.Convert.ToInt32(element.SelectSingleNode("coins").InnerText);
        }
        if(element.SelectSingleNode("hearts") != null)
        {
            GameData.Instance.userdata.hearts
                = System.Convert.ToInt32(element.SelectSingleNode("hearts").InnerText);
        }
        GameData.Instance.lobbyGM.UpdateCoreData();
    }

    // 업그레이드 정보를 usercore에 반영.
    public void ConvertXmlToUpgradeData(string xmlData)
    {
        xDoc.LoadXml(xmlData);
        
        XmlElement element = xDoc.DocumentElement;

        if(element.SelectSingleNode("gems") != null)
        {
            GameData.Instance.userdata.gems
                = System.Convert.ToInt32(element.SelectSingleNode("gems").InnerText);
        }
        if(element.SelectSingleNode("coins") != null)
        {
            GameData.Instance.userdata.coins
                = System.Convert.ToInt32(element.SelectSingleNode("coins").InnerText);
        }
        
        if(element.SelectSingleNode("attLv") != null)
        {
            GameData.Instance.userdata.attLv
                = System.Convert.ToInt32(element.SelectSingleNode("attLv").InnerText);
        }
        if(element.SelectSingleNode("defLv") != null)
        {
            GameData.Instance.userdata.defLv
                = System.Convert.ToInt32(element.SelectSingleNode("defLv").InnerText);
        }
        if(element.SelectSingleNode("moneyLv") != null)
        {
            GameData.Instance.userdata.moneyLv
                = System.Convert
                    .ToInt32(element.SelectSingleNode("moneyLv").InnerText);
        }
        GameData.Instance.lobbyGM.UpdateCoreData();
    }

    // Item 구매 정보를 통해서 보석과 코인 업데이트.
    public int ConvertXmlToItemAmount(string xmlData)
    {
        xDoc.LoadXml(xmlData);
        
        XmlElement element = xDoc.DocumentElement;
        
        if(element.SelectSingleNode("gems") != null)
        {
            GameData.Instance.userdata.gems
                = System.Convert.ToInt32(element.SelectSingleNode("gems").InnerText);
        }
        if(element.SelectSingleNode("coins") != null)
        {
            GameData.Instance.userdata.coins
                = System.Convert.ToInt32(element.SelectSingleNode("coins").InnerText);
        }

        GameData.Instance.lobbyGM.UpdateCoreData();

        int resultAmount = System.Convert
            .ToInt32(element.SelectSingleNode("amount").InnerText);

        return resultAmount;
    }

}
