using System;
using System.Collections.Generic;
using UnityEngine;

namespace Main {
    [Serializable]
    public class ConveyorCrossSection {
        public List<Vector3> points;

        public Vector3 Midpoint {
            get {
                Vector3 total = new Vector3();
                foreach (Vector3 point in points) {
                    total += point;
                }

                return total / points.Count;
            }
        }

        public float VerticalMidpoint => Midpoint.x;

        public float Width {
            get {
                float maxX = points[0].x;
                float minX = points[0].x;
                foreach (Vector3 point in points) {
                    if (point.x > maxX) maxX = point.x;
                    if (point.x < minX) minX = point.x;
                }

                return Mathf.Abs(maxX - minX);
            }
        }

    }
}
