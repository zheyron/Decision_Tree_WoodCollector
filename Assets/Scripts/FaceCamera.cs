using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    private Transform mainCamTransform;

    private void Start()
    {
        // Find the main camera's transform when the object is enabled/starts.
        // Ensure your main camera has the "MainCamera" tag.
        if (Camera.main != null)
        {
            mainCamTransform = Camera.main.transform;
        }
        else
        {
            Debug.LogError("Main Camera not found! Please tag your camera as 'MainCamera'.");
        }
    }

    // Use LateUpdate for smoother camera following after all other updates are done
    private void LateUpdate()
    {
        if (mainCamTransform != null)
        {
            // Make the object look at the camera's position
            transform.LookAt(mainCamTransform.position);

            // Text is often imported facing the opposite direction, so an 
            // additional 180-degree rotation is needed to display it correctly
            transform.Rotate(0, 180, 0);
        }
    }
}