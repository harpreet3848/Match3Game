using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputController : MonoBehaviour
{
    [SerializeField] Match _match;

    [SerializeField] PlayerInput _playerInput;  

    InputAction _clickAction;
    InputAction _pointAction;
    Vector2 _mousePosition => _pointAction.ReadValue<Vector2>();

    private Vector2 _mouseWorldPosition;
    private Vector2Int _selectedGridIndex;
    private bool _hasSwapped = false;
    private Camera _cam;
    private void Awake()
    {
        _cam = Camera.main;

        _pointAction = _playerInput.actions["Select"];
        _clickAction = _playerInput.actions["Click"];

        _clickAction.started += InitilizeSwapTilePositions;
        _clickAction.canceled += OnClickCanceled;

        _pointAction.performed += OnPointMoved;
    }
    private void OnEnable()
    {
        _pointAction.Enable();
        _clickAction.Enable();
    }
    private void OnDisable()
    {
        _pointAction.Disable();
        _clickAction.Disable();
    }

    private void InitilizeSwapTilePositions(InputAction.CallbackContext ctx)
    {
        if (GameManager.instance.gameState == GameState.Animating ||_hasSwapped) return;

        _mouseWorldPosition = _cam.ScreenToWorldPoint(_mousePosition);

        Vector2 gridPos = _match.Grid.WorldToGridIndex(_mouseWorldPosition.x, _mouseWorldPosition.y);

        _selectedGridIndex = new Vector2Int((int)gridPos.x,
                                           (int)gridPos.y);
    }

    private void OnPointMoved(InputAction.CallbackContext ctx)
    {
        if (_clickAction.phase == InputActionPhase.Performed)
            CheckSwapPosition();
    }
    private void CheckSwapPosition()
    {
        if (GameManager.instance.gameState == GameState.Animating ||
            _hasSwapped ||
            !IsValidPosition()) return;

        Vector3 currentMousePosition = _cam.ScreenToWorldPoint(_mousePosition);

        if (Vector2.Distance(_mouseWorldPosition, currentMousePosition) > 0.7f)
        {
            Tile tile = _match.Grid.GetGridObjectAtIndex(_selectedGridIndex.y, _selectedGridIndex.x);

            if (tile == null) return;

            Vector3 relative = tile.transform.InverseTransformPoint(currentMousePosition);
            float angle = Mathf.Atan2(relative.y, relative.x) * Mathf.Rad2Deg;

            _hasSwapped = true;

            // Now check four quadrants (each covers a 90° slice):
            if (angle >= -45f && angle < 45f)
            {
                //Debug.Log("Direction = Right");
                _match.SwapTiles(_selectedGridIndex, new Vector2Int(Mathf.Min(_match.Grid.MaxColumn - 1, _selectedGridIndex.x + 1), _selectedGridIndex.y));

            }
            else if (angle >= 45f && angle < 135f)
            {
                _match.SwapTiles(_selectedGridIndex, new Vector2Int(_selectedGridIndex.x, Mathf.Max(0, _selectedGridIndex.y - 1)));
                //Debug.Log("Direction = Top");
            }
            else if (angle >= -135f && angle < -45f)
            {
                _match.SwapTiles(_selectedGridIndex, new Vector2Int(_selectedGridIndex.x, Mathf.Min(_match.Grid.MaxRow - 1, _selectedGridIndex.y + 1)));
                //Debug.Log("Direction = Bottom");
            }
            else
            {
                _match.SwapTiles(_selectedGridIndex, new Vector2Int(Mathf.Max(0, _selectedGridIndex.x - 1), _selectedGridIndex.y));
                //Debug.Log("Direction = Left");
            }
        }
    }
    private bool IsValidPosition()
    {
        return _selectedGridIndex.y >= 0 && _selectedGridIndex.y < _match.Grid.MaxRow &&
                        _selectedGridIndex.x >= 0 && _selectedGridIndex.x < _match.Grid.MaxColumn;
    }
    private void OnClickCanceled(InputAction.CallbackContext ctx)
    {
        _hasSwapped = false;
    }
}
