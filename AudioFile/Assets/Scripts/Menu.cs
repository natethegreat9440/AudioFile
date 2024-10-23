using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine.EventSystems;

namespace AudioFile.View
{
    /// <summary>
    /// Concrete class for a Menu.
    /// <param name="alphaMenu">Has a boolean alphaMenu as a parameter. Alpha menu is the top-most menu button for the top menu strip.</param>
    /// <remarks>
    /// Part of a Composite design pattern implementation. Menu and MenuItems are nodes in the Menu Component tree. Members:
    /// Add(MenuComponent), Remove(MenuComponent), GetChild(int i), GetName(), GetDescription(), Display(), Hide(),
    /// ExecuteAction(), MenuItem_Click() implementations. Implements OnPointerEnter() and OnPointerExit() methods from IPointerEnterHandler and IPointerExitHandler.
    /// </remarks>
    /// <see cref="MenuComponent"/>
    /// <seealso cref="MenuItem"/>
    /// <seealso cref="IPointerEnterHandler"/>
    /// <seealso cref="IPointerExitHandler"/>
    /// </summary>
    public class Menu : MenuComponent, IPointerEnterHandler, IPointerExitHandler
    {
        List<MenuComponent> _menuComponents = new List<MenuComponent>();
        bool _alphaMenu;

        public Menu(Button button, string description, bool alphaMenu = false) : base(button, description)
        {
            _alphaMenu = alphaMenu;
            // Enable Raycast Target for hover detection
            text.raycastTarget = true;

            // Attach the event handling to the Text GameObject
            EventTrigger trigger = text.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = text.gameObject.AddComponent<EventTrigger>();
            }

            // Add OnPointerEnter event
            EventTrigger.Entry pointerEnterEntry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerEnter
            };
            pointerEnterEntry.callback.AddListener((eventData) => { ((IPointerEnterHandler)this).OnPointerEnter((PointerEventData)eventData); });
            trigger.triggers.Add(pointerEnterEntry);

            // Add OnPointerExit event
            EventTrigger.Entry pointerExitEntry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerExit
            };
            pointerExitEntry.callback.AddListener((eventData) => { ((IPointerExitHandler)this).OnPointerExit((PointerEventData)eventData); });
            trigger.triggers.Add(pointerExitEntry);
        }
        public override void Add(MenuComponent menuComponent) => _menuComponents.Add(menuComponent);
        public override void Remove(MenuComponent menuComponent) => _menuComponents.Remove(menuComponent);
        public override MenuComponent GetChild(int i) => _menuComponents[i];

        public override string GetName() => _name;
        public override string GetDescription() => _description;


        public override void Display()
        {
            if (_enabled == true)
            {
                button.gameObject.SetActive(true);

                for (int i = 0; i < this._menuComponents.Count; i++)
                {
                    MenuComponent child = this.GetChild(i);
                    child.button.gameObject.SetActive(true);
                    //child.Display(); Don't call this or else you will get full recursion
                }
            }

        }

        public override void Hide() // Hide method to undisplay the menu components (called on OnPointerExit)
        {

            for (int i = 0; i < this._menuComponents.Count; i++)
            {
                MenuComponent child = this.GetChild(i);
                child.button.gameObject.SetActive(false);
                //child.Hide(); Don't call this or else you will get full recursion
            }

        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            Debug.Log("Pointer Entered: " + this._name);
            Display();
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            Debug.Log("Pointer Exited: " + this._name);

            RectTransform rectTransform = button.GetComponent<RectTransform>();

            Vector2 localMousePosition = rectTransform.InverseTransformPoint(eventData.position);
            Rect rect = rectTransform.rect;

            // Check if mouse is exiting from the bottom and top boundaries only
            if ((localMousePosition.y > rect.yMax || localMousePosition.y < rect.yMin) && !_alphaMenu)
            {
                Debug.Log("Pointer Exited from the top/bottom boundary: " + this._name);
                Hide();
            }
            // Check if mouse is exiting from the sides and top boundaries only
            else if ((localMousePosition.x < rect.xMin || localMousePosition.x > rect.xMax || localMousePosition.y > rect.yMax) && _alphaMenu)
            {
                Debug.Log("Pointer Exited from the top/left/right boundary: " + this._name);
                Hide();

            }
        }
    }
}
