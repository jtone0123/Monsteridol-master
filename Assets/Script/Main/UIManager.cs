using UnityEngine;
using System.Collections.Generic;

// NUnit.Framework와 UnityEngine.UI, TMPro는 현재 코드에서 직접 사용되지 않으므로 제거하거나 필요시 다시 추가합니다.

public class UIManager : MonoBehaviour
{
    // 1. 싱글톤 인스턴스
    public static UIManager instance;


    public GameObject turnButton;
    public GameObject startButton;

    // 2. 인스펙터에서 UI 상태와 패널 게임오브젝트를 연결할 리스트
    [System.Serializable]
    public class UIPanelInfo
    {
        public UIState state;
        public GameObject panel;
    }

    [Header("UI 패널 정보")]
    [SerializeField] private List<UIPanelInfo> panelInfos;

    // 3. UI 상태 Enum (기존과 동일)
    public enum UIState
    {
        mainMenuPanel,
        scheduleManagementPanel,
        roomPlacementPanel,
        itemUsingPanel
    }

    // 4. 패널 관리를 위한 Dictionary
    private Dictionary<UIState, GameObject> panelDictionary = new Dictionary<UIState, GameObject>();

    // 5. 현재 UI 상태 (외부에서 읽기만 가능하도록 private set 추가)
    public UIState CurrentState { get; private set; }

    private void Awake()
    {
        // 싱글톤 패턴 초기화 (안전성 강화)
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            // 중복 인스턴스가 생성될 경우 이 게임오브젝트를 파괴
            Destroy(gameObject);
            return;
        }

        // 6. 인스펙터에서 설정한 리스트를 바탕으로 Dictionary 자동 생성
        foreach (var info in panelInfos)
        {
            if (info.panel != null && !panelDictionary.ContainsKey(info.state))
            {
                panelDictionary.Add(info.state, info.panel);
            }
        }
    }

    private void Start()
    {
        // 7. 시작 시 모든 패널을 비활성화하고 메인 메뉴만 표시
        foreach (var panel in panelDictionary.Values)
        {
            panel.SetActive(false);
        }
        ShowPanel(UIState.mainMenuPanel);
        AvailableNextTurn();
    }

    // 8. Enum을 직접 받아 패널을 보여주는 메서드
    public void ShowPanel(UIState stateToShow)
    {
        // 요청된 상태가 Dictionary에 없으면 오류를 출력하고 종료
        if (!panelDictionary.ContainsKey(stateToShow))
        {
            Debug.LogError($"[UIManager] '{stateToShow}'에 해당하는 패널이 등록되지 않았습니다.");
            return;
        }

        // 모든 패널을 순회하며 상태에 맞는 패널만 활성화
        foreach (var entry in panelDictionary)
        {
            bool isActive = entry.Key == stateToShow;
            if (entry.Value.activeSelf != isActive) // 불필요한 SetActive 호출 방지
            {
                entry.Value.SetActive(isActive);
            }
        }

        // 현재 상태 업데이트
        CurrentState = stateToShow;
    }


    public void AvailableNextTurn()
    {
        startButton.SetActive(false);
        turnButton.SetActive(true);
    }

    public void UnAvailavleNextTurn()
    {
        startButton.SetActive(true);
        turnButton.SetActive(false);
    }

    // (참고) 버튼의 OnClick 이벤트 등에서 int 값을 사용해야 할 경우
    // 이 메서드를 통해 enum으로 변환하여 ShowPanel을 호출할 수 있습니다.
    public void ShowPanelByIndex(int index)
    {
        // int 값이 UIState enum의 유효한 범위 내에 있는지 확인
        if (System.Enum.IsDefined(typeof(UIState), index))
        {
            UIState state = (UIState)index;
            ShowPanel(state);
        }
        else
        {
            Debug.LogError($"[UIManager] 잘못된 인덱스({index})가 전달되었습니다.");
        }
    }
}