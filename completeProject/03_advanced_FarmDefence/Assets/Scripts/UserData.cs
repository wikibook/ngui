using System.Xml;
using System.Xml.Serialization;

/// <summary>
/// 사용자 기본 데이터.
/// </summary>
public struct UserData {
    public string name;
    public int gems;
    public int coins;
    public int hearts;
    public int highScore;
    public double loginTime;
    public string facebook;
    
    public int upgradeNo;
    public int attLv;
    public int defLv;
    public int moneyLv;
}

/// <summary>
/// 아이템 정보.
/// </summary>
[XmlRoot]
public struct ItemData
{
    [XmlElement]
    public int no;
    [XmlElement]
    public int itemNo;
    [XmlElement]
    public int amount;
    [XmlElement]
    public bool use;
}

/// <summary>
/// 친구 정보.
/// </summary>
[XmlRoot]
public struct FriendData
{
    [XmlElement]
    public int no;
    [XmlElement]
    public int friend;
    [XmlElement]
    public string name;
    [XmlElement]
    public int score;
    [XmlElement]
    public int state;
    [XmlElement]
    public double sendTime;
    [XmlElement]
    public string facebook;
}

/// <summary>
/// 메시지 정보.
/// </summary>
[XmlRoot]
public struct MessageData
{
    [XmlElement]
    public int no;
    [XmlElement]
    public string sender;
    [XmlElement]
    public int msgType;
    [XmlElement]
    public int amount;
}

/// <summary>
/// 업그레이드 및 아이템 가격 정보.
/// </summary>
[XmlRoot]
public struct PriceData
{
    [XmlElement]
    public int codeNo;
    [XmlElement]
    public string price;
    [XmlElement]
    public string bonus;
    [XmlElement]
    public string amount;
}

/// <summary>
/// Inapp item data.
/// </summary>
[XmlRoot]
public struct InappItemData
{
    [XmlElement]
    public int codeNo;
    [XmlElement]
    public string sku;
}