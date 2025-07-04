using UnityEngine;
using DG.Tweening;
using DG.Tweening.Core.Easing; // DOTween 사용을 위한 네임스페이스

public class CameraManager : MonoBehaviour
{
    //싱글톤
    static public CameraManager Instance;


    [Header("카메라 설정")]
    public Camera mainCamera; // 씬의 메인 카메라 (없으면 Camera.main으로 자동 할당)

    [Header("줌인 목표 설정")]
    public Transform targetCharacter;
    public Vector2 zoomTargetOffsetFromCharacter = new Vector2(0f, 0.5f); // 캐릭터 기준 줌인 목표점 오프셋 (화면 가운데에서 살짝 왼쪽)
                                                                          // 예: (캐릭터 x + 오프셋 x, 캐릭터 y + 오프셋 y)
    public float zoomInOrthographicSize = 5f; // 줌인 후 카메라의 Orthographic Size (값이 작을수록 더 크게 줌인)

    [Header("애니메이션 설정")]
    public float zoomDuration = 0.5f; // 줌인/줌아웃 애니메이션 시간
    public Ease zoomEaseType = Ease.OutQuad; // 줌인/줌아웃 애니메이션 이징 타입 (부드러운 효과)

    // === 카메라의 초기 상태 저장 변수 ===
    private Vector3 originalCameraPosition;
    private float originalOrthographicSize;

    private bool isZoomedIn = false; // 현재 줌인 상태인지 확인하는 플래그
    
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(Instance);
        }
        // 메인 카메라가 할당되지 않았다면 찾아옵니다.
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // 카메라의 초기 위치와 Orthographic Size를 저장합니다.
        if (mainCamera != null)
        {
            originalCameraPosition = mainCamera.transform.position;
            originalOrthographicSize = mainCamera.orthographicSize;
        }
        else
        {
            Debug.LogError("메인 카메라를 찾을 수 없습니다! 'MainCamera' 태그가 할당되었는지 확인하세요.");
            enabled = false; // 스크립트 비활성화
        }
    }
    private void LateUpdate()
    {
        if(isZoomedIn)
        {
            FllowingCamera();
        }
    }
    public void SetTargetOffset(Vector2 targetOffset)
    {
        zoomTargetOffsetFromCharacter = targetOffset;
    }
    public void SetCameraTarget(Transform target, float zoomSize = 5 )
    {
        targetCharacter = target;
        zoomInOrthographicSize = zoomSize;
    }
    // === 줌인 함수 ===

    public void FllowingCamera()
    {
      
        // 카메라의 Z축은 -10으로 고정하는 것이 2D 게임에서 일반적입니다.
        Vector3 targetCameraPosition = new Vector3(
            targetCharacter.position.x + zoomTargetOffsetFromCharacter.x,
            targetCharacter.position.y + zoomTargetOffsetFromCharacter.y,
            originalCameraPosition.z // 카메라 Z축은 유지하거나 -10 등으로 고정
        );

        // DOTween을 사용하여 카메라 위치와 Orthographic Size를 동시에 부드럽게 변경
        mainCamera.transform.DOMove(targetCameraPosition, zoomDuration).SetEase(zoomEaseType);
       
    }
    public void ZoomInToCharacter()
    {
        if (mainCamera == null || targetCharacter == null || isZoomedIn) return;
       
        // 줌인 목표 위치 계산
        // 캐릭터의 위치를 기반으로 하되, y축으로 약간 위로, x축으로 약간 왼쪽으로 오프셋을 줍니다.
        // 카메라의 Z축은 -10으로 고정하는 것이 2D 게임에서 일반적입니다.
        Vector3 targetCameraPosition = new Vector3(
            targetCharacter.position.x + zoomTargetOffsetFromCharacter.x,
            targetCharacter.position.y + zoomTargetOffsetFromCharacter.y,
            originalCameraPosition.z // 카메라 Z축은 유지하거나 -10 등으로 고정
        );

        // DOTween을 사용하여 카메라 위치와 Orthographic Size를 동시에 부드럽게 변경
        mainCamera.transform.DOMove(targetCameraPosition, zoomDuration).SetEase(zoomEaseType);
       mainCamera.DOOrthoSize(zoomInOrthographicSize, zoomDuration).SetEase(zoomEaseType)
                  .OnComplete(() => isZoomedIn = true); // 애니메이션 완료 후 줌인 상태로 설정
    }

    // === 줌아웃 함수 (원래 상태로 돌아가기) ===
    public void ZoomOutToOriginal()
    {
        if (mainCamera == null || !isZoomedIn) return;


        // DOTween을 사용하여 카메라 위치와 Orthographic Size를 원래대로 부드럽게 변경

        isZoomedIn = false;
        mainCamera.DOOrthoSize(originalOrthographicSize, zoomDuration).SetEase(zoomEaseType);
                   
        mainCamera.transform.DOMove(originalCameraPosition, zoomDuration).SetEase(zoomEaseType);
    }

    // === 테스트용 버튼 입력 (선택 사항) ===
    void Update()
    {
        
         
     }
}