using System;
using System.Collections.Generic;
using DefaultNamespace;
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

            List<Vector3> pointsToPaint = new List<Vector3>();
            
            Handles.color = Color.blue;
            for (int i = 1; i < path.Count - 1; i++) {
                PathPoint pathPoint = path.GetPoint(i);
                pathPoint.radius = 0.5f;
                Vector3 forwardVector = path.GetNormalizedForwardVector(pathPoint).normalized;

                Vector3 leftDirection = new Vector3(-forwardVector.z, forwardVector.y, forwardVector.x);
                Vector3 rightDirection = new Vector3(forwardVector.z, forwardVector.y, -forwardVector.x);

                Vector3 pathDirection = (path.GetPoint(i + 1).position - path.GetPoint(i).position).normalized;
                
                float leftDot = Vector3.Dot(leftDirection, pathDirection);
                float rightDot = Vector3.Dot(rightDirection, pathDirection);
                Vector3 direction = leftDot > rightDot ? leftDirection : rightDirection;

                Handles.color = Color.cyan;
                // for (int k = 0; k < 12; k++) {
                //     float angle = Mathf.Lerp(0, 360, (float) k / 12);
                //     float xPosition = Mathf.Cos(Mathf.Deg2Rad * angle);
                //     float yPosition = Mathf.Sin(Mathf.Deg2Rad * angle);
                //     Vector3 position = new Vector3(xPosition, 0, yPosition) * pathPoint.radius + circlePosition;
                //     Handles.FreeMoveHandle(-1, position, Quaternion.identity, 0.1f, Vector2.zero,
                //         Handles.SphereHandleCap);
                // }

                Handles.color = Color.blue;
                



                float endPointAngle = Vector3.Angle(path.GetPoint(i - 1).position - path.GetPoint(i).position, 
                    path.GetPoint(i + 1).position - path.GetPoint(i).position);
                
                float totalAngleDelta = 180 - endPointAngle;
                float midPointAngle = Vector3.SignedAngle(new Vector3(-direction.x, direction.y, -direction.z),
                    Vector3.right, Vector3.up);
                float startAngle = midPointAngle - totalAngleDelta / 2;
                float endAngle = midPointAngle + totalAngleDelta / 2;
                
                // float kiteDiagonal = Mathf.Tan(Mathf.Deg2Rad * endPointAngle / 2) * Mathf.Sin(Mathf.Deg2Rad * totalAngleDelta / 2) *
                //                pathPoint.radius + Mathf.Cos(Mathf.Deg2Rad * totalAngleDelta / 2) * pathPoint.radius;
                float kiteDiagonal = pathPoint.radius / Mathf.Sin(Mathf.Deg2Rad * endPointAngle / 2);
                
                Vector3 circlePosition = pathPoint.position + direction * kiteDiagonal;

                Handles.color = Color.cyan;
                Handles.FreeMoveHandle(circlePosition, Quaternion.identity, 0.1f, Vector2.zero, Handles.SphereHandleCap);
                Handles.color = Color.blue;


                for (int j = 0; j <= 6; j++) {
                    float angle = Mathf.Lerp(startAngle, endAngle, (float) j / 6);
                    float xPosition = Mathf.Cos(Mathf.Deg2Rad * angle);
                    float yPosition = Mathf.Sin(Mathf.Deg2Rad * angle);
                    Vector3 trigDirection = new Vector3(xPosition, 0, yPosition);
                    Vector3 pointPosition = circlePosition + trigDirection * pathPoint.radius;
                    pointsToPaint.Add(pointPosition);
                }
                
            }
            
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
            Handles.color = Color.blue;
            foreach (Vector3 position in pointsToPaint) {
                Handles.FreeMoveHandle(position, Quaternion.identity, 0.1f, Vector2.zero, Handles.SphereHandleCap);
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
