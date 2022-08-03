using UnityEngine;
using UnityEngine.Tilemaps;

public class SelectionManager : MonoBehaviour
{
    [SerializeField]
    private Tilemap gameTilemap;
    UnitSelection _input;
    Camera _camera;

    BaseGoblin currentEntity;
    BaseGoblin prevEntity;

    private void Awake()
    {
        _input = new UnitSelection();
        _camera = Camera.main;
    }

    private void OnEnable() 
    {
        _input.Enable();
    }

    private void OnDisable() 
    {
        _input.Disable();
    }

    void Start()
    {
        _input.Input.Select.performed += _ => SelectUnit();
    }

    void SelectUnit()
    {
        prevEntity = currentEntity;
        Vector2 mousePos = _input.Input.Pos.ReadValue<Vector2>();
        mousePos = Camera.main.ScreenToWorldPoint(mousePos);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if(hit.collider == null) return;
        currentEntity = hit.collider.gameObject.GetComponent<BaseGoblin>();
        if(prevEntity != null && prevEntity != currentEntity) prevEntity.actionManager.Deselect();
        if(!(currentEntity is BaseGoblin)) return;
        currentEntity.OnClick();
    }
}
