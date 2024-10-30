using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
using AudioFile.Controller;

namespace AudioFile.View
{
    /// <summary>
    /// Abstract base class for all Menu Components.
    /// <param name="button">Has a Unity Button GameObject component as a parameter.</param>
    /// <param name="description">Has a string description as a parameter.</param>
    /// <remarks>
    /// Part of a Composite design pattern implementation. Menu and MenuItems are nodes in the tree. Members:
    /// Add(MenuComponent), Remove(MenuComponent), GetChild(int i), GetName(), GetDescription(), Display(), Hide(),
    /// ExecuteAction(), MenuItem_Click(). Has default ToString() override and IsEnabled() and SetEnabled(bool enabled) implementations.
    /// </remarks>
    /// </summary>
    public abstract class MenuComponent
    {
        public Button button;
        public Text text;
        protected string _name;
        protected string _description;
        protected bool _enabled = true;

        public MenuComponent(Button button, string description)
        {
            this.button = button;
            _name = button.GetComponentInChildren<Text>().text;
            text = button.GetComponentInChildren<Text>();
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
}
