using UnityEngine;

public class CoinsManager : MonoBehaviour
{
    private int totalCoins;
    private int coinsCollected;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        totalCoins = transform.childCount;   
    }

    public void CoinCollected()
    {
        coinsCollected += 1;
        //Debug.Log("Coins collected " +  coinsCollected + " of total coins " + totalCoins);
    }

}
