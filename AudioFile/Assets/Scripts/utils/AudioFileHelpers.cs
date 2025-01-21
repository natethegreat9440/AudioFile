using UnityEngine;

namespace AudioFile.Utilities
{
    /// <summary>
    /// Assorted helper functions used in AudioFile for various purposes
    /// <remarks>
    /// Members: FindInChildren() which is useful for finding a child Unity game object by name. SetHexColor() which is useful for setting a color from a hex string.
    /// </remarks>
    /// </summary>
    public static class AudioFileHelpers
    {
        public static GameObject FindInChildren(this GameObject parent, string name)
        {
            Transform[] children = parent.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                if (child.name == name)
                {
                    return child.gameObject;
                }
            }
            return null;
        }

        public static Color SetHexColor(string hexColor)
        {
            if (ColorUtility.TryParseHtmlString(hexColor, out Color newColor))
            {
                return newColor;

            }
            else
            {
                Debug.LogError("Invalid hex color string");
                return Color.magenta;
            }
        }
    }
}
