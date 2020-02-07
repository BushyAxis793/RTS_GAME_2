using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraControl : MonoBehaviour
{
    static CameraControl cameraControl;

    public float cameraSpeed, zoomSpeed, groundHeight;
    public Vector2 cameraHeightMinMax;
    public Vector2 cameraRotationMinMax;

    [Range(0, 1)]//zakres liczbowy
    public float zoomLerp = .1f;
    [Range(0, 0.2f)]//zakres liczbowy
    public float cursorTreshhold;

    RectTransform selectionBox;//dostęp do RectTransform
    new Camera camera;//nowa zmienna typu Camera
    Vector2 mousePos, mousePosScreen, keyboardInput, mouseScroll;//kilka zmiennych tego samego typu
    bool isCursorInGameScreen;
    Rect selectionRect, boxRect;//zmienne typu Rect
    List<ISelectable> selectedUnits = new List<ISelectable>();//lista "selectedUnits typu Unit
    BuildingPlacer placer;
    GameObject buildingPrefabToSpawn;

    private void Awake()
    {
        cameraControl = this;
        selectionBox = GetComponentInChildren<Image>(true).transform as RectTransform;//dostęp do dziecka zmiennej typu Image  rzutowanej jako Recttransform
        camera = GetComponent<Camera>();//Dostęp do kompnentu typu Camera
        selectionBox.gameObject.SetActive(false);//ustawianie obiektu na nieaktywny
    }

    private void Start()
    {
        placer = GameObject.FindObjectOfType<BuildingPlacer>();
        placer.gameObject.SetActive(false);
    }
    private void Update()
    {
        UpdateMovement();//wywołanie funkcji
        UpdateZoom();//wywołanie funkcji
        UpdateClicks();//wywołanie funkcji
        UpdatePlacer();
    }


    void UpdateMovement()
    {
        keyboardInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));//dostęp sterowania klawiaturą w dwóch płaszczyznach
        mousePos = Input.mousePosition;//dostęp do pozycji kursora
        mousePosScreen = camera.ScreenToViewportPoint(mousePos);//pozycja na ekranie w % 0-1
        isCursorInGameScreen = mousePosScreen.x >= 0 && mousePosScreen.x <= 1 &&
            mousePosScreen.y >= 0 && mousePosScreen.y <= 1;//"widełki" kursora na ekranie

        Vector2 movementDirection = keyboardInput;//przypisanie wartosci keyboardInput do movementDirection

        if (isCursorInGameScreen)//warunek działania treshholda kursora na ekranie
        {
            if (mousePosScreen.x < cursorTreshhold) movementDirection.x -= 1 - mousePosScreen.x / cursorTreshhold;
            if (mousePosScreen.x > 1 - cursorTreshhold) movementDirection.x += 1 - (1 - mousePosScreen.x) / (cursorTreshhold);
            if (mousePosScreen.y < cursorTreshhold) movementDirection.y -= 1 - mousePosScreen.y / cursorTreshhold;
            if (mousePosScreen.y > 1 - cursorTreshhold) movementDirection.y += 1 - (1 - mousePosScreen.y) / (cursorTreshhold);
        }

        var deltaPosition = new Vector3(movementDirection.x, 0, movementDirection.y);//przypisanie wartosci wektora do zmiennej deltaPosition
        deltaPosition *= cameraSpeed * Time.deltaTime;//obliczanie deltaPosition
        transform.position += deltaPosition;//pozycja kamery powiekszona o deltaPosition

    }

    void UpdateZoom()
    {
        mouseScroll = Input.mouseScrollDelta;//przypisanie zmiany wartosci scrolla do zmiennej
        float zoomDelta = mouseScroll.y * zoomSpeed * Time.deltaTime;//obliczanie zoomDelta
        zoomLerp = Mathf.Clamp01(zoomLerp + zoomDelta);//zwracanie wyniku działania w zakresie od 0 do 1

        var position = transform.localPosition;//przypisanie lokalnej pozycji do zmienej
        position.y = Mathf.Lerp(cameraHeightMinMax.y, cameraHeightMinMax.x, zoomLerp) + groundHeight;//interpolacja liniowa zmiennej na y
        transform.localPosition = position;//przypisanie lokalnej pozycji zmiennej

        var rotation = transform.localEulerAngles;//przypisanie lokalnej rotacji w stopniach zmiennej rotation
        rotation.x = Mathf.Lerp(cameraRotationMinMax.y, cameraRotationMinMax.x, zoomLerp);//interpolacja liniowa zmiennej na x
        transform.localEulerAngles = rotation;//przypisanie lokalnej rotacji zmiennej

    }

    void UpdateClicks()
    {
        if (Input.GetMouseButtonDown(0))//lewy przycisk myszy
        {
            selectionBox.gameObject.SetActive(true);//ustawianie obiektu na aktywny
            selectionRect.position = mousePos;//przypisanie pozycji "ramki do zaznaczania" do zmiennej 
            TryBuild();
        }
        else if (Input.GetMouseButtonUp(0))//lewy przycisk myszy
        {
            selectionBox.gameObject.SetActive(false);//ustawianie obiektu na nieaktywny
        }
        if (Input.GetMouseButton(0))//lewy przycisk myszy
        {
            selectionRect.size = mousePos - selectionRect.position;//rozmiar "ramki do zaznaczania" obliczany jak wynik różnicy dwóch zmiennych
            boxRect = AbsRect(selectionRect);//wartość z funkcji AbsRect przypisana do zmiennej boxRect 
            selectionBox.anchoredPosition = boxRect.position;//przypisanie "pozycji kotwicy" selectionBox do pozycji boxRect
            selectionBox.sizeDelta = selectionRect.size;//przypisanie zmiany rozmieru selectionBox do rozmiaru selectionRect
            if (boxRect.size.x != 0 || boxRect.size.y != 0) 
            UpdateSelecting();//wywołanie funkcji
        }

        if (Input.GetMouseButtonDown(1))//prawy przycisk myszy
        {
            GiveCommands();//wywołanie funkcji
            buildingPrefabToSpawn = null;
        }


    }



    void UpdateSelecting()
    {
        selectedUnits.Clear();//wyczyszczenie listy selectedUtis
        foreach (ISelectable selectable in Unit.SelectableUnits)//petla "dla każdego unit typu Unit w liście SelectableUnits"
        {
            if (selectable == null) continue;//jeżeli nie jest unit LUB nie jest isAlive przejdź dalej
            MonoBehaviour monoBehaviour = selectable as MonoBehaviour;
            var pos = monoBehaviour.transform.position;//przypisnie pozycji unit do zmiennej
            var posScreen = camera.WorldToScreenPoint(pos);//zmiana world space do screen space przypisana do zmiennej
            bool inRect = IsPointInRect(boxRect, posScreen);//przypisanie wartosci funkcji do zmiennej typu bool
            (selectable as ISelectable).SetSelected(inRect);//rzutowanie unit jako interfejs ISelectable i wywołanie zmiennej inRect
            if (inRect)//jezeli inRect
            {
                selectedUnits.Add(selectable);//dodaj do listy selectedUnits unit
            }
        }
    }

    bool IsPointInRect(Rect rect, Vector2 point)//funkcja do obliczania IsPointInRect
    {
        return point.x >= rect.position.x && point.x <= (rect.position.x + rect.size.x) &&
            point.y >= rect.position.y && point.y <= (rect.position.y + rect.size.y);
    }

    Rect AbsRect(Rect rect)//
    {
        if (rect.width < 0)
        {
            rect.x += rect.width;
            rect.width *= -1;
        }
        if (rect.height < 0)
        {
            rect.y += rect.height;
            rect.height *= -1;
        }
        return rect;
    }

    Ray ray;
    RaycastHit rayHit;
    [SerializeField]
    LayerMask commandLayerMask = -1, buildingLayerMask=0;
    void GiveCommands()
    {
        ray = camera.ViewportPointToRay(mousePosScreen);//przypisanie promienia z kamery to punktu do zmiennej
        if (Physics.Raycast(ray, out rayHit, 1000, commandLayerMask))//jezeli raycast ma podane parametry to
        {
            object commandData = null;//
            if (rayHit.collider is TerrainCollider)//jezeli collider jest colliderem terenu
            {
                //Debug.Log("Terrain: " + rayHit.point.ToString());
                commandData = rayHit.point;//przypisanie punktu do obiektu

            }
            else
            {
                //   Debug.Log(rayHit.collider);
                commandData = rayHit.collider.gameObject.GetComponent<Unit>(); //przypisanie komponentu do obiektu

            }
            GiveCommands(commandData,"Command");//wywołanie funkcji
        }
    }

    void GiveCommands(object dataCommands, string commandName)
    {
        foreach (ISelectable selectable in selectedUnits)//dla wszystkich unit typu Unit w liscie selectedUnits
        {
            (selectable as MonoBehaviour).SendMessage(commandName, dataCommands, SendMessageOptions.DontRequireReceiver);
        }
    }

    public static void SpawnUnits(GameObject prefab)
    {
        cameraControl.GiveCommands(prefab,"Spawn");
    }

    public static void SpawnBuilding(GameObject prefab)
    {
        cameraControl.buildingPrefabToSpawn = prefab;
       
    }

    void UpdatePlacer()
    {
        placer.gameObject.SetActive(buildingPrefabToSpawn);
        if (placer.gameObject.activeInHierarchy)
        {
            ray = camera.ViewportPointToRay(mousePosScreen);//przypisanie promienia z kamery to punktu do zmiennej
            if (Physics.Raycast(ray, out rayHit, 1000, buildingLayerMask))//jezeli raycast ma podane parametry to
            {
                placer.SetPosition(rayHit.point);
            }
        }
    }
    void TryBuild()
    {
        if (buildingPrefabToSpawn &&placer && placer.isActiveAndEnabled && placer.CanBuildHere())
        {
            var buyable = buildingPrefabToSpawn.GetComponent<Buyable>();
            if (!buyable || !Money.TrySpendMoney(buyable.cost)) return;

            var building = Instantiate(buildingPrefabToSpawn, transform.position, transform.rotation);
            MoneyEarner.ShowMoneyText(building.transform.position, -(int)buyable.cost);
        }
    }

}

