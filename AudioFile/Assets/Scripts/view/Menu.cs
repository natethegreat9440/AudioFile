using TMPro;
using System;
using System.Threading;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using TagLib.Flac;
using UnityEngine.Rendering.VirtualTexturing;

namespace AudioFile.View
{
    /// <summary>
    /// Concrete class for a Menu.
    /// <param name="_isAlphaMenu">Has a boolean isAlphaMenu as a parameter. Alpha menu is the top-most menu button for the top menu strip.</param>
    /// <remarks>
    /// Part of a Composite design pattern implementation. Menu and MenuItems are nodes in the Menu Component tree. 
    /// Members: Implements Add(MenuComponent), Remove(MenuComponent), GetChild(int i), Display(), Hide(),
    /// from abstract base class MenuComponent. Implements OnPointerEnter() and OnPointerExit() methods from IPointerEnterHandler and IPointerExitHandler.
    /// </remarks>
    /// <see cref="MenuComponent"/>
    /// <seealso cref="MenuItem"/>
    /// <seealso cref="IPointerEnterHandler"/>
    /// <seealso cref="IPointerExitHandler"/>
    /// </summary>
    public class Menu : MenuComponent, IPointerEnterHandler, IPointerExitHandler
    {
        public List<MenuComponent> MenuComponents = new List<MenuComponent>();

        bool _isAlphaMenu;

        //These correction factors are used to correct for IPointerExitHandler being inconsistent about logging the proper mouse position once an OnPointerExit event is fired
        //which can cause bugs with how Hide() is called once the mouse has exited from either the yMax or ymin position
        //Use them on a case by case basis after using the debug statements in OnPointerExit to see what the correction factors should be
        //Context menu's seem to need a +1f correction factor to correct yMin
        float _correctYMax; 
        float _correctYMin;

        public Canvas mainCanvas;

        private ClickOutsideHandler ClickOutsideHandler;

        public Menu Initialize(Button button, string label, string description, bool isAlphaMenu = false, float correctYMax = 0f, float correctYMin = 0f)
        {
            base.Button = button;
            base.Text = Button.GetComponentInChildren<Text>();
            base.Description = description;
            base.Name = label;

            _isAlphaMenu = isAlphaMenu;

            _correctYMax = correctYMax;
            _correctYMin = correctYMin;

            base.Text.text = " " + label; //Extra space as buffer as I don't like how close the text starts to the left button border by default

            base.Text.raycastTarget = true;

            return this;
        }

        public override void Add(MenuComponent menuComponent) => MenuComponents.Add(menuComponent);
        public override void Remove(MenuComponent menuComponent) => MenuComponents.Remove(menuComponent);
        public override MenuComponent GetChild(int i) => MenuComponents[i];

        public override void Display()
        {
            if (_enabled == true)
            {
                Button.gameObject.SetActive(true);

                for (int i = 0; i < this.MenuComponents.Count; i++)
                {
                    MenuComponent child = this.GetChild(i);
                    child.Button.gameObject.SetActive(true);

                    //Set as last sibling makes it so the menu renders above the click catcher so the click 
                    //catcher doesn't intercept mouse behavior
                    //this.gameObject.transform.SetAsLastSibling();

                    Debug.Log($"{child} was displayed");
                    //child.Display(); Don't call this or else you will get full recursion
                }
            }
        }

        public override void Hide() // Hide method to undisplay the menu components (called on OnPointerExit)
        {
            for (int i = 0; i < this.MenuComponents.Count; i++)
            {
                MenuComponent child = this.GetChild(i);
                child.Button.gameObject.SetActive(false);
                Debug.Log($"{child} was hidden");
                child.Hide(); //Recursively calls all children under the menu and children of any sub-menus
            }
        }

        //IMPORTANT NOTE: OnPointerEnter and OnPointerExit will not work as expected unless anchors and pivot for Menus are centered 
        //using x=0.5, y=0.5 within the Unity scene using the Inspector. If these are set properly and bugs are occurring try
        //adding float tolerance = 0.5f or similar to OnPointerExit and subtracting/adding tolerance to rect.yMin/rect.yMax respectively.
        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            //Debug.Log("Pointer Entered: " + this.Name);
            if (this._isAlphaMenu)
            {
                CreateClickCatcher();
            }
            Display();
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            //Debug.Log("Pointer Exited: " + this.Name);

            RectTransform rectTransform = Button.GetComponent<RectTransform>();

            Vector2 localMousePosition = rectTransform.InverseTransformPoint(eventData.position);
            Rect rect = rectTransform.rect;

            // Check if mouse is exiting from the bottom and top boundaries only
            if ((localMousePosition.y > rect.yMax + _correctYMax || localMousePosition.y < rect.yMin + _correctYMin) && !_isAlphaMenu)
            {
                Hide();
                //This commented out block is only for debugging. Keep here if needed for future use.
                /*if (localMousePosition.y > rect.yMax + _correctYMax)
                {
                    Debug.Log("Pointer Exited from the top boundary: " + this.Name);
                    Debug.Log($"localMousePosition.y: {localMousePosition.y} rect.yMax: {rect.yMax} localMousePosition.y > rect.yMax??");

                }
                else if (localMousePosition.y < rect.yMin + _correctYMin)
                {
                    Debug.Log("Pointer Exited from the bottom boundary: " + this.Name);
                    Debug.Log($"localMousePosition.y: {localMousePosition.y} rect.yMin: {rect.yMin} localMousePosition.y < rect.yMin??");
                }
                else
                {
                    Debug.Log("Something went wrong: " + this.Name);
                    Debug.Log($"localMousePosition.y: {localMousePosition.y} rect.yMin: {rect.yMin} rect.yMax: {rect.yMax}");
                }*/
            }
            // Check if mouse is exiting from the sides and top boundaries only
            else if ((localMousePosition.x < rect.xMin || localMousePosition.x > rect.xMax || localMousePosition.y > rect.yMax + _correctYMax) && _isAlphaMenu)
            {
                Debug.Log("Pointer Exited from the top/left/right boundary: " + this.Name);
                Hide();


                if (ClickOutsideHandler != null)
                {
                    ClickOutsideHandler.DestroyClickOutsideHandler();
                }
            }
            else
            {
                //This commented out block is only for debugging.Keep here if needed for future use.
                Debug.Log("Something went really wrong: " + this.Name);
                Debug.Log($"localMousePosition.y: {localMousePosition.y} rect.yMin: {rect.yMin} rect.yMax: {rect.yMax}");
            }

        }

        //ClickCatcher is used to detect clicks outside the menu's transform
        private void CreateClickCatcher()
        {
            // Create the overlay for detecting clicks outside the menu
            mainCanvas = FindObjectOfType<Canvas>();
            GameObject overlay = new GameObject("ClickCatcherOverlay");
            overlay.transform.SetParent(mainCanvas.transform, false);
            RectTransform overlayRect = overlay.AddComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;

            Image overlayImage = overlay.AddComponent<Image>();
            overlayImage.color = new Color(0, 0, 0, 0); // Fully transparent
            overlayImage.raycastTarget = true;

            ClickOutsideHandler clickOutsideHandler = overlay.AddComponent<ClickOutsideHandler>();
            clickOutsideHandler.alphaMenu = this; 
            ClickOutsideHandler = clickOutsideHandler;
            ClickOutsideHandler.gameObject.transform.SetAsFirstSibling();
        }
    }
}
