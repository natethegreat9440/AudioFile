using TMPro;
using System.Collections.Generic;
using System.Threading;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
using AudioFile.Controller;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace AudioFile.View
{
    /// <summary>
    /// Abstract base class for all Menu Components.
    /// <param name="button">Has a Unity Button GameObject component as a parameter.</param>
    /// <param name="description">Has a string description as a parameter.</param>
    /// <remarks>
    /// Part of a Composite design pattern implementation. Menu and MenuItems are nodes in the tree. Members:
    /// Name, Description, _enabled, Add(MenuComponent), Remove(MenuComponent), GetChild(int i), Display(), Hide(),
    /// ExecuteAction(), MenuItem_Click(). Has default ToString() override and IsEnabled() and SetEnabled(bool enabled) implementations.
    /// </remarks>
    /// </summary>
    public abstract class MenuComponent : MonoBehaviour
    {
        public Button Button;
        public Text Text;

        protected string Name { get; set; }
        protected string Description { get; set; }
        protected bool _enabled = true;

        private void Awake()
        {
            if (Button != null)
            {
                Text = Button.GetComponentInChildren<Text>();
            }
        }

        public override string ToString()
        {
            return $"{Name} - {Description}";
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

        protected bool IsEnabled()
        {
            return _enabled;
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
