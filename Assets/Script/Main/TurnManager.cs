using System;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections;
public class TurnManager : MonoBehaviour
{
    //ΩÃ±€≈Ê
    static public TurnManager instance;


    public TextMeshProUGUI currentTurnText;
    public Image fadePanel;
    public Canvas fadeCanvas;
    public float fadeDuration = 0.8f;
    public float currentTurn = 1;

    public Action<float> ChangeTurn;



    private void Awake()
    {

        currentTurnText.text = currentTurn.ToString();
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
    public void DayTransition()
    {
        StartCoroutine(DayTransitionCoroutine());
    }
    public IEnumerator DayTransitionCoroutine()
    {
        fadeCanvas.sortingOrder = 100;
        yield return fadePanel.DOFade(1f, fadeDuration).SetEase(Ease.InOutQuad).WaitForCompletion();
        NextTurn();
        yield return new WaitForSeconds(1.0f);

        yield return fadePanel.DOFade(0f, fadeDuration).SetEase(Ease.InOutQuad).WaitForCompletion();
        fadeCanvas.sortingOrder = 0;
    }
    void Update()
    {
        
    }
}
