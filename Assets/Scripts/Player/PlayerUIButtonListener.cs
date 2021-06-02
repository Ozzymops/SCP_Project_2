using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayerUIButtonListener : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public PlayerManager linkedPlayer;
    public int thisItemIndex;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (thisItemIndex < linkedPlayer.inventoryScript.items.Count)
            {
                linkedPlayer.uiScript.showInventory = false;
                linkedPlayer.inventoryScript.Cmd_HoldItem(thisItemIndex);
                linkedPlayer.InventoryToolTip(-1);
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (thisItemIndex < linkedPlayer.inventoryScript.items.Count)
            {             
                linkedPlayer.inventoryScript.Cmd_DropItem(thisItemIndex);
                linkedPlayer.InventoryToolTip(-1);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (thisItemIndex < linkedPlayer.inventoryScript.items.Count)
        {
            linkedPlayer.InventoryToolTip(thisItemIndex);
        }      
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        linkedPlayer.InventoryToolTip(-1);
    }
}
