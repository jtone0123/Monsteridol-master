using UnityEngine;
using UnityEngine.InputSystem;

// 클릭 가능한 오브젝트가 구현해야 할 인터페이스
public interface IClickable
{
    void OnClick();
}

public class ClickManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static ClickManager Instance { get; private set; }

    public PlayerInputActions actions;

    private void Awake()
    {
        actions = new PlayerInputActions();
        
    }
    private void OnEnable()
    {
        actions.Gameplay.Enable();
        actions.Gameplay.ClikAction.performed += OnClick;
    }


    public void OnClick(InputAction.CallbackContext context)
    {
       
        Vector2 screenPosition = actions.Gameplay.Position.ReadValue<Vector2>();

        // 스크린 좌표를 통해 월드에 레이(Ray)를 쏩니다.
        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(screenPosition);

        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);


        // 레이캐스팅 실행
        if (hit.collider != null)
        {
            
            
            hit.collider.GetComponent<IClickable>()?.OnClick();
            // IClickable 인터페이스가 있다면 OnClick() 메서드를 호출합니다.
           
            // 또는 특정 태그를 가진 오브젝트인지 확인할 수도 있습니다.
            // if(hit.collider.CompareTag("Enemy")) { ... }

            Debug.Log($"Clicked on: {hit.collider.name}");
        }
    }

    
}


