using AudioFile.View;
using TagLib.Riff;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

namespace AudioFile.View
{
    /// <summary>
    /// View class for managing the ClickOutsideHandler used by Menu objects.
    /// <remarks>
    /// Handle the user clicking outside of the menu tree bounds when initialized. Works with the Context Menu and Alpha Menus from the top ribbon. Destroys the ClickOutsideHandler object whenever a click outside is detected.
    /// Members: GenerateMenuTreeBounds(), DestroyClickOutsideHandler(). Implements OnPointerClick() from IPointerClickHandler from UnityEngine.EventSystems.
    /// </remarks>
    /// <see cref="MonoBehaviour"/>
    /// <see cref="UIContextMenu"/>
    /// <see cref="Menu"/>
    /// <see also cref="IPointerClickHandler"/>
    /// </summary>
    public class ClickOutsideHandler : MonoBehaviour, IPointerClickHandler
    {
        public UIContextMenu contextMenu;

        public Menu alphaMenu;

        public void OnPointerClick(PointerEventData eventData)
        {
            // Check if the click is outside the context menu
            if (contextMenu != null)
            {
                HandleClickOutSideContextMenu(eventData);
            }

            //Check if the click is outside the alpha menu's tree
            if (alphaMenu != null)
            {
                HandleClickOutSideAlphaMenu(eventData);
            }
        }

        private void HandleClickOutSideContextMenu(PointerEventData eventData)
        {
            RectTransform contextMenuRect = contextMenu.GetComponent<RectTransform>();

            // Use RectTransformUtility to check if the click is inside the context menu bounds
            if (!RectTransformUtility.RectangleContainsScreenPoint(
                contextMenuRect, eventData.position, eventData.pressEventCamera))
            {
                DestroyClickOutsideHandler();
            }
        }
        private void HandleClickOutSideAlphaMenu(PointerEventData eventData)
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
}
