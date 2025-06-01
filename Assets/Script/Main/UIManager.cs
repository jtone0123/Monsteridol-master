// UIManager.cs
// Unity UI �г� ��ȯ�� �����ϴ� ��ũ��Ʈ�Դϴ�.
// �� ��ũ��Ʈ�� Canvas�� �� GameObject("UIManager" ��)�� �߰��մϴ�.

using UnityEngine;
using UnityEngine.UI; // Button�� ���� UI ��ҿ� �����Ϸ��� �ʿ��� �� �ֽ��ϴ�.
using TMPro;

public class UIManager : MonoBehaviour
{
    // Inspector â���� ������ �гε�
    [Header("UI Panels")] // Inspector���� ���� ���� �׷�ȭ
    public GameObject mainMenuPanel;          // ���� �޴� �г�
    public GameObject scheduleManagementPanel; // ������ ���� �г�
   // public TextMeshProUGUI currentTurnText; // ���� �� �ؽ�Ʈ UI
    // �ʿ信 ���� �ٸ� �гε鵵 ���⿡ �߰��� �� �ֽ��ϴ�.
    // public GameObject idolDetailPanel;
    // public GameObject settingsPanel;
    //int currentTurn = 0;

    //public void CurrentTurnUpdate()
    //{
     //   currentTurn++;
     //   currentTurnText.text = $"<size=200>{currentTurn}</size>";
   // }
    void Start()
    {

        //CurrentTurnUpdate();

        // ���� ���� �� �ʱ� ���� ����: ���� �޴��� Ȱ��ȭ
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("���� �޴� �г��� UIManager�� �Ҵ���� �ʾҽ��ϴ�!");
        }

        if (scheduleManagementPanel != null)
        {
            //scheduleManagementPanel.SetActive(false); // ������ ���� �г��� ó������ ��Ȱ��ȭ
        }
        else
        {
            Debug.LogError("������ ���� �г��� UIManager�� �Ҵ���� �ʾҽ��ϴ�!");
        }

        // �ٸ� �гε鵵 �ʱ⿡�� ��Ȱ��ȭ
        // if (idolDetailPanel != null) idolDetailPanel.SetActive(false);
        // if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    // ������ ���� �г��� �����ִ� �Լ�
    // �� �Լ��� ���� �޴��� "������ ����" ��ư�� OnClick �̺�Ʈ�� �����մϴ�.
    public void ShowScheduleManagementPanel()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false); // ���� �޴� �г� �����
        }

        if (scheduleManagementPanel != null)
        {
            scheduleManagementPanel.SetActive(true); // ������ ���� �г� ���̱�
        }
        else
        {
            Debug.LogError("������ ���� �г��� ã�� �� �����ϴ�. UIManager�� �Ҵ�Ǿ����� Ȯ�����ּ���.");
        }
    }

    // ���� �޴� �г��� �����ִ� �Լ�
    // �� �Լ��� ������ ���� �г��� "�ڷΰ���" �Ǵ� "��������" ��ư�� OnClick �̺�Ʈ�� ������ �� �ֽ��ϴ�.
    public void ShowMainMenuPanel()
    {
        if (scheduleManagementPanel != null)
        {
            scheduleManagementPanel.SetActive(false); // ������ ���� �г� �����
        }

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true); // ���� �޴� �г� ���̱�
        }
        else
        {
            Debug.LogError("���� �޴� �г��� ã�� �� �����ϴ�. UIManager�� �Ҵ�Ǿ����� Ȯ�����ּ���.");
        }
    }

    // (���� ����) �ٸ� �гε��� ���� �Լ���
    // public void ShowIdolDetailPanel()
    // {
    //     if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
    //     if (idolDetailPanel != null) idolDetailPanel.SetActive(true);
    // }

    // public void ShowSettingsPanel()
    // {
    //     if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
    //     if (settingsPanel != null) settingsPanel.SetActive(true);
    // }

    // ��� Ȱ�� �г��� �ݰ� ���� �޴��� ���ư��� �Լ� (�ϰ� ó����)
    public void CloseAllPanelsAndShowMain()
    {
        if (scheduleManagementPanel != null) scheduleManagementPanel.SetActive(false);
        // if (idolDetailPanel != null) idolDetailPanel.SetActive(false);
        // if (settingsPanel != null) settingsPanel.SetActive(false);

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("���� �޴� �г��� ã�� �� �����ϴ�. UIManager�� �Ҵ�Ǿ����� Ȯ�����ּ���.");
        }
    }
}
