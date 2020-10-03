using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Text availableSoldiersToSpawn;

    public void SetAvailableSoldiersToSpawnAmount(int amount)
    {
        availableSoldiersToSpawn.text = amount.ToString();
    }
}
