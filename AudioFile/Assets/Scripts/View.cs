using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
//using UnityEngine.UIElements; Having this line in causes ambiguous references for Buttons etc. This library is for newer buttons whereas UnityEngine.UI has all of the legacy UI elements

namespace AudioFile
{
    public abstract class MenuComponent
    {
        public Button _button;
        public Text _text;
        protected string _name;
        protected string _description;
        protected bool _enabled = true;

        public MenuComponent(Button button, string description) 
        {
            _button = button;
            _name = _button.GetComponentInChildren<Text>().text;
            _text = _button.GetComponentInChildren<Text>();
            _description = description;
        }
        public override string ToString()
        {
            return $"{_name} - {_description}";
        }
        public virtual void Add(MenuComponent menuComponent)
        {
            throw new NotImplementedException();
        }

        public virtual void Remove(MenuComponent menuComponent)
        {
            throw new NotImplementedException();
        }

        public virtual MenuComponent GetChild(int i)
        {
            throw new NotImplementedException();
        }

        public virtual string GetName()
        {
            throw new NotImplementedException();
        }

        public virtual string GetDescription()
        {
            throw new NotImplementedException();
        }

        protected bool IsEnabled()
        {
            if (_enabled == true) return true;
            else return false;
        }

        protected void SetEnabled(bool enabled)
        {
            _enabled = enabled;
        }

        public virtual void Display()
        {
            Debug.Log("Display called on a MenuComponent interface");
            throw new NotImplementedException();
        }

        public virtual void Hide()
        {
            throw new NotImplementedException();
        }

        public virtual void ExecuteAction()
        {
            throw new NotImplementedException();
        }

        public virtual void MenuItem_Click()
        {
            throw new NotImplementedException();
        }

    }

    public class MenuItem : MenuComponent
    {
        ICommand _command;

        public MenuItem(Button button, string description, ICommand command) : base(button, description)
        {
            _button.onClick.AddListener(MenuItem_Click); // Wire up the event handler
            _command = command;
        }

        public override string GetName()
        {
            return _name;
        }

        public override string GetDescription()
        {
            return _description;
        }

        public override void Display()
        {
            if (_enabled == true)
            {
                _button.gameObject.SetActive(true); //set button when Display is called
            }
            else
            {
                _button.enabled = false;
            }
        }

        public override void Hide()
        {
            _button.gameObject.SetActive(false); //set button when Hide is called
        }

        public override void ExecuteAction()
        {
            if (_enabled && _command != null)
            {
                _command.Execute();
            }
        }

        public override void MenuItem_Click()
        {
            this.ExecuteAction();
        }

    }

    public class Menu : MenuComponent, IPointerEnterHandler, IPointerExitHandler
    {
        List<MenuComponent> _menuComponents = new List<MenuComponent>();

        public Menu(Button button, string description) : base(button, description)
        {
            // Enable Raycast Target for hover detection
            _text.raycastTarget = true;

            // Attach the event handling to the Text GameObject
            EventTrigger trigger = _text.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = _text.gameObject.AddComponent<EventTrigger>();
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
                _button.gameObject.SetActive(true);

                for (int i = 0; i < this._menuComponents.Count; i++)
                {
                    MenuComponent child = this.GetChild(i);
                    child._button.gameObject.SetActive(true);
                    //child.Display(); Don't call this or else you will get full recursion
                }
            }

        }

        public override void Hide() // Hide method to undisplay the menu components (called on OnPointerExit)
        {
            _text.gameObject.SetActive(false);
            foreach (var component in _menuComponents)
            {
                component.Hide(); // Assuming child components have a Hide() method
            }
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            Debug.Log("Pointer Entered: " + this._name);
            Display();
            /*for (int i = 0; i < _menuComponents.Count; i++)
            {
                _menuComponents[i].Display();
            }
            */
            /*foreach (var component in _menuComponents)
            {
                component.Display(); // Display only direct children
            }*/
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            //Debug.Log("Pointer Exited: " + this._name);
            //this.Hide();

            RectTransform rectTransform = _text.GetComponent<RectTransform>();

            Vector2 localMousePosition = rectTransform.InverseTransformPoint(eventData.position);
            Rect rect = rectTransform.rect;

            // Check if mouse is exiting from the bottom boundary only
            /*if (localMousePosition.y < rect.yMin)
            {
                Debug.Log("Pointer Exited from the bottom boundary: " + this._name);
                foreach (var component in _menuComponents)
                {
                    component.Display(); // Hide only direct children
                }
            }*/
        }
    }
}