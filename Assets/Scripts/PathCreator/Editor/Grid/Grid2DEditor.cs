using UnityEditor;
using UnityEngine;

namespace PathCreator.Editor.Grid {
    [CustomEditor(typeof(Grid2D))]
    public class Grid2DEditor : UnityEditor.Editor {

        private Grid2D grid;

        private SerializedProperty horizontalSteps;
        private SerializedProperty verticalSteps;
        private SerializedProperty cellSize;
        private SerializedProperty display;


        public Camera camera;
        private Vector3 centerPoint;

        public bool displayGrid;

        private void OnSceneGUI() {
            // if (Event.current.type == EventType.Repaint && displayGrid) {
            //     grid.FillGrid();
            //     DrawGrid(grid.GetGrid());
            // }
        }

        [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected)]
        private static void DrawGridGizmo(Grid2D grid, GizmoType gizmoType) {
            DrawGrid(grid);
            
        }

        private static void DrawGrid(Grid2D grid) {
            if (!grid.display || grid.GetGrid() == null) return;
            RecursivelyDrawVerticalGridLines(0, 0, grid);
            RecursivelyDrawHorizontalLines(0, 0, grid);
        }

        private static void RecursivelyDrawHorizontalLines(int i, int j, Grid2D grid) {
            if (i >= grid.GetGrid().GetLength(0) - 1) {
                j++;
                i = 0;
            }

            if (j >= grid.GetGrid().GetLength(1)) return;
            Vector3 startPoint = grid.ConvertGridPointToWorldPoint(i, j);
            Vector3 endPoint = grid.ConvertGridPointToWorldPoint(++i, j);
            Gizmos.DrawLine(startPoint, endPoint);
            RecursivelyDrawHorizontalLines(i, j, grid);
        }


        private static void RecursivelyDrawVerticalGridLines(int i, int j, Grid2D grid) {
            if (j >= grid.GetGrid().GetLength(1) - 1) {
                i++;
                j = 0;
            }

            if (i >= grid.GetGrid().GetLength(0)) return;
            Vector3 startPoint = grid.ConvertGridPointToWorldPoint(i, j);
            Vector3 endPoint = grid.ConvertGridPointToWorldPoint(i, ++j);
            Gizmos.DrawLine(startPoint, endPoint);
            RecursivelyDrawVerticalGridLines(i, j, grid);
        }

        private void OnEnable() {
            camera = Camera.main;
            grid = (Grid2D) target;

            horizontalSteps = serializedObject.FindProperty("horizontalSteps");
            verticalSteps = serializedObject.FindProperty("verticalSteps");
            cellSize = serializedObject.FindProperty("cellSize");
            display = serializedObject.FindProperty("display");

        }

        private void CalculateCenterPoint() {
            Vector3 cameraPos = camera.transform.position;
            Vector3 vectorSlope = camera.transform.forward;
            float slopeMultiplier = cameraPos.y / vectorSlope.y;
            float xPos = cameraPos.x - slopeMultiplier * vectorSlope.x;
            float zPos = cameraPos.z - slopeMultiplier * vectorSlope.z;
            centerPoint = new Vector3(xPos, 0, zPos);
        }

        public override void OnInspectorGUI() {
            EditorGUI.BeginChangeCheck();
            Rect horizontalStepsPropertySpace =
                GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(20));
            int horizontalValue = EditorGUI.IntSlider(horizontalStepsPropertySpace, new GUIContent("Horizontal Steps"),
                horizontalSteps.intValue, 2, 100);

            Rect verticalStepsPropertySpace =
                GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(20));
            int verticalValue = EditorGUI.IntSlider(verticalStepsPropertySpace, new GUIContent("Vertical Steps"),
                verticalSteps.intValue, 2, 100);

            Rect cellSizePropertySpace = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(20));
            float cellSizeValue = EditorGUI.Slider(cellSizePropertySpace, new GUIContent("CellSize"),
                cellSize.floatValue, 0.01f, 5f);

            if (EditorGUI.EndChangeCheck()) {
                grid.MarkAsChanged();
                horizontalSteps.intValue = horizontalValue;
                verticalSteps.intValue = verticalValue;
                cellSize.floatValue = cellSizeValue;
                serializedObject.ApplyModifiedProperties();
                grid.FillGrid();
            }

            Rect displayProperty = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(20));
            EditorGUI.PropertyField(displayProperty, display, new GUIContent("Show Grid "));

            serializedObject.ApplyModifiedProperties();
        }

    }
}
