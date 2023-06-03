using System;
using System.Collections.Generic;
using PathCreator.Editor.MainEditor.Data;
using PathCreator.Editor.MainEditor.Tools.Move;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace PathCreator.Editor.MainEditor.Tools.Delete {
    [EditorTool("Delete Point Tool", typeof(Path))]
    public class DeleteTool : EditorTool {

        public static bool IsActive;

        private PathPoint _hoveringOverPoint;
    
        private Dictionary<PathPoint, int> _controlIds;

        [SerializeField] private Texture2D iconTexture;

        public static event Action PointDeleted;
    
        public override GUIContent toolbarIcon {
            get {
                GUIContent display = new GUIContent() {
                    image = iconTexture
                };
                return display;
            }
        }
    
        [Shortcut("Delete Point Tool", typeof(SceneView))]
        static void DeletePointToolShortcut() {
            Path[] filtered = Selection.GetFiltered<Path>(SelectionMode.TopLevel);
            if (filtered.Length > 0) {
                ToolManager.SetActiveTool<MoveTool>();
            }
        }

        public override void OnToolGUI(EditorWindow window) {
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
                case EventType.Repaint:
                    RepaintHandles();
                    break;
                case EventType.MouseDown:
                    DeletePoint();
                    break;
                case EventType.MouseMove:
                    FindPoint();
                    break;
            }
        }

        private void RepaintHandles() {

            Path path = PathEditorState.Instance.Path;
        
            Handles.color = Color.red;
            for (int i = 0; i < path.Count - 1; i++) {
                Handles.DrawLine(path.GetPoint(i).position, path.GetPoint(i + 1).position, 2f);
            }
        
            Handles.color = Color.white;
            foreach (PathPoint pathPoint in path.Points) {
                int handleId = _controlIds[pathPoint];

                if (pathPoint.index == path.Count - 1) Handles.color = Color.red;
                if (pathPoint.index == 0) Handles.color = Color.green;
                
                if (pathPoint == _hoveringOverPoint) {
                    using (new Handles.DrawingScope(Color.yellow)) {
                        Handles.FreeMoveHandle(handleId, pathPoint.position, Quaternion.identity, 0.1f, Vector2.zero, Handles.SphereHandleCap);
                    }
                    continue;
                }
            
                Handles.FreeMoveHandle(handleId, pathPoint.position, Quaternion.identity, 0.1f, Vector2.zero, Handles.SphereHandleCap);
                Handles.color = Color.white;
            }
        }
    
        private void DeletePoint() {
            if (Event.current.button != 0) return;
            if (_hoveringOverPoint == null) return;
            Undo.RecordObject(PathEditorState.Instance.Path, "Delete point on path");
            PathEditorState.Instance.Path.Remove(_hoveringOverPoint);
            PointDeleted?.Invoke();
            Event.current.Use();
        }
    
        private void FindPoint() {
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

            _hoveringOverPoint = distance <= radius ? closetPoint : null;
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
