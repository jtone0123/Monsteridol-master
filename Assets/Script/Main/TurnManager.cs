using System;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    //�̱���
    static public TurnManager instance;

    public float currentTurn;
    public Action<float> ChangeTurn;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(instance);
        }
    }

    public void NextTurn()
    {
        currentTurn += 1f;
        ChangeTurn?.Invoke(1f);
        Debug.Log("�� �Ѿ");
    }

    public float GetCurrentTurn()
    { 
        return currentTurn;
    }

    void Start()
    {
        
    }

    
    void Update()
    {
        
    }
}
