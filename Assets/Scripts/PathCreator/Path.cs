using System;
using System.Collections.Generic;
using UnityEngine;


namespace PathCreator {
    public class Path : MonoBehaviour {
        
        [SerializeField, HideInInspector] private List<PathPoint> points = new List<PathPoint>() {
            new PathPoint{index = 0, position = new Vector3(0, 0, 0)}, new PathPoint{index = 1, position = new Vector3(0, 0, 1)}
        };

        public event Action PathChanged;

        public int Count => points.Count;

        [SerializeField] private int resolution;

        public float Length {
            get {
                float totalDist = 0;
                for (int i = 0; i < Count - 1; i++) {
                    PathPoint startPoint = GetPoint(i);
                    PathPoint endPoint = GetPoint(i + 1);
                    totalDist += Vector3.Distance(startPoint.position, endPoint.position);
                }

                return totalDist;
            }
        }

        public IEnumerable<PathPoint> Points => points;


        public Vector3 Lerp(float t) {
            t = Mathf.Clamp(t, 0, 1);
            
            float targetDistance = t * Length;

            int index = -1;
            float ratioOfRemainingLine = 0;
            for (int i = 0; i < Count - 1; i++) {
                Vector3 startPoint = GetPoint(i).position;
                Vector3 endPoint = GetPoint(i + 1).position;
                float tempDistance = Vector3.Distance(startPoint, endPoint);
                
                if (targetDistance <= tempDistance) {
                    index = i;
                    ratioOfRemainingLine = targetDistance / tempDistance;
                    break;
                }
                targetDistance -= tempDistance;
            }

            if (index == -1) {
                return GetPoint(Count - 1).position;
            };

            Vector3 startPos = GetPoint(index).position;
            Vector3 endPos = GetPoint(index + 1).position;
            Vector3 finalPosition = Vector3.Lerp(startPos, endPos, ratioOfRemainingLine);
            return finalPosition;
        }
        
        public static Vector3 GetMidpoint(PathPoint startPoint, PathPoint endPoint) {
            return Vector3.Lerp(startPoint.position, endPoint.position, 0.5f);
        }

        public Vector3 GetNormalizedForwardVector(PathPoint point) {
            Vector3 forwardVector = Vector3.zero;

            if (point.index > 0) {
                forwardVector += (point.position - points[point.index - 1].position).normalized;
            }

            if (point.index < points.Count - 1) {
                forwardVector += (points[point.index + 1].position - point.position).normalized;
            }

            return forwardVector.normalized;
        }

        public PathPoint GetPoint(int index) {
            return points[index];
        }

        public void Remove(PathPoint point) {
            int index = point.index;
            points.Remove(point);
            for (int i = index; i < points.Count; i++) {
                points[i].index = i;
            }
        }


        public PathPoint Add(int index, Vector3 position) {
            PathPoint pathPoint = new PathPoint();
            pathPoint.position = position;
            pathPoint.index = index;
            if (index == points.Count) {
                points.Add(pathPoint);
                return pathPoint;
            }
            points.Insert(index, pathPoint);
            for (int i = index + 1; i < points.Count; i++) {
                points[i].index = i;
            }

            return pathPoint;
        }
        

        public void MarkAsChanged() {
            PathChanged?.Invoke();
        }
    }
}
