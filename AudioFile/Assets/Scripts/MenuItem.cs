using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace AudioFile.View
{
    /// <summary>
    /// Concrete class for a Menu Item.
    /// <param name="command">Has a ICommand object as a parameter.</param>
    /// <remarks>
    /// Part of a Composite design pattern implementation. Menu and MenuItems are nodes in the Menu Component tree. 
    /// ExecuteAction() executes a command as part of a Command design pattern. Members: Implements
    /// Display(), Hide(), ExecuteAction(), MenuItem_Click() from abstract base class MenuComponent.
    /// </remarks>
    /// <see cref="MenuComponent"/>
    /// <seealso cref="Menu"/>
    /// <seealso cref="ICommand"/>
    /// </summary>
    public class MenuItem : MenuComponent
    {
        ICommand _command;

        /*public MenuItem(Button button, string label, string description, ICommand command) : base(button, description)
        {
            button.onClick.AddListener(MenuItem_Click); // Wire up the event handler
            _command = command;
        }*/

        public MenuItem Initialize(Button button, string label, string description, ICommand command)
        {
            base.Button = button;
            base.Text = Button.GetComponentInChildren<Text>();
            base.Description = description;
            base.Name = label;

            base.Text.text = " " + label; //Extra space as buffer as I don't like how close the text starts to the left button border by default
            base.Button.onClick.AddListener(MenuItem_Click); // Wire up the event handler
            _command = command;

            return this;
        }

        public override void Display()
        {
            if (_enabled == true)
            {
                Button.gameObject.SetActive(true); //set button when Display is called
            }
            else
            {
                Button.enabled = false;
            }
        }

        public override void Hide()
        {
            Button.gameObject.SetActive(false); //set button when Hide is called
        }

        public override void ExecuteAction()
        {
            if (_enabled && _command != null)
            {
                _command.Execute();
            }
        }

        //Keeping this redundant method in for now in case we want to be able to add functionality later in order to 
        //navigate the Menu's and execute commands with just the keyboard
        public override void MenuItem_Click() 
        {
            this.ExecuteAction();
        }
    }
}
