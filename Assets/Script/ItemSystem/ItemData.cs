using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "MyGame/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("기본 정보")]
    public string itemName;
    [TextArea(3, 5)]
    public string description;

    //사용 대상(방에 사용,아이돌에게 사용,복도에 사용)
    public ApplyItemTarget applyItemTarget;
    
    
}
