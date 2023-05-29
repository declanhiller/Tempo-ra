using System;
using System.Collections.Generic;
using DefaultNamespace;
using PathCreator.Editor.MainEditor.Tools.Add;
using PathCreator.Editor.MainEditor.Tools.Delete;
using PathCreator.Editor.MainEditor.Tools.Move;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.UIElements;

namespace PathCreator.Editor.MainEditor {
    [CustomEditor(typeof(Path))]
    public class PathMainEditor : UnityEditor.Editor {

        private Path _path;
        private Grid2D _grid;

        private bool _wasStartPointChanged;

        [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected)]
        private static void DrawGridGizmo(Path path, GizmoType gizmoType) {
            if (AddTool.IsActive || MoveTool.IsActive || DeleteTool.IsActive) return;
            DrawGizmoPath(path);
        }

        private Vector3 _startTransformPosition;
        private void OnSceneGUI() {

            if (!_wasStartPointChanged) MovePointsWithTransform();

            _startTransformPosition = _path.transform.position;
            _wasStartPointChanged = false;
        }

        private void MovePointsWithTransform() {
            if (_path.transform.hasChanged) {
                Vector3 closestPointOnGrid = _grid.GetClosestPointOnGrid(_path.transform.position);
                _path.transform.position = closestPointOnGrid;
                Vector3 delta = closestPointOnGrid - _startTransformPosition;
                foreach (PathPoint pathPoint in _path.Points) {
                    pathPoint.position += delta;
                }

                _path.transform.hasChanged = false;
            }
        }

        private void OnEnable() {
            _path = (Path) target;
            _grid = FindObjectOfType<Grid2D>();
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
            _path.MarkAsChanged();
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
