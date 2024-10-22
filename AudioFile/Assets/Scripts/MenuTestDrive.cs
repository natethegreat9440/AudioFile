using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace AudioFile
{
    public class MenuTestDrive : MonoBehaviour
    {
        void Start()
        {
            GameObject fileGameObject = GameObject.Find("File");
            GameObject newGameObject = GameObject.Find("New");
            GameObject openGameObject = GameObject.Find("Open");
            GameObject optionsGameObject = GameObject.Find("Options");
            GameObject newChildGameObject = GameObject.Find("New Child");
            GameObject openChildGameObject = GameObject.Find("Open Child");
            GameObject option1GameObject = GameObject.Find("Option 1");
            GameObject option2GameObject = GameObject.Find("Option 2");
            GameObject option3GameObject = GameObject.Find("Option 3");

            Text fileTextRef = fileGameObject.GetComponent<Text>();
            Text newTextRef = newGameObject.GetComponent<Text>();
            Text openTextRef = openGameObject.GetComponent<Text>();
            Text optionsTextRef = optionsGameObject.GetComponent<Text>();
            Text newChildTextRef = newChildGameObject.GetComponent<Text>();
            Text openChildTextRef = openChildGameObject.GetComponent<Text>();
            Text option1TextRef = option1GameObject.GetComponent<Text>();
            Text option2TextRef = option2GameObject.GetComponent<Text>();
            Text option3TextRef = option3GameObject.GetComponent<Text>();

            Menu fileMenu = new Menu("File", "File Menu", fileTextRef);
            Menu newMenu = new Menu("New", "New Menu", newTextRef);
            Menu openMenu = new Menu("Open", "Open Menu", openTextRef);
            Menu optionsMenu = new Menu("Options", "Options Menu", optionsTextRef);
            Menu newChildMenu = new Menu("New Child", "New Child Menu", newChildTextRef);
            Menu openChildMenu = new Menu("Open Child", "Open Child Menu", openChildTextRef);
            Menu option1Menu = new Menu("Option 1", "Option 1 Menu", option1TextRef);
            Menu option2Menu = new Menu("Option 2", "Option 2 Menu", option2TextRef);
            Menu option3Menu = new Menu("Option 3", "Option 3 Menu", option3TextRef);


            fileMenu.Add(newMenu);
            fileMenu.Add(openMenu);
            fileMenu.Add(optionsMenu);

            newMenu.Add(newChildMenu);
            openMenu.Add(openChildMenu);

            optionsMenu.Add(option1Menu);
            optionsMenu.Add(option2Menu);
            optionsMenu.Add(option3Menu);

            newTextRef.gameObject.SetActive(false);
            newChildTextRef.gameObject.SetActive(false);
            openTextRef.gameObject.SetActive(false);
            openChildTextRef.gameObject.SetActive(false);
            optionsTextRef.gameObject.SetActive(false);
            option1TextRef.gameObject.SetActive(false);
            option2TextRef.gameObject.SetActive(false);
            option3TextRef.gameObject.SetActive(false);


        }
    }
}
