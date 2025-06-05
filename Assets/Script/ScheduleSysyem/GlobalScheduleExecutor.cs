using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // Linq 사용을 위해 추가

/// <summary>
/// 여러 아이돌의 정보를 통합 관리하기 위한 도우미 클래스입니다.
/// 각 아이돌은 자신만의 캐릭터 정보, 이동 컴포넌트, 스케줄 큐, 다음 스케줄 보정치를 가집니다.
/// </summary>
[System.Serializable]
public class ManagedIdol
{
    public string idolName; // 식별을 위한 아이돌 이름 (IdolCharacter.characterName과 동기화 가능)
    public IdolCharacter idolCharacter;       // 아이돌의 스탯 및 기본 정보
    public Idol idolMovementController;      // 아이돌의 이동 및 NavPoint 관련 로직
    public ScheduleDropZone scheduleQueueUI; // 해당 아이돌의 스케줄 큐 UI (RectTransform을 가진 Panel 등)
    public NextScheduleModifiers pendingModifiers; // 해당 아이돌의 다음 스케줄에 적용될 보정치
    // public Coroutine currentProcessingCoroutine; // 병렬 처리 시 개별 코루틴 참조는 관리 복잡성 증가로 isProcessingComplete로 대체
    public bool isProcessingComplete; // 이 아이돌의 현재 턴 스케줄 처리가 완료되었는지 여부

    public ManagedIdol(IdolCharacter character, Idol movement, ScheduleDropZone queue)
    {
        idolCharacter = character;
        idolMovementController = movement;
        scheduleQueueUI = queue;
        idolName = character != null ? character.characterName : "Unknown Idol";
        pendingModifiers = new NextScheduleModifiers();
        isProcessingComplete = true; // 초기에는 처리 완료 상태
    }
}

/// <summary>
/// 여러 아이돌의 스케줄을 전역적으로 관리하고 실행하는 클래스입니다.
/// TempScheduleExecutor.cs의 기능을 확장하여 다수의 아이돌을 지원합니다.
/// </summary>
public class GlobalScheduleExecutor : MonoBehaviour
{
    [Header("관리 대상 아이돌 목록")]
    [Tooltip("게임 내에서 관리할 아이돌들의 정보를 담는 리스트입니다. Inspector에서 직접 할당하거나, 시작 시 자동으로 찾도록 설정할 수 있습니다.")]
    public List<ManagedIdol> managedIdols = new List<ManagedIdol>();

    [Header("UI 및 입력 설정")]
    [Tooltip("전체 아이돌 스케줄 실행을 위한 UI 버튼 (선택 사항)")]
    public Button globalExecuteButton;
    [Tooltip("각 스케줄 묶음 또는 단일 스케줄 실행 후, 다음 스케줄 실행까지의 대기 시간 (초)")]
    public float delayBetweenSchedules = 1.5f;
    [Tooltip("아이템 제거 후 나머지 아이템이 올라오는 애니메이션과 다음 스케줄 실행 사이의 추가 딜레이")]
    public float delayAfterLayoutAnimation = 0.3f;

    [Header("참조 (필요시 할당)")]
    public UIManager uiManager; // UI 패널 전환 등

    private PlayerInputActions playerInputActions;
    private bool isCurrentlyProcessingGlobalQueue = false; // 전체 큐 처리 중인지 여부

    void Awake()
    {
        playerInputActions = new PlayerInputActions();

        if (managedIdols.Count == 0)
        {
            Debug.LogWarning("GlobalScheduleExecutor: 관리할 아이돌 목록(managedIdols)이 비어있습니다. Inspector에서 설정하거나 자동 검색 로직을 추가해야 합니다.");
        }
        else
        {
            foreach (var managedIdol in managedIdols)
            {
                if (managedIdol.pendingModifiers == null)
                {
                    managedIdol.pendingModifiers = new NextScheduleModifiers();
                }
                if (managedIdol.idolCharacter == null) Debug.LogError($"GlobalScheduleExecutor: 아이돌 '{managedIdol.idolName}'의 IdolCharacter가 할당되지 않았습니다.");
                if (managedIdol.idolMovementController == null) Debug.LogError($"GlobalScheduleExecutor: 아이돌 '{managedIdol.idolName}'의 IdolMovementController가 할당되지 않았습니다.");
                if (managedIdol.scheduleQueueUI == null) Debug.LogError($"GlobalScheduleExecutor: 아이돌 '{managedIdol.idolName}'의 ScheduleDropZone (scheduleQueueUI)이 할당되지 않았습니다.");
            }
        }
    }

    void OnEnable()
    {
        if (playerInputActions.Gameplay.Get() != null)
        {
            playerInputActions.Gameplay.Enable();
            if (playerInputActions.Gameplay.ExecuteSchedule != null)
            {
                playerInputActions.Gameplay.ExecuteSchedule.performed += OnGlobalExecuteScheduleInput;
            }
            else Debug.LogError("GlobalScheduleExecutor: 'ExecuteSchedule' Action을 'Gameplay' Action Map에서 찾을 수 없습니다.");
        }
        else Debug.LogError("GlobalScheduleExecutor: 'Gameplay' Action Map을 찾을 수 없습니다.");

        if (globalExecuteButton != null)
            globalExecuteButton.onClick.AddListener(StartGlobalScheduleProcessing);
    }

    void OnDisable()
    {
        if (playerInputActions.Gameplay.Get() != null && playerInputActions.Gameplay.ExecuteSchedule != null)
        {
            playerInputActions.Gameplay.ExecuteSchedule.performed -= OnGlobalExecuteScheduleInput;
            playerInputActions.Gameplay.Disable();
        }
        if (globalExecuteButton != null)
            globalExecuteButton.onClick.RemoveListener(StartGlobalScheduleProcessing);

        // OnDisable 시 모든 진행 중인 코루틴 중지 (ManagedIdol의 currentProcessingCoroutine은 더 이상 직접 사용하지 않음)
        StopAllCoroutines(); // GlobalScheduleExecutor에서 실행한 모든 코루틴 중지
        isCurrentlyProcessingGlobalQueue = false; // 상태 초기화
        if (globalExecuteButton != null) globalExecuteButton.interactable = true; // 버튼 활성화
    }

    private void OnGlobalExecuteScheduleInput(InputAction.CallbackContext context)
    {
        Debug.Log("전체 스케줄 큐 실행 입력 감지!");
        StartGlobalScheduleProcessing();
    }

    public void StartGlobalScheduleProcessing()
    {
        if (isCurrentlyProcessingGlobalQueue)
        {
            Debug.LogWarning("이미 전체 스케줄 큐를 처리 중입니다.");
            return;
        }
        if (managedIdols.Count == 0)
        {
            Debug.Log("관리할 아이돌이 없습니다.");
            return;
        }

        bool anyIdolHasSchedule = managedIdols.Any(idol => idol.scheduleQueueUI != null && idol.scheduleQueueUI.transform.childCount > 0);
        if (!anyIdolHasSchedule)
        {
            Debug.Log("실행할 스케줄이 있는 아이돌이 없습니다.");
            return;
        }

        StartCoroutine(ProcessAllIdolQueuesCoroutine());
    }

    /// <summary>
    /// 모든 아이돌의 스케줄 큐를 병렬적으로 시작하고, 모든 처리가 완료될 때까지 기다립니다.
    /// 각 아이돌은 자신의 스케줄을 독립적으로 진행합니다.
    /// </summary>
    IEnumerator ProcessAllIdolQueuesCoroutine()
    {
        isCurrentlyProcessingGlobalQueue = true;
        if (globalExecuteButton != null) globalExecuteButton.interactable = false;
        Debug.Log("모든 아이돌 스케줄 큐 (병렬 시작 방식) 자동 실행 시작...");

        // 처리해야 할 아이돌들의 isProcessingComplete 플래그를 false로 설정
        foreach (var managedIdol in managedIdols)
        {
            if (managedIdol.scheduleQueueUI != null && managedIdol.scheduleQueueUI.transform.childCount > 0 &&
                managedIdol.idolCharacter != null && managedIdol.idolMovementController != null)
            {
                managedIdol.isProcessingComplete = false;
            }
            else
            {
                managedIdol.isProcessingComplete = true; // 스케줄이 없거나 필수 컴포넌트 없으면 바로 완료 처리
            }
        }

        // 각 아이돌의 스케줄 처리 코루틴을 시작 (yield return 없이 바로 시작)
        foreach (var managedIdol in managedIdols)
        {
            if (!managedIdol.isProcessingComplete) // 처리해야 할 아이돌만
            {
                Debug.Log($"아이돌 '{managedIdol.idolName}'의 스케줄 처리 코루틴 시작.");
                StartCoroutine(ProcessSingleIdolScheduleQueueCoroutine(managedIdol));
            }
        }

        // 모든 아이돌의 isProcessingComplete가 true가 될 때까지 기다립니다.
        // Linq의 All()을 사용하여 모든 아이돌이 완료되었는지 확인합니다.
        yield return new WaitUntil(() => managedIdols.All(idol => idol.isProcessingComplete));

        Debug.Log("모든 아이돌의 스케줄 실행 (병렬 시작 방식) 완료.");
        if (globalExecuteButton != null) globalExecuteButton.interactable = true;
        isCurrentlyProcessingGlobalQueue = false;

        // (선택적) 모든 스케줄 완료 후 UI 상태 변경 등
        // if (uiManager != null) uiManager.ShowMainMenuPanel();
    }


    /// <summary>
    /// 단일 아이돌의 스케줄 큐를 순차적으로 처리하는 코루틴입니다.
    /// 각 아이돌은 이 코루틴을 통해 자신의 스케줄을 독립적으로 진행합니다.
    /// </summary>
    /// <param name="currentManagedIdol">현재 처리할 아이돌의 ManagedIdol 정보</param>
    IEnumerator ProcessSingleIdolScheduleQueueCoroutine(ManagedIdol currentManagedIdol)
    {
        ScheduleDropZone queuePanel = currentManagedIdol.scheduleQueueUI;
        Idol idolMovement = currentManagedIdol.idolMovementController;
        IdolCharacter idolCharacter = currentManagedIdol.idolCharacter; // IdolCharacter 참조 추가

        bool hasArrivedForCurrentSchedule = false;
        System.Action arrivalCallback = () => { hasArrivedForCurrentSchedule = true; };

        // 루프 시작 전에 pendingModifiers를 한 번 초기화하는 것이 좋을 수 있습니다.
        // (혹시 이전 턴의 잔여 데이터가 있다면)
        // currentManagedIdol.pendingModifiers.Reset(); 

        while (queuePanel.transform.childCount > 0)
        {
            DraggableScheduleItem firstScheduleItemInQueue = queuePanel.transform.GetChild(0).GetComponent<DraggableScheduleItem>();
            if (firstScheduleItemInQueue == null || firstScheduleItemInQueue.scheduleData == null)
            {
                Debug.LogWarning($"아이돌 '{currentManagedIdol.idolName}' 큐의 첫 번째 아이템이 유효하지 않습니다. 제거합니다.");
                if (queuePanel.transform.childCount > 0) Destroy(queuePanel.transform.GetChild(0).gameObject);
                yield return null;
                continue;
            }

            ScheduleData currentScheduleData = firstScheduleItemInQueue.scheduleData;
            int bundleSizeN = 0;
            List<Transform> itemsInBundle = new List<Transform>();

            for (int i = 0; i < queuePanel.transform.childCount; i++)
            {
                Transform itemTransform = queuePanel.transform.GetChild(i);
                DraggableScheduleItem ditem = itemTransform.GetComponent<DraggableScheduleItem>();
                if (ditem != null && ditem.scheduleData == currentScheduleData)
                {
                    itemsInBundle.Add(itemTransform);
                    bundleSizeN++;
                }
                else
                {
                    break;
                }
            }

            if (bundleSizeN > 0)
            {
                Debug.Log($"아이돌 '{idolCharacter.characterName}': '{currentScheduleData.scheduleName}' 스케줄 묶음 (크기: {bundleSizeN}) 처리 시작.");

                ScheduleNavType scheduleNavType = firstScheduleItemInQueue.GetComponent<ScheduleNavType>();
                if (scheduleNavType != null && scheduleNavType.NavPoint != null)
                {
                    Debug.Log($"아이돌 '{idolCharacter.characterName}': '{scheduleNavType.NavPoint.pointName}'으로 이동 시작.");

                    // 이동 전 콜백 등록 및 플래그 초기화
                    hasArrivedForCurrentSchedule = false; // 도착 플래그 재설정
                    idolMovement.OnArrivedAtTarget += arrivalCallback;
                    idolMovement.AssignScheduledTarget(scheduleNavType.NavPoint);

                    // 도착 대기: hasArrivedForCurrentSchedule가 true가 되거나, 이동이 멈췄는데 목표가 없는 경우(오류 상황)
                    yield return new WaitUntil(() => hasArrivedForCurrentSchedule || (idolMovement.finalScheduledTarget == null));

                    idolMovement.OnArrivedAtTarget -= arrivalCallback; // 콜백 해제

                    if (!hasArrivedForCurrentSchedule && idolMovement.finalScheduledTarget != null)
                    {
                        Debug.LogWarning($"아이돌 '{idolCharacter.characterName}': 목표({idolMovement.finalScheduledTarget.pointName})에 도달하지 못하고 이동이 중단된 것 같습니다. 스케줄을 건너뜁니다.");
                        // 아이템 제거 및 다음 스케줄로 넘어가는 로직 추가 가능
                    }
                    else if (hasArrivedForCurrentSchedule)
                    {
                        Debug.Log($"아이돌 '{idolCharacter.characterName}': '{scheduleNavType.NavPoint.pointName}' 도착.");
                    }
                    else
                    {
                        Debug.Log($"아이돌 '{idolCharacter.characterName}': '{scheduleNavType.NavPoint.pointName}'로의 이동이 완료되었거나 중단되었습니다 (도착 콜백 미호출).");
                    }
                }
                else
                {
                    Debug.LogWarning($"아이돌 '{idolCharacter.characterName}': '{currentScheduleData.scheduleName}' 스케줄에 NavPoint가 설정되지 않았거나 ScheduleNavType 컴포넌트가 없습니다. 이동 없이 스케줄을 실행합니다.");
                }

                PerformScheduleBundle(currentScheduleData, bundleSizeN, currentManagedIdol.pendingModifiers, idolCharacter, idolMovement);
                currentManagedIdol.pendingModifiers.Reset();

                foreach (Transform itemToRemove in itemsInBundle)
                {
                    if (itemToRemove != null) Destroy(itemToRemove.gameObject);
                }
                itemsInBundle.Clear();

                yield return null;

                if (queuePanel.transform.childCount > 0)
                {
                    queuePanel.RefreshLayout(true, true);
                    yield return new WaitForSeconds(queuePanel.siblingAnimationDuration + delayAfterLayoutAnimation);
                }

                if (queuePanel.transform.childCount > 0)
                {
                    DraggableScheduleItem nextScheduleItem = queuePanel.transform.GetChild(0).GetComponent<DraggableScheduleItem>();
                    if (nextScheduleItem != null && nextScheduleItem.scheduleData != null)
                    {
                        ScheduleData nextScheduleInQueueData = nextScheduleItem.scheduleData;
                        if (currentScheduleData.outgoingLinkEffectRules != null)
                        {
                            foreach (var rule in currentScheduleData.outgoingLinkEffectRules)
                            {
                                if (rule.targetNextScheduleCondition == nextScheduleInQueueData && rule.effectToApplyOnNextSchedule != null)
                                {
                                    rule.effectToApplyOnNextSchedule.ApplyToNext(idolCharacter, currentScheduleData, nextScheduleInQueueData, currentManagedIdol.pendingModifiers);
                                }
                            }
                        }
                    }
                }
            }

            if (queuePanel.transform.childCount > 0)
            {
                yield return new WaitForSeconds(delayBetweenSchedules);
            }
        }
        currentManagedIdol.isProcessingComplete = true;
        Debug.Log($"아이돌 '{idolCharacter.characterName}'의 모든 스케줄 처리 완료.");
    }

    private void PerformScheduleBundle(ScheduleData dataToExecute, int bundleSizeN, NextScheduleModifiers modifiers, IdolCharacter targetIdolCharacter, Idol targetIdolMovement)
    {
        if (targetIdolCharacter == null)
        {
            Debug.LogError("PerformScheduleBundle: targetIdolCharacter가 null입니다. 스케줄을 실행할 수 없습니다.");
            return;
        }

        Debug.Log($"--- 아이돌 '{targetIdolCharacter.characterName}': '{dataToExecute.scheduleName}' (묶음 크기: {bundleSizeN}) 실행 시작 ---");

        float finalSuccessRate = dataToExecute.baseSuccessRate;
        int baseStatImprovementForBundle = dataToExecute.primaryStatImprovementAmount;
        // 연속 효과가 아이템별로 적용되지 않거나, 연속 효과 조건 미달 시 기본값 사용
        if (!(dataToExecute.applyStatBonusPerItemInBundle && dataToExecute.canFormConsecutiveBundle && bundleSizeN >= dataToExecute.minItemsForConsecutiveEffect))
        {
            baseStatImprovementForBundle *= bundleSizeN;
        }
        // 만약 applyStatBonusPerItemInBundle가 true이고 연속 효과 조건 만족 시,
        // 기본 스탯 향상은 아이템당 한 번만 적용되고, 연속 보너스가 각 아이템에 추가되는 방식이라면
        // baseStatImprovementForBundle = dataToExecute.primaryStatImprovementAmount; (곱하지 않음)
        // 그리고 연속 보너스 부분에서 (dataToExecute.consecutiveStatBonus * bundleSizeN)를 더함.
        // 현재 코드는 primaryStatImprovementAmount는 항상 bundleSize만큼 곱하고, consecutiveStatBonus는 applyStatBonusPerItemInBundle에 따라 다르게 적용.
        // 좀 더 명확하게 하려면, ScheduleData에 '기본 스탯 향상도 묶음 크기만큼 곱할 것인가?'라는 별도 bool 변수 추가 고려.
        // 여기서는 일단 기본 스탯은 묶음 크기만큼 곱하고, 연속 보너스는 설정에 따라 처리하는 것으로 유지.
        // 단, applyStatBonusPerItemInBundle이 true이고 연속 효과가 발동하면, 기본 스탯은 한 번만 적용하고 연속 보너스를 아이템 수만큼 더하는 것이 더 자연스러울 수 있음.
        // 아래 로직을 수정하여 이를 반영:
        if (dataToExecute.applyStatBonusPerItemInBundle && dataToExecute.canFormConsecutiveBundle && bundleSizeN >= dataToExecute.minItemsForConsecutiveEffect)
        {
            baseStatImprovementForBundle = dataToExecute.primaryStatImprovementAmount; // 연속 효과 시 기본은 한 번만
        }
        else
        {
            baseStatImprovementForBundle = dataToExecute.primaryStatImprovementAmount * bundleSizeN;
        }


        int finalStatImprovement = baseStatImprovementForBundle;

        // 스트레스 변화량도 묶음 크기에 비례할지, 아니면 고정일지 게임 기획에 따라 결정. 여기서는 비례로 가정.
        int finalStressChangeOnSuccess = dataToExecute.stressChangeOnSuccess * bundleSizeN;
        int finalStressChangeOnFailure = dataToExecute.stressChangeOnFailure * bundleSizeN;


        if (modifiers != null)
        {
            if (modifiers.successRateBonus != 0)
            {
                finalSuccessRate += modifiers.successRateBonus;
                Debug.Log($"연계 효과로 성공 확률 {modifiers.successRateBonus * 100:F0}%p 적용됨. 현재 성공률: {finalSuccessRate * 100:F0}%");
            }
            if (modifiers.statToBuff == dataToExecute.primaryTargetStat && modifiers.statBuffAmount != 0)
            {
                finalStatImprovement += modifiers.statBuffAmount;
                Debug.Log($"연계 효과로 {modifiers.statToBuff} 스탯 +{modifiers.statBuffAmount} 추가 적용됨.");
            }
            else if (modifiers.statToBuff != StatType.None && modifiers.statBuffAmount != 0)
            {
                Debug.Log($"연계 효과로 {modifiers.statToBuff} 스탯에 +{modifiers.statBuffAmount} 보너스가 있었지만, 주 대상 스탯과 달라 직접 합산되지 않음.");
                // 필요시 여기서 targetIdolCharacter.Add[StatName]Point(modifiers.statBuffAmount) 직접 호출
            }
        }

        if (dataToExecute.canFormConsecutiveBundle && bundleSizeN >= dataToExecute.minItemsForConsecutiveEffect)
        {
            Debug.Log($"'{dataToExecute.scheduleName}' 연속 효과 발동! (묶음 크기: {bundleSizeN})");
            if (dataToExecute.consecutiveSuccessRateModifierPerN != 0)
            {
                int nForPenaltyCalc = bundleSizeN;
                if (dataToExecute.maxNForSuccessRatePenaltyStack > 0 && dataToExecute.consecutiveSuccessRateModifierPerN < 0)
                {
                    nForPenaltyCalc = Mathf.Min(bundleSizeN, dataToExecute.maxNForSuccessRatePenaltyStack);
                }
                float successRateChange = dataToExecute.consecutiveSuccessRateModifierPerN * nForPenaltyCalc;
                finalSuccessRate += successRateChange;
                Debug.Log($"연속 효과로 성공 확률 보정: {successRateChange * 100:F0}%p. 현재 성공률: {finalSuccessRate * 100:F0}%");
            }

            if (dataToExecute.consecutiveStatBonus != 0)
            {
                if (dataToExecute.applyStatBonusPerItemInBundle)
                {
                    finalStatImprovement += (dataToExecute.consecutiveStatBonus * bundleSizeN);
                    Debug.Log($"연속 효과 스탯 보너스 적용: 각 아이템당 +{dataToExecute.consecutiveStatBonus} (총 +{dataToExecute.consecutiveStatBonus * bundleSizeN})");
                }
                else
                {
                    finalStatImprovement += dataToExecute.consecutiveStatBonus;
                    Debug.Log($"연속 효과 스탯 보너스 적용: 묶음 전체에 +{dataToExecute.consecutiveStatBonus}");
                }
            }
        }
        finalSuccessRate = Mathf.Clamp01(finalSuccessRate);

        bool success = Random.value < finalSuccessRate;
        Debug.Log($"최종 성공 확률: {finalSuccessRate * 100:F0}% > 결과: {(success ? "성공!" : "실패...")}");
        string effectLog = "";

        if (success)
        {
            effectLog += "성공 효과: ";
            if (dataToExecute.primaryTargetStat != StatType.None && finalStatImprovement != 0)
            {
                switch (dataToExecute.primaryTargetStat)
                {
                    case StatType.Vocal: targetIdolCharacter.AddVocalPoint(finalStatImprovement); break;
                    case StatType.Dance: targetIdolCharacter.AddDancePoint(finalStatImprovement); break;
                    case StatType.Rap: targetIdolCharacter.AddRapPoint(finalStatImprovement); break;
                    default:
                        Debug.LogWarning($"PerformScheduleBundle: 처리되지 않은 primaryTargetStat ({dataToExecute.primaryTargetStat})에 대한 스탯 적용 로직이 IdolCharacter에 없습니다.");
                        break;
                }
                effectLog += $"{dataToExecute.primaryTargetStat} +{finalStatImprovement}, ";
            }
            targetIdolCharacter.ChangeStress(finalStressChangeOnSuccess);
            effectLog += $"스트레스 {(finalStressChangeOnSuccess >= 0 ? "+" : "")}{finalStressChangeOnSuccess}";
        }
        else
        {
            effectLog += "실패 효과: ";
            targetIdolCharacter.ChangeStress(finalStressChangeOnFailure);
            effectLog += $"스트레스 {(finalStressChangeOnFailure >= 0 ? "+" : "")}{finalStressChangeOnFailure}";
        }
        Debug.Log(effectLog.TrimEnd(' ', ','));
        Debug.Log($"아이돌 '{targetIdolCharacter.characterName}': '{dataToExecute.scheduleName}' (묶음 크기: {bundleSizeN}) 실행 완료. 현재 아이돌 상태: {targetIdolCharacter.GetCurrentStatus()}");
        Debug.Log("------------------------------------");
    }

    public List<GameObject> scheduleItemPrefabs;
    public Transform availableSchedulesPanel;

    public void ClearAvailableSchedulesDisplay()
    {
        if (availableSchedulesPanel == null) return;
        for (int i = availableSchedulesPanel.childCount - 1; i >= 0; i--)
        {
            Destroy(availableSchedulesPanel.GetChild(i).gameObject);
        }
    }

    public void SpawnNewScheduleItemInPool(int prefabIndex)
    {
        if (scheduleItemPrefabs == null || scheduleItemPrefabs.Count <= prefabIndex || scheduleItemPrefabs[prefabIndex] == null || availableSchedulesPanel == null)
        {
            Debug.LogError("스케줄 프리팹이 없거나, 인덱스가 잘못되었거나, 부모 패널(availableSchedulesPanel)이 할당되지 않았습니다!");
            return;
        }
        GameObject newScheduleObject = Instantiate(scheduleItemPrefabs[prefabIndex], availableSchedulesPanel);
    }
}
