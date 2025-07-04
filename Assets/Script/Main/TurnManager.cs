using System;
using UnityEngine;
using TMPro;
public class TurnManager : MonoBehaviour
{
    //ΩÃ±€≈Ê
    static public TurnManager instance;


    public TextMeshProUGUI currentTurnText;
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
        currentTurnText.text = currentTurn.ToString();
        Debug.Log("≈œ ≥—æÓ∞®");
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
