using System;
using System.Collections.Generic;
using PathCreator.Editor.MainEditor.Data;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace PathCreator.Editor.MainEditor.Tools.Move {
    [EditorTool("Move Tool", typeof(Path))]
    public class MoveTool : EditorTool {

        public static bool IsActive;
        
        private PathPoint _selectedPoint;
        private Dictionary<PathPoint, int> _controlIds;

        public static event Action PointMoved;
        public static event Action StartPointMoved;

        private Action<PathEditorState.SnapType> _updateSnapType;

        [SerializeField] private Texture2D iconTexture;
    
        public override GUIContent toolbarIcon {
            get {
                GUIContent display = new GUIContent() {
                    image = iconTexture
                };
                return display;
            }
        }

        private MoveToolOverlay Overlay => MoveToolOverlay.Instance;

        [Shortcut("Move Tool", typeof(SceneView))]
        static void MoveToolShortcut() {
            Path[] filtered = Selection.GetFiltered<Path>(SelectionMode.TopLevel);
            if (filtered.Length > 0) {
                ToolManager.SetActiveTool<MoveTool>();
            }
        }

        private void OnEnable() {
            MoveToolOverlay.PositionChanged += pos => {
                if (_selectedPoint == null) return;
                _selectedPoint.position = pos;
            };
        }

        private void OnDisable() {
            
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
                    foreach (PathPoint point in PathEditorState.Instance.Path.Points) {
                        _controlIds.Add(point, GUIUtility.GetControlID(FocusType.Passive));
                    }

                    break;
                case EventType.MouseDown:
                    SelectPoint();
                    break;
                case EventType.Repaint:
                    RepaintHandles();
                    break;
                case EventType.MouseDrag:
                    MovePoint();
                    break;
            }

        }

        private void MovePoint() {
            if (_selectedPoint == null || Event.current.button != 0) return;
            Path path = PathEditorState.Instance.Path;
            Grid2D grid = PathEditorState.Instance.Grid;

            
            Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            float slopeMultiplier = (worldRay.origin.y - path.transform.position.y) / worldRay.direction.y;
            float x = worldRay.origin.x - slopeMultiplier * worldRay.direction.x;
            float z = worldRay.origin.z - slopeMultiplier * worldRay.direction.z;
            Vector3 location = new Vector3(x, path.transform.position.y, z);
            if (PathEditorState.Instance.snapType == PathEditorState.SnapType.Snap) {
                location = grid.GetClosestPointOnGrid(location);
            }
            PointMoved?.Invoke();
            Undo.RecordObject(path, "Move point");
            _selectedPoint.position = location;
            if (_selectedPoint.index == 0) {
                StartPointMoved?.Invoke();
                path.transform.position = _selectedPoint.position;
            }
            if (Overlay == null) throw new Exception("Move tool overlay was null, Declan.... you fucked up");
            Overlay.UpdateValues(_selectedPoint.position);
            HandleUtility.Repaint();
        }

        private void SelectPoint() {
            if (Event.current.button != 0) return;
            float radius = 0.1f;
            Path path = PathEditorState.Instance.Path;
            PathPoint closetPoint = path.GetPoint(0);
            float distance = HandleUtility.DistanceToCircle(closetPoint.position, radius);
            foreach (PathPoint point in path.Points) {
                float distToHandle = HandleUtility.DistanceToCircle(point.position, radius);
                if (distToHandle < distance) {
                    closetPoint = point;
                    distance = distToHandle;
                }
            }

            _selectedPoint = distance <= radius ? closetPoint : null;
            if(_selectedPoint != null) Overlay.UpdateValues(_selectedPoint.position);
            Event.current.Use();
        }

        private void RepaintHandles() {

            Path path = PathEditorState.Instance.Path;

            Handles.color = Color.red;
            for (int i = 0; i < path.Count - 1; i++) {
                Handles.DrawLine(path.GetPoint(i).position, path.GetPoint(i + 1).position, 2f);
            }

            Handles.color = Color.white;
            foreach (PathPoint point in path.Points) {
                int handleId = _controlIds[point];
                if (point == _selectedPoint) {
                    using (new Handles.DrawingScope(Color.yellow)) {
                        Handles.FreeMoveHandle(handleId, point.position, Quaternion.identity, 0.1f, Vector2.zero,
                            Handles.SphereHandleCap);
                    }
                }
                else {
                    Handles.FreeMoveHandle(handleId, point.position, Quaternion.identity, 0.1f, Vector2.zero,
                        Handles.SphereHandleCap);

                }
            }
        }

        public override void OnActivated() {
            IsActive = true;
        }

        public override void OnWillBeDeactivated() {
            IsActive = false;
        }


    }
}
