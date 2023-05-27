using System;
using System.Collections.Generic;
using PathCreator.Editor.MainEditor.Data;
using PathCreator.Editor.MainEditor.Tools.Move;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace PathCreator.Editor.MainEditor.Tools.Add {
    [EditorTool("Add Point Tool", typeof(Path))]
    public class AddTool : EditorTool {

        private PathData _pathData;
        private Dictionary<PathPoint, int> _controlIds;

        private Vector3 _proposedPointPosition;
        private int _indexOfProposedPoint;
        private int _proposedPointControlId;

        private bool _isInSnapMode;
        
        public static bool IsActive { get; private set; }

        public static event Action PointAdded;
    
    
        [SerializeField] private Texture2D iconTexture;
    
        public override GUIContent toolbarIcon {
            get {
                GUIContent display = new GUIContent() {
                    image = iconTexture
                };
                return display;
            }
        }
    

        [Shortcut("Add Point Tool", typeof(SceneView))]
        static void AddToolShortcut() {
            Path[] filtered = Selection.GetFiltered<Path>(SelectionMode.TopLevel);
            if (filtered.Length > 0) {
                ToolManager.SetActiveTool<MoveTool>();
            }
        }

        private void OnEnable() {
            _pathData = new PathData((Path) target, FindObjectOfType<Grid2D>());
            AddToolOverlay.SnapModeSwitched += isSnapMode => _isInSnapMode = isSnapMode;
        }

        private void OnDisable() {
            _pathData = null;
        }

        public override void OnToolGUI(EditorWindow window) {
            if (!(window is SceneView)) return;
        
            Event e = Event.current;

            switch (e.type) {
                case EventType.Layout:
                    using (var check = new EditorGUI.ChangeCheckScope()) {

                        HandleUtility.AddDefaultControl(0);

                        if (check.changed) {
                            EditorApplication.QueuePlayerLoopUpdate();
                        }
                    }

                    _controlIds = new Dictionary<PathPoint, int>();
                    foreach (PathPoint point in _pathData.path.Points) {
                        _controlIds.Add(point, GUIUtility.GetControlID(FocusType.Passive));
                    }
                    _proposedPointControlId = GUIUtility.GetControlID(FocusType.Passive);
                    break;
                case EventType.MouseMove:
                    GetNewPosition();
                    break;
                case EventType.MouseDown:
                    CreateNewPointAtPosition();
                    break;
                case EventType.Repaint:
                    RepaintHandles();
                    break;
            }
        
        }
    
        private void CreateNewPointAtPosition() {
            if (Event.current.button != 0) return;
            Undo.RecordObject(_pathData.path, "Add new point");
            _pathData.path.Add(_indexOfProposedPoint, _proposedPointPosition);
            PointAdded?.Invoke();
            Event.current.Use();
        }
    
        private void RepaintHandles() {
            Path path = _pathData.path;

            Handles.color = Color.red;
            if (_indexOfProposedPoint == 0) {
                Handles.DrawLine(_proposedPointPosition, path.GetPoint(0).position);
            }
        
            for (int i = 1; i < path.Count; i++) {
                int startIndex = i - 1;
                int endIndex = i;

                if (endIndex == _indexOfProposedPoint) {
                    Handles.DrawLine(path.GetPoint(startIndex).position, _proposedPointPosition, 2f);
                    Handles.DrawLine(_proposedPointPosition, path.GetPoint(endIndex).position, 2f);
                } else {
                    Handles.DrawLine(path.GetPoint(startIndex).position, path.GetPoint(endIndex).position, 2f);
                }
            }

            if (_indexOfProposedPoint == path.Count) {
                Handles.DrawLine(path.GetPoint(path.Count - 1).position, _proposedPointPosition);
            }
        
            Handles.color = Color.yellow;
            Handles.FreeMoveHandle(_proposedPointControlId, _proposedPointPosition, Quaternion.identity, 0.1f, Vector2.zero,
                Handles.SphereHandleCap);
        
            Handles.color = Color.white;
            foreach (PathPoint point in path.Points) {
                Handles.FreeMoveHandle(_controlIds[point], point.position, Quaternion.identity, 0.1f, Vector2.zero, Handles.SphereHandleCap);
            }
        
        }
    
        private void GetNewPosition() {

            Event evt = Event.current;
            Path path = _pathData.path;
            Grid2D grid = _pathData.grid;

            //Find point world position
            Ray worldRay = HandleUtility.GUIPointToWorldRay(evt.mousePosition);
            float slopeMultiplier = (worldRay.origin.y - path.transform.position.y) / worldRay.direction.y;
            float x = worldRay.origin.x - slopeMultiplier * worldRay.direction.x;
            float z = worldRay.origin.z - slopeMultiplier * worldRay.direction.z;
            Vector3 mousePosition = new Vector3(x, path.transform.position.y, z);
            if (_isInSnapMode) {
                mousePosition = grid.GetClosestPointOnGrid(mousePosition);
            }
            _proposedPointPosition = mousePosition;
        
            //Find point array index
            int indexOfNearestPoint = 0;
            float distance = Vector3.Distance(mousePosition, path.GetPoint(0).position);
        
            for (int i = 1; i < path.Count; i++) {
                Vector3 currPoint = path.GetPoint(i).position;
                float tempDistance = Vector3.Distance(mousePosition, currPoint);
                if (tempDistance < distance) {
                    distance = tempDistance;
                    indexOfNearestPoint = i;
                }
            }

            float dot = Vector3.Dot(path.GetNormalizedForwardVector(path.GetPoint(indexOfNearestPoint)), mousePosition - path.GetPoint(indexOfNearestPoint).position);
            _indexOfProposedPoint = dot > 0 ? indexOfNearestPoint + 1 : indexOfNearestPoint;
        
            HandleUtility.Repaint();
        }

        public override void OnActivated() {
            IsActive = true;
        }

        public override void OnWillBeDeactivated() {
            IsActive = false;
        }

    }
}

