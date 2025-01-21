using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Runtime.CompilerServices;
using AudioFile.Utilities;


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

        bool _isContextItem;

        public MenuItem Initialize(Button button, string label, string description, ICommand command, bool isContextItem = false)
        {
            base.Button = button;
            base.Text = Button.GetComponentInChildren<Text>();
            base.Description = description;
            base.Name = label;

            base.Text.text = " " + label; //Extra space as buffer as I don't like how close the text starts to the left button border by default
            base.Button.onClick.AddListener(MenuItem_Click); // Wire up the event handler
            _command = command;

            _isContextItem = isContextItem;

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
            //This code ensures that a new command instance gets created each time the Menu Item is clicked/Action is executed rather than
            //just the same command instance used for instantiating the MenuItem as this can lead to undesired behavior
            if (_enabled && _command != null && !_isContextItem)
            {
                Type commandType = _command.GetType();
                ICommand newCommand = (ICommand)Activator.CreateInstance(commandType);
                newCommand.Execute();
            }
            else if (_enabled && _command != null && _isContextItem) //Because context menu items are created and destroyed with each right click and menu exit there already is a new command instance created each time with the correct proeprties
            {
                _command.Execute();
            }
        }

        //Keeping this redundant method in for now in case we want to be able to add functionality later in order to navigate the Menu's and execute commands with just the keyboard
        public override void MenuItem_Click() 
            {
                this.ExecuteAction();
            }
        }
}
