using AudioFile.View;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickOutsideHandler : MonoBehaviour, IPointerClickHandler
{
    public UIContextMenu contextMenu;

    public void OnPointerClick(PointerEventData eventData)
    {

        // Check if the click is outside the context menu
        if (contextMenu != null)
        {
            RectTransform contextMenuRect = contextMenu.GetComponent<RectTransform>();

            // Use RectTransformUtility to check if the click is inside the context menu bounds
            if (!RectTransformUtility.RectangleContainsScreenPoint(
                contextMenuRect, eventData.position, eventData.pressEventCamera))
            {
                DestroyClickOutsideHandler();
            }
        }
    }

    public void DestroyClickOutsideHandler()
    {
        //Debug.Log("Click outside detected. Destroying context menu.");
        contextMenu.DestroyContextMenu();
        Destroy(gameObject);
    }
}
