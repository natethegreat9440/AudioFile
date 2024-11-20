using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.UIElements;

namespace AudioFile.View
{
    public class UIDisplayVisibilitySlider : MonoBehaviour
    {
        public Slider Display_Slider; //Attach this script to the slider in the inspector
        public Button Display_Toggle_Button; //Drag this here in the inspector
        public GameObject Track_List_Display; //Drag this here in the inspector

        private Renderer targetRenderer;
        private Material targetMaterial;

        bool IsTrackDisplayActive { get => Display_Toggle_Button.GetComponent<UIToggleDisplay>().IsTrackDisplayActive; }

        //const float alphaMinThreshold = 0.25f;
        //const float alphaMaxThreshold = 0.95f;

        void Start()
        {
            if (Display_Slider != null)
            {
                // Set up a listener for the slider's value change event
                Display_Slider.onValueChanged.AddListener(UpdateTransparency);
            }
        }

        void UpdateTransparency(float value)
        {
            if (Track_List_Display != null)
            {
                SetTransparencyIterative(Track_List_Display.transform, value);
                //Code to add if I decide I want the ends of the slider to toggle Track Display active state again
                /*if (value <= alphaMinThreshold && IsTrackDisplayActive)
                {
                    Display_Toggle_Button.GetComponent<UIToggleDisplay>().ToggleDisplay();
                }

                else if (value > alphaMinThreshold && !IsTrackDisplayActive)
                {
                    Display_Toggle_Button.GetComponent<UIToggleDisplay>().ToggleDisplay();
                }

                else
                {
                    // Apply transparency to the Scroll View and all its children
                    SetTransparencyIterative(Track_List_Display.transform, value);
                }*/
            }
        }

        void SetTransparencyIterative(Transform root, float alpha)
        {
            //TODO: Figure out how to get this method to 
            Stack<Transform> stack = new Stack<Transform>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                Transform current = stack.Pop();

                var image = current.GetComponent<Image>();
                if (image != null)
                {
                    Color color = image.color;
                    color.a = alpha;
                    image.color = color;
                }

                foreach (Transform child in current)
                {
                    stack.Push(child);
                }
            }
        }

        void OnDestroy()
        {
            // Remove the listener to avoid memory leaks
            if (Display_Slider != null)
            {
                Display_Slider.onValueChanged.RemoveListener(UpdateTransparency);
            }
        }
    }
}
