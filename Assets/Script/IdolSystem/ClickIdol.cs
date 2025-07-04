using UnityEngine;

public class ClickIdol : MonoBehaviour, IClickable
{
    public void OnClick()
    {
        CameraManager.Instance.SetCameraTarget(transform,2f);
        CameraManager.Instance.ZoomInToCharacter();
        UIManager.instance.ShowPanelByIndex(1);
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
