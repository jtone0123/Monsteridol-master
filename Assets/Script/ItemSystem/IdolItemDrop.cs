using UnityEngine;

public class IdolItemDrop : MonoBehaviour
{

    public ItemData item;
    public IdolCharacter idolstate;

    private void Awake()
    {
        idolstate = GetComponent<IdolCharacter>();
    }
    public void ApplyItemdata(ItemData itemData)
    {
        
    }

}
