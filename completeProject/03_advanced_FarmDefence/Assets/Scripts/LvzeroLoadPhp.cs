using UnityEngine;
using System.Collections;

public class LvzeroLoadPhp : MonoBehaviour
{
	IEnumerator LoadFromPhp()
	{
		string url = "http://localhost/test.php";
		WWW www = new WWW(url);

		yield return www;

		if( www.isDone)
		{
			if( www.error == null)
			{
				Debug.Log("Receive Data : " + www.text);
			}
			else
			{
				Debug.Log("error : " + www.error);
			}
		}
	}


	void test()
	{
		int i = 0;
		i++;
		Debug.Log(i);
	}
}

