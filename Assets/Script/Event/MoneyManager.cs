using UnityEngine;

public class MoneyManager : MonoBehaviour 
{
    static public MoneyManager Instance;
    

    public int currentMoney = 0;

    public int GetCurrentMoney()
    {
        return currentMoney;
    }

    public void AddMoney(int amount)
    {
        currentMoney += amount;
    }


    private void Awake()
    {
        if(Instance != null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


}
