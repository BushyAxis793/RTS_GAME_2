using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraControl : MonoBehaviour
{
    public float cameraSpeed, zoomSpeed, groundHeight;
    public Vector2 cameraHeightMinMax;
    public Vector2 cameraRotationMinMax;

    [Range(0, 1)]
    public float zoomLerp = .1f;
    [Range(0, 0.2f)]
    public float cursorTreshhold;

    RectTransform selectionBox;
    new Camera camera;
    Vector2 mousePos, mousePosScreen, keyboardInput, mouseScroll;
    bool isCursorInGameScreen;

    private void Awake()
    {
        selectionBox = GetComponentInChildren<Image>(true).transform as RectTransform;
        camera = GetComponent<Camera>();
    }

    private void Update()
    {
        UpdateMovement();
        UpdateZoom();
        UpdateClicks();
    }


    void UpdateMovement()
    {
        keyboardInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        mousePos = Input.mousePosition;
        mousePosScreen = camera.ScreenToViewportPoint(mousePos);
        isCursorInGameScreen = mousePosScreen.x >= 0 && mousePosScreen.x <= 1 &&
            mousePosScreen.y >= 0 && mousePosScreen.y <= 1;

        Vector2 movementDirection = keyboardInput;

        if (isCursorInGameScreen)
        {
            if (mousePosScreen.x < cursorTreshhold) movementDirection.x -= 1 - mousePosScreen.x / cursorTreshhold;
            if (mousePosScreen.x > 1 - cursorTreshhold) movementDirection.x += 1 - (1 - mousePosScreen.x) / (cursorTreshhold);
            if (mousePosScreen.y < cursorTreshhold) movementDirection.y -= 1 - mousePosScreen.y / cursorTreshhold;
            if (mousePosScreen.y > 1 - cursorTreshhold) movementDirection.y += 1 - (1 - mousePosScreen.y) / (cursorTreshhold);
        }

        var deltaPosition = new Vector3(movementDirection.x, 0, movementDirection.y);
        deltaPosition *= cameraSpeed * Time.deltaTime;
        transform.position += deltaPosition;

    }

    void UpdateZoom()
    {
        mouseScroll = Input.mouseScrollDelta;
        float zoomDelta = mouseScroll.y * zoomSpeed * Time.deltaTime;
        zoomLerp = Mathf.Clamp01(zoomLerp + zoomDelta);

        var position = transform.localPosition;
        position.y = Mathf.Lerp(cameraHeightMinMax.y, cameraHeightMinMax.x, zoomLerp) + groundHeight;
        transform.localPosition = position;

        var rotation = transform.localEulerAngles;
        rotation.x = Mathf.Lerp(cameraRotationMinMax.y, cameraRotationMinMax.x, zoomLerp);
        transform.localEulerAngles = rotation; 

    }

    void UpdateClicks()
    {

        //selectionBox.anchoredPosition = mousePos;
    }
}

