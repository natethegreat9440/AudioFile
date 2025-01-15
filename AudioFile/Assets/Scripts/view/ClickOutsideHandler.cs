using AudioFile.View;
using TagLib.Riff;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;


public class ClickOutsideHandler : MonoBehaviour, IPointerClickHandler
{
    public UIContextMenu contextMenu;

    public Menu alphaMenu;

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

        //Check if the click is outside the alpha menu's tree
        if (alphaMenu != null)
        {
            List<RectTransform> menuTreeBounds = GenerateMenuTreeBounds();

            // Use RectTransformUtility to check if the click is inside the context menu bounds
            foreach (var rect in menuTreeBounds)
            {
                if (!RectTransformUtility.RectangleContainsScreenPoint(
                    rect, eventData.position, eventData.pressEventCamera))
                {
                    alphaMenu.Hide();
                }
            }

            DestroyClickOutsideHandler();
        }
    }

    private List<RectTransform> GenerateMenuTreeBounds()
    {
        List<RectTransform> menuTreeBounds = new List<RectTransform>();

        RectTransform alphaMenuRect = alphaMenu.GetComponent<RectTransform>();
        menuTreeBounds.Add(alphaMenuRect);

        for (int i = 0; i < alphaMenu.MenuComponents.Count; i++)
        {
            MenuComponent child = alphaMenu.MenuComponents[i];
            child.GetComponent<RectTransform>();
            menuTreeBounds.Add(alphaMenuRect);
        }

        return menuTreeBounds;

    }

    public void DestroyClickOutsideHandler()
    {
        //Debug.Log("Click outside detected. Destroying context menu.");
        if (contextMenu != null)
        {
            contextMenu.DestroyContextMenu();
        }
        Destroy(gameObject);
    }
}
