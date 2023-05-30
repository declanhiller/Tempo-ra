using System;
using DefaultNamespace;
using PathCreator.Editor.MainEditor.Data;
using PathCreator.Editor.MainEditor.Tools.Add;
using PathCreator.Editor.MainEditor.Tools.Delete;
using PathCreator.Editor.MainEditor.Tools.Move;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace PathCreator.Editor.MainEditor {
    [CustomEditor(typeof(Path))]
    public class PathMainEditor : UnityEditor.Editor {

        private bool _wasStartPointChanged;
        
        [SerializeField] private VisualTreeAsset inspectorGUI;

        private Action<PathEditorState.MoveType> _updateMoveType;
        private Action<PathEditorState.SnapType> _updateSnapType;
        

        [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected)]
        private static void DrawGridGizmo(Path path, GizmoType gizmoType) {
            if (AddTool.IsActive || MoveTool.IsActive || DeleteTool.IsActive) return;
            DrawGizmoPath(path);
        }

        private Vector3 _startTransformPosition;
        private void OnSceneGUI() {

            if (!_wasStartPointChanged) MovePointsWithTransform();

            _startTransformPosition = PathEditorState.Instance.Path.transform.position;
            _wasStartPointChanged = false;
        }

        public override VisualElement CreateInspectorGUI() {
            VisualElement root = new VisualElement();
            inspectorGUI.CloneTree(root);

            //Register move type enum
            EnumField moveTypeField = root.Query<EnumField>("move_type").First();
            moveTypeField.Init(PathEditorState.Instance.moveType);
            moveTypeField.RegisterValueChangedCallback(evt => {
                PathEditorState.Instance.moveType = (PathEditorState.MoveType) evt.newValue; 
            });
            _updateMoveType = type => moveTypeField.SetValueWithoutNotify(type);
            PathEditorState.MoveTypeChanged += _updateMoveType;
            
            //Register snap type enum
            EnumField snapTypeField = root.Query<EnumField>("snap_type").First();
            snapTypeField.Init(PathEditorState.Instance.snapType);
            snapTypeField.RegisterValueChangedCallback(evt => {
                PathEditorState.Instance.snapType = (PathEditorState.SnapType) evt.newValue;
            });
            _updateSnapType = type => snapTypeField.SetValueWithoutNotify(type);
            PathEditorState.SnapTypeChanged += _updateSnapType;
            

            return root;
        }

        private void OnDestroy() {
            PathEditorState.MoveTypeChanged -= _updateMoveType;
            PathEditorState.SnapTypeChanged -= _updateSnapType;
        }

        private void MovePointsWithTransform() {
            Path path = PathEditorState.Instance.Path;
            Grid2D grid = PathEditorState.Instance.Grid;
            if (path.transform.hasChanged) {
                Vector3 closestPointOnGrid = grid.GetClosestPointOnGrid(path.transform.position);
                path.transform.position = closestPointOnGrid;
                Vector3 delta = closestPointOnGrid - _startTransformPosition;
                foreach (PathPoint pathPoint in path.Points) {
                    pathPoint.position += delta;
                }

                path.transform.hasChanged = false;
            }
        }

        private void OnEnable() {
            // _path = (Path) target;
            // _grid = FindObjectOfType<Grid2D>();
            PathEditorState.Instance.Path = (Path) target;
            PathEditorState.Instance.Grid = FindObjectOfType<Grid2D>();
            MoveTool.PointMoved += MarkAsChanged;
            AddTool.PointAdded += MarkAsChanged;
            DeleteTool.PointDeleted += MarkAsChanged;
            MoveTool.StartPointMoved += StartPointChanged;
        }

        private void OnDisable() {
            MoveTool.PointMoved -= MarkAsChanged;
            AddTool.PointAdded -= MarkAsChanged;
            DeleteTool.PointDeleted -= MarkAsChanged;
            MoveTool.StartPointMoved -= StartPointChanged;
        }

        private void StartPointChanged() {
            _wasStartPointChanged = true;
        }

        private void MarkAsChanged() {
            PathEditorState.Instance.Path.MarkAsChanged();
        }

        private static void DrawGizmoPath(Path path) {
            for (int i = 0; i < path.Count - 1; i++) {
                Vector3 startPoint = path.GetPoint(i).position;
                Vector3 endPoint = path.GetPoint(i + 1).position;

                Gizmos.color = Color.red;
                
                GizmosUtility.DrawLineWithThickness(startPoint, endPoint, 2f);
                
            }
        }
        
    }
}
