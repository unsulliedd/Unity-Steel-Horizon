using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CompassManager : MonoBehaviour
{
    public RectTransform compassBarTransform;

    public RectTransform objectiveMarkerTransform;
    public RectTransform northMarkerTransform;
    public RectTransform southMarkerTransform;
    public RectTransform westMakerTrasform;
    public RectTransform eastMakerTrasform;

    public Transform cameraObjectTransform;
    public Transform objectiveObjectTransform;

    void Start()
    {

    }

    void Update()
    {
        SetMarkerPosition(objectiveMarkerTransform, objectiveObjectTransform.position);
        SetMarkerPosition(northMarkerTransform, cameraObjectTransform.position + Vector3.forward * 1000);
        SetMarkerPosition(westMakerTrasform, cameraObjectTransform.position + Vector3.left * 1000);
        SetMarkerPosition(eastMakerTrasform, cameraObjectTransform.position + Vector3.right * 1000);
        SetMarkerPosition(southMarkerTransform, cameraObjectTransform.position + Vector3.back * 1000);
    }

    private void SetMarkerPosition(RectTransform markerTransform, Vector3 worldPosition)
    {
        Vector3 directionToTarget = worldPosition - cameraObjectTransform.position;
        float angle = Vector2.SignedAngle(new Vector2(cameraObjectTransform.forward.x, cameraObjectTransform.forward.z), new Vector2(directionToTarget.x, directionToTarget.z));

        // Normalize the angle between -180 and 180
        if (angle < -180) angle += 360;
        if (angle > 180) angle -= 360;

        float compassPositionX = angle / 180.0f; // Map the angle to -1 to 1 range
        markerTransform.anchoredPosition = new Vector2(compassBarTransform.rect.width / 2 * compassPositionX, 0);
    }
}
