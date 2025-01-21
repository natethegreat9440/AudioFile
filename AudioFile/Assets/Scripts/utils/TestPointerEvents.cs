using UnityEngine;
using UnityEngine.EventSystems;

namespace AudioFile.Utilities
{
    /// <summary>
    /// TestPointerEvents is a test class for testing the Unity IPointerEnterHandler and IPointerExitHandler for comparison to any of AudioFile's implementations of these interfaces
    /// <remarks>
    /// Good for testing/verifying whether an issue is a Unity issue or an AudioFile issue
    /// </remarks>
    /// <see cref="MonoBehaviour"/>
    /// <see also cref="IPointerEnterHandler"/>
    /// <see also cref="IPointerExitHandler"/>
    /// </summary>

    public class TestPointerEvents : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public void OnPointerEnter(PointerEventData eventData)
        {
            Debug.Log("Pointer Enter detected on: " + gameObject.name);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Debug.Log("Pointer Exit detected on: " + gameObject.name);
        }
    }
}
