using UnityEngine;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

// sealed 한정자를 통해서 해당 클래스가 상속이 불가능하도록 조치.
public sealed class GameData
{
	// 싱글톤 인스턴스를 저장.
	private static volatile GameData uniqueInstance;
	private static object _lock = new System.Object();
	
	// 생성자.
	private GameData() {}
	
	// 외부에서 접근할 수 있도록 함.
	public static GameData Instance
	{
		get
		{
			if (uniqueInstance == null)
			{
				// lock으로 지정된 블록안의 코드를 하나의 쓰레드만 접근하도록 한다.
				lock (_lock)
				{
					if (uniqueInstance == null)
						uniqueInstance = new GameData();
				}
			}
			
			return uniqueInstance ;
		}
	}


	public GamePlayManager gamePlayManager;
    public bool isPrepareGame = false;
    public bool useResurrection = false;

    public LobbyGM lobbyGM;
    public MessageBox msgBox;
    public Store store;
    public GoogleIAB googleIAB;

    // 데이터 요청 경로 등록.
    public string urlPrefix = "http://192.168.0.12/farmdefence/{0}.php";

    //  각 데이터 처리에 사용.
    public UserData userdata = new UserData();
    public List<ItemData> itemList = new List<ItemData>();
    public List<FriendData> friendList = new List<FriendData>();
    public List<MessageData> messageList = new List<MessageData>();
    public Dictionary<int, PriceData> priceDic = new Dictionary<int, PriceData>();
    public List<InappItemData> inappItemList = new List<InappItemData>();

    double nowTime;
    System.DateTime origin 
        = new System.DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Local);
    System.TimeSpan diff;

    public float targetWidth = 0, targetHeight = 640f;
    
    /// <summary>
    /// 서버에서 반환된 기준 시간.
    /// </summary>
    public double serverLoadedTime
    {
        get
        {
            return guessServerNowTime - nowTime;
        }
        set
        {
            nowTime = guessServerNowTime - value;
        }
    }

    public double guessServerNowTime
    {
        get
        {
            diff = System.DateTime.Now - origin;
            return System.Math.Floor(diff.TotalSeconds);
        }
    }

    #region Convert Xml to Data
    XmlDocument xDoc = new XmlDocument();
    
    public void ConvertUserCore(string xmlString)
    {
        xDoc.LoadXml(xmlString);
        XmlNode mainNode = xDoc.DocumentElement.SelectSingleNode("usercore");
        
        userdata.name = mainNode["ID"].InnerText;
        userdata.gems 
            = System.Convert.ToInt32(mainNode["gems"].InnerText);
        userdata.coins
            = System.Convert.ToInt32(mainNode["coins"].InnerText);
        userdata.hearts
            = System.Convert.ToInt32(mainNode["hearts"].InnerText);
        userdata.highScore
            = System.Convert.ToInt32(mainNode["highScore"].InnerText);
        userdata.upgradeNo
            = System.Convert.ToInt32(mainNode["upgradeNo"].InnerText);
        userdata.attLv
            = System.Convert.ToInt32(mainNode["attLv"].InnerText);
        userdata.defLv
            = System.Convert.ToInt32(mainNode["defLv"].InnerText);
        userdata.moneyLv
            = System.Convert.ToInt32(mainNode["moneyLv"].InnerText);
        userdata.loginTime
            = System.Convert.ToDouble(mainNode["loginTime"].InnerText);
        serverLoadedTime 
            = System.Convert.ToDouble(mainNode["serverTime"].InnerText);
        userdata.facebook = mainNode["facebook"].InnerText; //facebook id
    }
    
    // xml 데이터를 형식매개변수 T 클래스로 변환하여 targetList에 넣는다.
    public void ConvertData<T>(string xmlString, 
                               string rootNode, 
                               string dataNode, 
                               List<T> targetList)
    {
        xDoc.LoadXml(xmlString);
        
        XmlNodeList nodeList 
            = xDoc.DocumentElement.SelectSingleNode(rootNode).SelectNodes(dataNode);
        
        // 리스트 초기화.
        targetList.Clear();
        // 항목이 존재할때만 처리.
        if( nodeList.Count > 0 )
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            for(int i=0; i<nodeList.Count;++i)
            {
                T cData 
                    = (T)serializer.Deserialize(new XmlNodeReader(nodeList[i]));
                targetList.Add(cData);
            }
        }
    }

    public void ConvertData<T>(string xmlString,
                                string rootNode,
                                string dataNode,
                                string noNode,
                                Dictionary<int, T> targetDic)
    {
        xDoc.LoadXml(xmlString);
        
        XmlNodeList nodeList 
            = xDoc.DocumentElement.SelectSingleNode(rootNode).SelectNodes(dataNode);
        
        // 초기화.
        targetDic.Clear();
        // 항목이 존재할때만 처리.
        if( nodeList.Count > 0 )
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            for(int i=0; i<nodeList.Count;++i)
            {
                T cData 
                    = (T)serializer.Deserialize(new XmlNodeReader(nodeList[i]));
                int key = System.Convert.ToInt32(nodeList[i][noNode].InnerText);
                targetDic.Add(key, cData);
            }
        }
    }
    #endregion

    public int ConvertUpgradeLvToAddValue(int nowLv)
    {
        int addValue = 0;
        if(nowLv > 29)
        {
            addValue += 83;
            addValue += (nowLv-29)*5;
        }
        else if(nowLv > 19)
        {
            addValue += 43;
            addValue += (nowLv-19)*4;
        }
        else if(nowLv > 9)
        {
            addValue += 13;
            addValue += (nowLv-9)*3;
        }
        else if(nowLv > 5)
        {
            addValue += 5;
            addValue += (nowLv-5)*2;
        }
        else
        {
            addValue += nowLv;
        }
        return addValue;
    }
}
