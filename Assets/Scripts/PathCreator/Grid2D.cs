using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace PathCreator {
    public class Grid2D : MonoBehaviour, ISerializationCallbackReceiver {
        [SerializeField] private int horizontalSteps;
        [SerializeField] private int verticalSteps;
        [SerializeField] private float cellSize;

        private Vector2[,] grid;
        [SerializeField] private Vector2[] gridContainer;
        
        
        public bool display = false;

        public void FillGrid() {
            Vector2 centerPoint = new Vector2(transform.position.x, transform.position.z);
            grid = new Vector2[verticalSteps, horizontalSteps];

            float x = centerPoint.x - (verticalSteps / 2) * cellSize;
            float begY = centerPoint.y - (horizontalSteps / 2) * cellSize;
            float y = begY;
            for (int i = 0; i < verticalSteps; i++) {
                for (int j = 0; j < horizontalSteps; j++) {
                    grid[i, j] = new Vector2(x, y);
                    y += cellSize;
                }

                y = begY;
                x += cellSize;
            }

            changed = false;
            FillContainerFromGrid();
        }


        public Vector3 ConvertGridPointToWorldPoint(Vector2Int gridPosition) {
            Vector2 a = grid[gridPosition.x, gridPosition.y];
            return new Vector3(a.x, transform.position.y, a.y);
        }

        public Vector3 ConvertGridPointToWorldPoint(int x, int y) {
            Vector2 a;
            try {
                a = grid[x, y];
            }
            catch (IndexOutOfRangeException e) {
                Debug.Log(x + ", " + y + " :: " + verticalSteps + ", " + horizontalSteps + " :: " + 
                          grid.GetLength(0) + ", " + grid.GetLength(1));
                throw e;
            }
            return new Vector3(a.x, transform.position.y, a.y);
        }

        public Vector3 GetClosestPointOnGrid(Vector3 position) {

            Vector2 centerPoint = new Vector2(transform.position.x, transform.position.z);


            int numberOfXSteps = Mathf.RoundToInt((position.x - centerPoint.x)/cellSize);
            int numberOfYSteps = Mathf.RoundToInt((position.z - centerPoint.y)/cellSize);

            Vector3 closetGridPoint = new Vector3(numberOfXSteps * cellSize +  centerPoint.x, 
                position.y, numberOfYSteps * cellSize + centerPoint.y);

            return closetGridPoint;
        }

        public Vector2[,] GetGrid() {
            return grid;
        }

        public void OnBeforeSerialize() {
            if (changed) return;
            FillContainerFromGrid();
        }

        public void OnAfterDeserialize() {
            if (changed) return;
            FillGridFromContainer();
        }

        public void FillGridFromContainer() {
            grid = new Vector2[verticalSteps, horizontalSteps];
            int i2D = 0;
            int j2D = 0;
            for (int i = 0; i < gridContainer.Length; i++) {
                grid[i2D, j2D] = gridContainer[i];
                i2D++;
                if (i2D >= verticalSteps) {
                    i2D = 0;
                    j2D++;
                }
            }
        }

        public void FillContainerFromGrid() {
            gridContainer = new Vector2[horizontalSteps * verticalSteps];
            int i2D = 0;
            int j2D = 0;
            for (int i = 0; i < gridContainer.Length; i++) {
                gridContainer[i] = grid[i2D, j2D];
                i2D++;
                if (i2D >= verticalSteps) {
                    i2D = 0;
                    j2D++;
                }
            }
        }

        private bool changed;
        public void MarkAsChanged() {
            changed = true;
        }
    }
}
