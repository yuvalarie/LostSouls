using System.Collections.Generic;
using System.Linq;
using Core.Input_System;
using Core.Managers;
using Game_Flow.ImpactObjects.Scripts.UnityMonoSOScripts;
using UnityEngine;
using UnityEngine.InputSystem;
using Grid = Game_Flow.ImpactObjects.Scripts.UnityMonoSOScripts.Grid;

namespace Game_Flow.DotVisual.Scripts.States
{
    public class TopDownState : IObjeckLockingState
    {
        private const float Speed = 2f;
        private Vector3 _position;
        // private Renderer _dotRenderer;
        private InputSystem_Actions _inputActions;
        private Vector2 _input;
        private MonoImpactObject _target;
        private MonoImpactObject _lastTarget;
        private InputAction _inputReader;
        private Grid grid;
        
        private float _moveCooldown = 0.3f;
        private float _lastMoveTime = -Mathf.Infinity;
        private LayerMask _impactLayerMask = LayerMask.GetMask("ImpactObject");
        
        private List<MonoImpactObject> _impactObjects;
        private ObjectController _cameraMono;
        private BoxCombinationHandler _boxCombintaionHandler;

        public Vector3 DebugInputDir   { get;  set; }
        public MonoImpactObject DebugNextTarget { get; set; }
        public void EnterState(Transform origin, GameObject dotInstance, List<MonoImpactObject> impactObjects,
            ObjectController objectController)
        {
            grid = objectController.Grid;
            EventManager.LockStateChanged(this);
            _inputActions = InputSystemBuffer.Instance.InputSystem;;
            _inputActions.Enable();
            _inputActions.Player.Enable();
            _inputReader = _inputActions.Player.Move;
            _position = origin.position;
            _inputReader.performed += OnMovePerformed;
            _inputReader.canceled += OnMoveCanceled;
            Debug.Log("Entered top state");
            _impactObjects = impactObjects;
            _target = impactObjects[0];
            _target.HighlightObject();
            _cameraMono = objectController;
            _lastTarget = _target;
            _boxCombintaionHandler = objectController.BoxCombinationHandler;
            
            var lightsToDisable = objectController.LightsToDisableInTopDownView;
            foreach(var light in lightsToDisable)
            {
                light.gameObject.SetActive(false);
            }
            var lightsToEnable = objectController.LightsToEnableInTopDownView;
            foreach(var light in lightsToEnable)
            {
                light.gameObject.SetActive(true);
            }
        }
        public void ExitState()
        {
            _inputReader.performed -= OnMovePerformed;
            _inputReader.canceled -= OnMoveCanceled;            
            _inputActions.Player.Disable();
            _inputActions.Disable();
        }

        public void Update()
        {
            if (_boxCombintaionHandler.GameFinished)
            {
                _target.UnhighlightObject();
                return;
            }
            if (!_cameraMono.IsLocked)
            {
                UnLockedUpdate();
            }
            
        }

        private void UnLockedUpdate()
        {
            if (_input == Vector2.zero || Time.time - _lastMoveTime < _moveCooldown || _target.IsMoving)
                return;
            // 0) get raw stick into 3D
            Vector3 raw3D = new Vector3(_input.x, 0f, _input.y);

            // 1) rotate by camera yaw
            float camYaw = _cameraMono.transform.eulerAngles.y;
            Quaternion yawRot = Quaternion.Euler(0f, camYaw, 0f);
            Vector3 camRelative = yawRot * raw3D;
            var cam2D = new Vector2(camRelative.x, camRelative.z);

            var inputDir = QuantizeTo4Directions(cam2D);
            DebugInputDir  = inputDir;

            if (inputDir == Vector3.zero)
                return;

            _lastMoveTime = Time.time;
            var newTarget  = FindNearestInDirection(inputDir);
            
            DebugNextTarget = newTarget;
            var oldTarget = _target;

            if (newTarget != null && newTarget != _target)
            {
                // we found a new one – unhighlight the old, highlight the new
                _lastTarget = oldTarget;
                oldTarget?.UnhighlightObject();
                oldTarget?.StopAudio();
                _target = newTarget;
                if (_target.IsMoveable)
                    _target.HighlightObject();
            }
        }
        
        
        
        private MonoImpactObject FindNearestInDirection(Vector3 dir)
        {
            if (_target == null || _target.UsedCells == null || _target.UsedCells.Count == 0)
                return _target;
            dir = dir.normalized;
            int rowDelta = 0, colDelta = 0;

            if (dir.x > 0.5f) colDelta = 1;         // RIGHT
            else if (dir.x < -0.5f) colDelta = -1;  // LEFT
            else if (dir.z > 0.5f) rowDelta = 1;    // UP
            else if (dir.z < -0.5f) rowDelta = -1;  // DOWN
            else
                return _target; // Invalid direction

            var edgeCells = GetEdgeCellsInDirection(_target.UsedCells, rowDelta, colDelta);

            // How far can we step outward from the edge?
            int maxSteps = Mathf.Max(grid.Rows, grid.Cols);

            for (int step = 1; step <= maxSteps; step++)
            {
                HashSet<MonoImpactObject> foundTargets = new();

                foreach (var (row, col) in edgeCells)
                {
                    int checkRow = row + step * rowDelta;
                    int checkCol = col + step * colDelta;

                    if (!grid.IsValidCell(checkRow, checkCol))
                        continue;

                    var occupant = grid.GetOccupant(checkRow, checkCol);
                    if (occupant != null && occupant != _target)
                        foundTargets.Add(occupant);
                }
                foundTargets.Remove(_target);
                
                if (foundTargets.Count > 0)
                {
                    var list = foundTargets.ToList();

                    // If there’s more than one target, exclude _lastTarget
                    if (list.Count > 1 && _lastTarget != null)
                        list.Remove(_lastTarget);

                    // Choose randomly from the remaining options
                    var newTarget = list[Random.Range(0, list.Count)];
                    return newTarget;
                }
            }

            return _target;
        }


        
        // Updated GetEdgeCellsInDirection with direction-specific tie-breakers
        private List<(int row, int col)> GetEdgeCellsInDirection(List<(int row, int col)> usedCells, int rowDelta, int colDelta)
        {
            var result = new List<(int row, int col)>();
    
            if (colDelta != 0) // LEFT or RIGHT
            {
                // Group by column and find the maximum group size
                var colGroups = usedCells.GroupBy(cell => cell.col);
                int maxCount = colGroups.Max(g => g.Count());
                var candidates = colGroups.Where(g => g.Count() == maxCount);

                // Tie-breaker: moving right -> choose leftmost column; moving left -> rightmost
                IGrouping<int, (int row, int col)> chosenGroup;
                if (colDelta > 0) // RIGHT
                    chosenGroup = candidates.OrderBy(g => g.Key).First();
                else // LEFT
                    chosenGroup = candidates.OrderByDescending(g => g.Key).First();

                result.AddRange(chosenGroup);
            }
            else if (rowDelta != 0) // UP or DOWN
            {
                // Group by row and find the maximum group size
                var rowGroups = usedCells.GroupBy(cell => cell.row);
                int maxCount = rowGroups.Max(g => g.Count());
                var candidates = rowGroups.Where(g => g.Count() == maxCount);

                // Tie-breaker: moving up -> choose bottommost row; moving down -> topmost
                IGrouping<int, (int row, int col)> chosenGroup;
                if (rowDelta > 0) // UP
                    chosenGroup = candidates.OrderBy(g => g.Key).First();
                else // DOWN
                    chosenGroup = candidates.OrderByDescending(g => g.Key).First();

                result.AddRange(chosenGroup);
            }

            return result;
        }




        
        private Vector3 QuantizeTo4Directions(Vector2 rawInput)
        {

            // 1) get 0–360° angle (where 0° = +X/East, 90° = +Y/North)
            float angle = Mathf.Atan2(rawInput.y, rawInput.x) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360f;

            // 2) snap to nearest multiple of 90°
            int sector = Mathf.RoundToInt(angle / 90f) % 4;
            float snappedAngle = sector * 90f * Mathf.Deg2Rad;

            // 3) rebuild a unit vector from that snapped angle
            Vector2 dir2D = new Vector2(Mathf.Cos(snappedAngle), Mathf.Sin(snappedAngle));
            return new Vector3(dir2D.x, 0f, dir2D.y);
        }


        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            _input = _inputReader.ReadValue<Vector2>();
        }
        
        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            _input = Vector2.zero;
        }
        
        public MonoImpactObject GetTarget(out MonoImpactObject target)
        {
            return target = _target;
        }

        public Vector3 CalculateMovement(Vector2 input)
        {
            return new Vector3(-input.x, 0, -input.y) * (Speed * Time.deltaTime);
        }
    }
}