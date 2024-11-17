using UnityEngine;

namespace AudioFile.Utilities
{
    public static class GameObjectExtensions
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
    }
}
