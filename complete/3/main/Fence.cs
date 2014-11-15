using UnityEngine;
using System.Collections;

public class Fence : MonoBehaviour, IDamageable {

	public void Damage(float damageTaken)
	{
		GameData.Instance.gamePlayManager.Damage(damageTaken);
	}
}
