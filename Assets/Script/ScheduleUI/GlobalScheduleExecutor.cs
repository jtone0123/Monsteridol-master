using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // Linq ����� ���� �߰�

/// <summary>
/// ���� ���̵��� ������ ���� �����ϱ� ���� ����� Ŭ�����Դϴ�.
/// �� ���̵��� �ڽŸ��� ĳ���� ����, �̵� ������Ʈ, ������ ť, ���� ������ ����ġ�� �����ϴ�.
/// </summary>
[System.Serializable]
public class ManagedIdol
{
    public string idolName; // �ĺ��� ���� ���̵� �̸� (IdolCharacter.characterName�� ����ȭ ����)
    public IdolCharacter idolCharacter;       // ���̵��� ���� �� �⺻ ����
    public Idol idolMovementController;      // ���̵��� �̵� �� NavPoint ���� ����
    public ScheduleDropZone scheduleQueueUI; // �ش� ���̵��� ������ ť UI (RectTransform�� ���� Panel ��)
    public NextScheduleModifiers pendingModifiers; // �ش� ���̵��� ���� �����ٿ� ����� ����ġ
    // public Coroutine currentProcessingCoroutine; // ���� ó�� �� ���� �ڷ�ƾ ������ ���� ���⼺ ������ isProcessingComplete�� ��ü
    public bool isProcessingComplete; // �� ���̵��� ���� �� ������ ó���� �Ϸ�Ǿ����� ����

    public ManagedIdol(IdolCharacter character, Idol movement, ScheduleDropZone queue)
    {
        idolCharacter = character;
        idolMovementController = movement;
        scheduleQueueUI = queue;
        idolName = character != null ? character.characterName : "Unknown Idol";
        pendingModifiers = new NextScheduleModifiers();
        isProcessingComplete = true; // �ʱ⿡�� ó�� �Ϸ� ����
    }
}

/// <summary>
/// ���� ���̵��� �������� ���������� �����ϰ� �����ϴ� Ŭ�����Դϴ�.
/// TempScheduleExecutor.cs�� ����� Ȯ���Ͽ� �ټ��� ���̵��� �����մϴ�.
/// </summary>
public class GlobalScheduleExecutor : MonoBehaviour
{
    [Header("���� ��� ���̵� ���")]
    [Tooltip("���� ������ ������ ���̵����� ������ ��� ����Ʈ�Դϴ�. Inspector���� ���� �Ҵ��ϰų�, ���� �� �ڵ����� ã���� ������ �� �ֽ��ϴ�.")]
    public List<ManagedIdol> managedIdols = new List<ManagedIdol>();

    [Header("UI �� �Է� ����")]
    [Tooltip("��ü ���̵� ������ ������ ���� UI ��ư (���� ����)")]
    public Button globalExecuteButton;
    [Tooltip("�� ������ ���� �Ǵ� ���� ������ ���� ��, ���� ������ ��������� ��� �ð� (��)")]
    public float delayBetweenSchedules = 1.5f;
    [Tooltip("������ ���� �� ������ �������� �ö���� �ִϸ��̼ǰ� ���� ������ ���� ������ �߰� ������")]
    public float delayAfterLayoutAnimation = 0.3f;

    [Header("���� (�ʿ�� �Ҵ�)")]
    public UIManager uiManager; // UI �г� ��ȯ ��

    private PlayerInputActions playerInputActions;
    private bool isCurrentlyProcessingGlobalQueue = false; // ��ü ť ó�� ������ ����

    void Awake()
    {
        playerInputActions = new PlayerInputActions();

        if (managedIdols.Count == 0)
        {
            Debug.LogWarning("GlobalScheduleExecutor: ������ ���̵� ���(managedIdols)�� ����ֽ��ϴ�. Inspector���� �����ϰų� �ڵ� �˻� ������ �߰��ؾ� �մϴ�.");
        }
        else
        {
            foreach (var managedIdol in managedIdols)
            {
                if (managedIdol.pendingModifiers == null)
                {
                    managedIdol.pendingModifiers = new NextScheduleModifiers();
                }
                if (managedIdol.idolCharacter == null) Debug.LogError($"GlobalScheduleExecutor: ���̵� '{managedIdol.idolName}'�� IdolCharacter�� �Ҵ���� �ʾҽ��ϴ�.");
                if (managedIdol.idolMovementController == null) Debug.LogError($"GlobalScheduleExecutor: ���̵� '{managedIdol.idolName}'�� IdolMovementController�� �Ҵ���� �ʾҽ��ϴ�.");
                if (managedIdol.scheduleQueueUI == null) Debug.LogError($"GlobalScheduleExecutor: ���̵� '{managedIdol.idolName}'�� ScheduleDropZone (scheduleQueueUI)�� �Ҵ���� �ʾҽ��ϴ�.");
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
            else Debug.LogError("GlobalScheduleExecutor: 'ExecuteSchedule' Action�� 'Gameplay' Action Map���� ã�� �� �����ϴ�.");
        }
        else Debug.LogError("GlobalScheduleExecutor: 'Gameplay' Action Map�� ã�� �� �����ϴ�.");

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

        // OnDisable �� ��� ���� ���� �ڷ�ƾ ���� (ManagedIdol�� currentProcessingCoroutine�� �� �̻� ���� ������� ����)
        StopAllCoroutines(); // GlobalScheduleExecutor���� ������ ��� �ڷ�ƾ ����
        isCurrentlyProcessingGlobalQueue = false; // ���� �ʱ�ȭ
        if (globalExecuteButton != null) globalExecuteButton.interactable = true; // ��ư Ȱ��ȭ
    }

    private void OnGlobalExecuteScheduleInput(InputAction.CallbackContext context)
    {
        Debug.Log("��ü ������ ť ���� �Է� ����!");
        StartGlobalScheduleProcessing();
    }

    public void StartGlobalScheduleProcessing()
    {
        if (isCurrentlyProcessingGlobalQueue)
        {
            Debug.LogWarning("�̹� ��ü ������ ť�� ó�� ���Դϴ�.");
            return;
        }
        if (managedIdols.Count == 0)
        {
            Debug.Log("������ ���̵��� �����ϴ�.");
            return;
        }

        bool anyIdolHasSchedule = managedIdols.Any(idol => idol.scheduleQueueUI != null && idol.scheduleQueueUI.transform.childCount > 0);
        if (!anyIdolHasSchedule)
        {
            Debug.Log("������ �������� �ִ� ���̵��� �����ϴ�.");
            return;
        }

        StartCoroutine(ProcessAllIdolQueuesCoroutine());
    }

    /// <summary>
    /// ��� ���̵��� ������ ť�� ���������� �����ϰ�, ��� ó���� �Ϸ�� ������ ��ٸ��ϴ�.
    /// �� ���̵��� �ڽ��� �������� ���������� �����մϴ�.
    /// </summary>
    IEnumerator ProcessAllIdolQueuesCoroutine()
    {
        isCurrentlyProcessingGlobalQueue = true;
        if (globalExecuteButton != null) globalExecuteButton.interactable = false;
        Debug.Log("��� ���̵� ������ ť (���� ���� ���) �ڵ� ���� ����...");

        // ó���ؾ� �� ���̵����� isProcessingComplete �÷��׸� false�� ����
        foreach (var managedIdol in managedIdols)
        {
            if (managedIdol.scheduleQueueUI != null && managedIdol.scheduleQueueUI.transform.childCount > 0 &&
                managedIdol.idolCharacter != null && managedIdol.idolMovementController != null)
            {
                managedIdol.isProcessingComplete = false;
            }
            else
            {
                managedIdol.isProcessingComplete = true; // �������� ���ų� �ʼ� ������Ʈ ������ �ٷ� �Ϸ� ó��
            }
        }

        // �� ���̵��� ������ ó�� �ڷ�ƾ�� ���� (yield return ���� �ٷ� ����)
        foreach (var managedIdol in managedIdols)
        {
            if (!managedIdol.isProcessingComplete) // ó���ؾ� �� ���̵���
            {
                Debug.Log($"���̵� '{managedIdol.idolName}'�� ������ ó�� �ڷ�ƾ ����.");
                StartCoroutine(ProcessSingleIdolScheduleQueueCoroutine(managedIdol));
            }
        }

        // ��� ���̵��� isProcessingComplete�� true�� �� ������ ��ٸ��ϴ�.
        // Linq�� All()�� ����Ͽ� ��� ���̵��� �Ϸ�Ǿ����� Ȯ���մϴ�.
        yield return new WaitUntil(() => managedIdols.All(idol => idol.isProcessingComplete));

        Debug.Log("��� ���̵��� ������ ���� (���� ���� ���) �Ϸ�.");
        if (globalExecuteButton != null) globalExecuteButton.interactable = true;
        isCurrentlyProcessingGlobalQueue = false;

        // (������) ��� ������ �Ϸ� �� UI ���� ���� ��
        // if (uiManager != null) uiManager.ShowMainMenuPanel();
    }


    /// <summary>
    /// ���� ���̵��� ������ ť�� ���������� ó���ϴ� �ڷ�ƾ�Դϴ�.
    /// �� ���̵��� �� �ڷ�ƾ�� ���� �ڽ��� �������� ���������� �����մϴ�.
    /// </summary>
    /// <param name="currentManagedIdol">���� ó���� ���̵��� ManagedIdol ����</param>
    IEnumerator ProcessSingleIdolScheduleQueueCoroutine(ManagedIdol currentManagedIdol)
    {
        ScheduleDropZone queuePanel = currentManagedIdol.scheduleQueueUI;
        Idol idolMovement = currentManagedIdol.idolMovementController;
        IdolCharacter idolCharacter = currentManagedIdol.idolCharacter; // IdolCharacter ���� �߰�

        bool hasArrivedForCurrentSchedule = false;
        System.Action arrivalCallback = () => { hasArrivedForCurrentSchedule = true; };

        // ���� ���� ���� pendingModifiers�� �� �� �ʱ�ȭ�ϴ� ���� ���� �� �ֽ��ϴ�.
        // (Ȥ�� ���� ���� �ܿ� �����Ͱ� �ִٸ�)
        // currentManagedIdol.pendingModifiers.Reset(); 

        while (queuePanel.transform.childCount > 0)
        {
            DraggableScheduleItem firstScheduleItemInQueue = queuePanel.transform.GetChild(0).GetComponent<DraggableScheduleItem>();
            if (firstScheduleItemInQueue == null || firstScheduleItemInQueue.scheduleData == null)
            {
                Debug.LogWarning($"���̵� '{currentManagedIdol.idolName}' ť�� ù ��° �������� ��ȿ���� �ʽ��ϴ�. �����մϴ�.");
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
                Debug.Log($"���̵� '{idolCharacter.characterName}': '{currentScheduleData.scheduleName}' ������ ���� (ũ��: {bundleSizeN}) ó�� ����.");

                ScheduleNavType scheduleNavType = firstScheduleItemInQueue.GetComponent<ScheduleNavType>();
                if (scheduleNavType != null && scheduleNavType.NavPoint != null)
                {
                    Debug.Log($"���̵� '{idolCharacter.characterName}': '{scheduleNavType.NavPoint.pointName}'���� �̵� ����.");

                    // �̵� �� �ݹ� ��� �� �÷��� �ʱ�ȭ
                    hasArrivedForCurrentSchedule = false; // ���� �÷��� �缳��
                    idolMovement.OnArrivedAtTarget += arrivalCallback;
                    idolMovement.AssignScheduledTarget(scheduleNavType.NavPoint);

                    // ���� ���: hasArrivedForCurrentSchedule�� true�� �ǰų�, �̵��� ����µ� ��ǥ�� ���� ���(���� ��Ȳ)
                    yield return new WaitUntil(() => hasArrivedForCurrentSchedule || (idolMovement.finalScheduledTarget == null));

                    idolMovement.OnArrivedAtTarget -= arrivalCallback; // �ݹ� ����

                    if (!hasArrivedForCurrentSchedule && idolMovement.finalScheduledTarget != null)
                    {
                        Debug.LogWarning($"���̵� '{idolCharacter.characterName}': ��ǥ({idolMovement.finalScheduledTarget.pointName})�� �������� ���ϰ� �̵��� �ߴܵ� �� �����ϴ�. �������� �ǳʶݴϴ�.");
                        // ������ ���� �� ���� �����ٷ� �Ѿ�� ���� �߰� ����
                    }
                    else if (hasArrivedForCurrentSchedule)
                    {
                        Debug.Log($"���̵� '{idolCharacter.characterName}': '{scheduleNavType.NavPoint.pointName}' ����.");
                    }
                    else
                    {
                        Debug.Log($"���̵� '{idolCharacter.characterName}': '{scheduleNavType.NavPoint.pointName}'���� �̵��� �Ϸ�Ǿ��ų� �ߴܵǾ����ϴ� (���� �ݹ� ��ȣ��).");
                    }
                }
                else
                {
                    Debug.LogWarning($"���̵� '{idolCharacter.characterName}': '{currentScheduleData.scheduleName}' �����ٿ� NavPoint�� �������� �ʾҰų� ScheduleNavType ������Ʈ�� �����ϴ�. �̵� ���� �������� �����մϴ�.");
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
        Debug.Log($"���̵� '{idolCharacter.characterName}'�� ��� ������ ó�� �Ϸ�.");
    }

    private void PerformScheduleBundle(ScheduleData dataToExecute, int bundleSizeN, NextScheduleModifiers modifiers, IdolCharacter targetIdolCharacter, Idol targetIdolMovement)
    {
        if (targetIdolCharacter == null)
        {
            Debug.LogError("PerformScheduleBundle: targetIdolCharacter�� null�Դϴ�. �������� ������ �� �����ϴ�.");
            return;
        }

        Debug.Log($"--- ���̵� '{targetIdolCharacter.characterName}': '{dataToExecute.scheduleName}' (���� ũ��: {bundleSizeN}) ���� ���� ---");

        float finalSuccessRate = dataToExecute.baseSuccessRate;
        int baseStatImprovementForBundle = dataToExecute.primaryStatImprovementAmount;
        // ���� ȿ���� �����ۺ��� ������� �ʰų�, ���� ȿ�� ���� �̴� �� �⺻�� ���
        if (!(dataToExecute.applyStatBonusPerItemInBundle && dataToExecute.canFormConsecutiveBundle && bundleSizeN >= dataToExecute.minItemsForConsecutiveEffect))
        {
            baseStatImprovementForBundle *= bundleSizeN;
        }
        // ���� applyStatBonusPerItemInBundle�� true�̰� ���� ȿ�� ���� ���� ��,
        // �⺻ ���� ����� �����۴� �� ���� ����ǰ�, ���� ���ʽ��� �� �����ۿ� �߰��Ǵ� ����̶��
        // baseStatImprovementForBundle = dataToExecute.primaryStatImprovementAmount; (������ ����)
        // �׸��� ���� ���ʽ� �κп��� (dataToExecute.consecutiveStatBonus * bundleSizeN)�� ����.
        // ���� �ڵ�� primaryStatImprovementAmount�� �׻� bundleSize��ŭ ���ϰ�, consecutiveStatBonus�� applyStatBonusPerItemInBundle�� ���� �ٸ��� ����.
        // �� �� ��Ȯ�ϰ� �Ϸ���, ScheduleData�� '�⺻ ���� ��� ���� ũ�⸸ŭ ���� ���ΰ�?'��� ���� bool ���� �߰� ���.
        // ���⼭�� �ϴ� �⺻ ������ ���� ũ�⸸ŭ ���ϰ�, ���� ���ʽ��� ������ ���� ó���ϴ� ������ ����.
        // ��, applyStatBonusPerItemInBundle�� true�̰� ���� ȿ���� �ߵ��ϸ�, �⺻ ������ �� ���� �����ϰ� ���� ���ʽ��� ������ ����ŭ ���ϴ� ���� �� �ڿ������� �� ����.
        // �Ʒ� ������ �����Ͽ� �̸� �ݿ�:
        if (dataToExecute.applyStatBonusPerItemInBundle && dataToExecute.canFormConsecutiveBundle && bundleSizeN >= dataToExecute.minItemsForConsecutiveEffect)
        {
            baseStatImprovementForBundle = dataToExecute.primaryStatImprovementAmount; // ���� ȿ�� �� �⺻�� �� ����
        }
        else
        {
            baseStatImprovementForBundle = dataToExecute.primaryStatImprovementAmount * bundleSizeN;
        }


        int finalStatImprovement = baseStatImprovementForBundle;

        // ��Ʈ���� ��ȭ���� ���� ũ�⿡ �������, �ƴϸ� �������� ���� ��ȹ�� ���� ����. ���⼭�� ��ʷ� ����.
        int finalStressChangeOnSuccess = dataToExecute.stressChangeOnSuccess * bundleSizeN;
        int finalStressChangeOnFailure = dataToExecute.stressChangeOnFailure * bundleSizeN;


        if (modifiers != null)
        {
            if (modifiers.successRateBonus != 0)
            {
                finalSuccessRate += modifiers.successRateBonus;
                Debug.Log($"���� ȿ���� ���� Ȯ�� {modifiers.successRateBonus * 100:F0}%p �����. ���� ������: {finalSuccessRate * 100:F0}%");
            }
            if (modifiers.statToBuff == dataToExecute.primaryTargetStat && modifiers.statBuffAmount != 0)
            {
                finalStatImprovement += modifiers.statBuffAmount;
                Debug.Log($"���� ȿ���� {modifiers.statToBuff} ���� +{modifiers.statBuffAmount} �߰� �����.");
            }
            else if (modifiers.statToBuff != StatType.None && modifiers.statBuffAmount != 0)
            {
                Debug.Log($"���� ȿ���� {modifiers.statToBuff} ���ȿ� +{modifiers.statBuffAmount} ���ʽ��� �־�����, �� ��� ���Ȱ� �޶� ���� �ջ���� ����.");
                // �ʿ�� ���⼭ targetIdolCharacter.Add[StatName]Point(modifiers.statBuffAmount) ���� ȣ��
            }
        }

        if (dataToExecute.canFormConsecutiveBundle && bundleSizeN >= dataToExecute.minItemsForConsecutiveEffect)
        {
            Debug.Log($"'{dataToExecute.scheduleName}' ���� ȿ�� �ߵ�! (���� ũ��: {bundleSizeN})");
            if (dataToExecute.consecutiveSuccessRateModifierPerN != 0)
            {
                int nForPenaltyCalc = bundleSizeN;
                if (dataToExecute.maxNForSuccessRatePenaltyStack > 0 && dataToExecute.consecutiveSuccessRateModifierPerN < 0)
                {
                    nForPenaltyCalc = Mathf.Min(bundleSizeN, dataToExecute.maxNForSuccessRatePenaltyStack);
                }
                float successRateChange = dataToExecute.consecutiveSuccessRateModifierPerN * nForPenaltyCalc;
                finalSuccessRate += successRateChange;
                Debug.Log($"���� ȿ���� ���� Ȯ�� ����: {successRateChange * 100:F0}%p. ���� ������: {finalSuccessRate * 100:F0}%");
            }

            if (dataToExecute.consecutiveStatBonus != 0)
            {
                if (dataToExecute.applyStatBonusPerItemInBundle)
                {
                    finalStatImprovement += (dataToExecute.consecutiveStatBonus * bundleSizeN);
                    Debug.Log($"���� ȿ�� ���� ���ʽ� ����: �� �����۴� +{dataToExecute.consecutiveStatBonus} (�� +{dataToExecute.consecutiveStatBonus * bundleSizeN})");
                }
                else
                {
                    finalStatImprovement += dataToExecute.consecutiveStatBonus;
                    Debug.Log($"���� ȿ�� ���� ���ʽ� ����: ���� ��ü�� +{dataToExecute.consecutiveStatBonus}");
                }
            }
        }
        finalSuccessRate = Mathf.Clamp01(finalSuccessRate);

        bool success = Random.value < finalSuccessRate;
        Debug.Log($"���� ���� Ȯ��: {finalSuccessRate * 100:F0}% > ���: {(success ? "����!" : "����...")}");
        string effectLog = "";

        if (success)
        {
            effectLog += "���� ȿ��: ";
            if (dataToExecute.primaryTargetStat != StatType.None && finalStatImprovement != 0)
            {
                switch (dataToExecute.primaryTargetStat)
                {
                    case StatType.Vocal: targetIdolCharacter.AddVocalPoint(finalStatImprovement); break;
                    case StatType.Dance: targetIdolCharacter.AddDancePoint(finalStatImprovement); break;
                    case StatType.Rap: targetIdolCharacter.AddRapPoint(finalStatImprovement); break;
                    default:
                        Debug.LogWarning($"PerformScheduleBundle: ó������ ���� primaryTargetStat ({dataToExecute.primaryTargetStat})�� ���� ���� ���� ������ IdolCharacter�� �����ϴ�.");
                        break;
                }
                effectLog += $"{dataToExecute.primaryTargetStat} +{finalStatImprovement}, ";
            }
            targetIdolCharacter.ChangeStress(finalStressChangeOnSuccess);
            effectLog += $"��Ʈ���� {(finalStressChangeOnSuccess >= 0 ? "+" : "")}{finalStressChangeOnSuccess}";
        }
        else
        {
            effectLog += "���� ȿ��: ";
            targetIdolCharacter.ChangeStress(finalStressChangeOnFailure);
            effectLog += $"��Ʈ���� {(finalStressChangeOnFailure >= 0 ? "+" : "")}{finalStressChangeOnFailure}";
        }
        Debug.Log(effectLog.TrimEnd(' ', ','));
        Debug.Log($"���̵� '{targetIdolCharacter.characterName}': '{dataToExecute.scheduleName}' (���� ũ��: {bundleSizeN}) ���� �Ϸ�. ���� ���̵� ����: {targetIdolCharacter.GetCurrentStatus()}");
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
            Debug.LogError("������ �������� ���ų�, �ε����� �߸��Ǿ��ų�, �θ� �г�(availableSchedulesPanel)�� �Ҵ���� �ʾҽ��ϴ�!");
            return;
        }
        GameObject newScheduleObject = Instantiate(scheduleItemPrefabs[prefabIndex], availableSchedulesPanel);
    }
}
