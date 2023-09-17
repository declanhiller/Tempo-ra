using System;
using System.Collections.Generic;
using Main;
using PathCreator;
using UnityEditor;
using UnityEngine;

namespace DefaultNamespace {
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(Path))]
    public class ConveyorBelt : MonoBehaviour {


        [SerializeField] private Path path;
        [SerializeField] private List<SushiData> sushi;
        [SerializeField] private float width;

        [SerializeField] private int curveResolution;

        [SerializeField] public ConveyorCrossSection crossSection = new();

        public List<Vector3> proposedPoints;

        public void CreateBeltMesh(MeshFilter meshFilter) {
            
            if (path == null) {
                path = GetComponent<Path>();
            }

            if (path.Count < 2) {

                Mesh emptyMesh = new Mesh();
                meshFilter.mesh = emptyMesh;
                Debug.LogWarning("Your path needs to have at least 2 points to generate a mesh");
                return;
            }

            proposedPoints = new List<Vector3>();


            Vector3[] vertices = new Vector3[path.Count * crossSection.points.Count];
            Vector2[] uvs = new Vector2[vertices.Length];
            int numberOfTrisInASegment = (path.Count - 1) * (crossSection.points.Count) * 2;
            int[] tris = new int[numberOfTrisInASegment * 3]; // multiplying 3 for each vertex of the triangle
            int vertIndex = 0;
            int triIndex = 0;
            Vector3 zero = path.GetPoint(0).position;

            for (int i = 0; i < path.Count; i++) {
                PathPoint endPoint = path.GetPoint(i);
                if (i == 0) {
                    Vector3 endPosition = endPoint.position;

                    Vector3 left = GetLeftPoint(path.GetNormalizedForwardVector(endPoint), width) + endPosition;
                    Vector3 right = GetRightPoint(path.GetNormalizedForwardVector(endPoint), width) + endPosition;

                    // vertices[vertIndex] = left - zero;
                    // vertices[vertIndex + 1] = right - zero;
                    
                    Vector3[] crossSectionPoints = GetCrossSectionPoints(path.GetNormalizedForwardVector(endPoint), endPosition, Vector3.Distance(left, right));
                    
                    for (int j = 0; j < crossSection.points.Count; j++) {
                        vertices[vertIndex + j] = crossSectionPoints[j] - zero;
                    }
                    
                }
                else {
                    PathPoint startPoint = path.GetPoint(i - 1);

                    Vector3 startPosition = startPoint.position;
                    Vector3 endPosition = endPoint.position;

                    Vector3 endPositionLeftDirection = GetLeftDirection(path.GetNormalizedForwardVector(endPoint));
                    Vector3 endPositionRightDirection = GetRightDirection(path.GetNormalizedForwardVector(endPoint));

                    Vector3 midpoint = Path.GetMidpoint(startPoint, endPoint);
                    Vector3 midpointForwardVector = (endPosition - startPosition).normalized;
                    Vector3 positionLeftOfMidpoint = GetLeftPoint(midpointForwardVector, width) + midpoint;
                    Vector3 positionRightOfMidpoint = GetRightPoint(midpointForwardVector, width) + midpoint;

                    Vector3 leftPointPosition;
                    Vector3 rightPointPosition;
                    LineLineIntersection(out leftPointPosition, positionLeftOfMidpoint, midpointForwardVector,
                        endPosition,
                        endPositionLeftDirection);
                    LineLineIntersection(out rightPointPosition, positionRightOfMidpoint, midpointForwardVector,
                        endPosition, endPositionRightDirection);
                    
                    Vector3[] crossSectionPoints = GetCrossSectionPoints(path.GetNormalizedForwardVector(endPoint), endPosition, Vector3.Distance(leftPointPosition, rightPointPosition));
                    
                    for (int j = 0; j < crossSection.points.Count; j++) {
                        vertices[vertIndex + j] = crossSectionPoints[j] - zero;
                    }
                    
                    // vertices[vertIndex] = leftPointPosition - zero;
                    // vertices[vertIndex + 1] = rightPointPosition - zero;
                }

                float completionPercent = i / (float) (path.Count - 1);
                float v = 1 - Mathf.Abs(2 * completionPercent - 1);
                for (int j = 0; j < crossSection.points.Count; j++) {
                    uvs[vertIndex + j] = new Vector2((float) j / (crossSection.points.Count - 1), v);
                }

                if (i < path.Count - 1) {
                    int crossSectionPointsCount = crossSection.points.Count;
                    for (int j = 0; j < crossSectionPointsCount - 1; j++) {
                        int relativeJZero = j + vertIndex;

                        tris[triIndex] = relativeJZero + crossSectionPointsCount + 1;
                        tris[triIndex + 1] = relativeJZero + crossSectionPointsCount;
                        tris[triIndex + 2] = relativeJZero;

                        tris[triIndex + 3] = relativeJZero + 1;
                        tris[triIndex + 4] = relativeJZero + crossSectionPointsCount + 1;
                        tris[triIndex + 5] = relativeJZero;
                        
                        triIndex += 6;
                    }

                    int indexOfEndingPoint = vertIndex + crossSectionPointsCount - 1;
                    
                    tris[triIndex] = vertIndex;
                    tris[triIndex + 1] = indexOfEndingPoint + 1;
                    tris[triIndex + 2] = indexOfEndingPoint;

                    tris[triIndex + 3] = indexOfEndingPoint + 1;
                    tris[triIndex + 4] = indexOfEndingPoint + crossSectionPointsCount;
                    tris[triIndex + 5] = indexOfEndingPoint;

                    triIndex += 6;

                }

                vertIndex += crossSection.points.Count;
            }

            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = tris;
            mesh.uv = uvs;
            mesh.RecalculateNormals();
            // mesh.
            meshFilter.mesh = mesh;
        }

        public Vector3[] GetCrossSectionPoints(Vector3 forward, Vector3 position, float width) {
            Vector2 rightZero = Vector2.up;
            float signedAngle = Vector2.SignedAngle(rightZero, new Vector2(forward.x, forward.z));
            float requiredScaleRatio = width / crossSection.Width;
            float midPoint = crossSection.VerticalMidpoint;

            Vector3[] returnPoints = new Vector3[crossSection.points.Count];

            for (int i = 0; i < crossSection.points.Count; i++) {
                Vector3 point = crossSection.points[i];
                float distToMid = (point.x - midPoint) * requiredScaleRatio;
                
                Vector3 scaledPoint = new Vector3(midPoint + distToMid, point.y, point.z);
                float x = scaledPoint.x * Mathf.Cos(signedAngle * Mathf.Deg2Rad) -
                          scaledPoint.z * Mathf.Sin(signedAngle * Mathf.Deg2Rad);
                float z = scaledPoint.x * Mathf.Sin(signedAngle * Mathf.Deg2Rad) +
                          scaledPoint.z * Mathf.Cos(signedAngle * Mathf.Deg2Rad);
                Vector3 newPathPoint = new Vector3(x, point.y, z) + position;
                proposedPoints.Add(newPathPoint);
                returnPoints[i] = newPathPoint;
            }

            return returnPoints;
        }

        private Vector3 GetLeftPoint(Vector3 forwardVector, float width) {
            return GetLeftDirection(forwardVector) * width * 0.5f;
        }

        private Vector3 GetLeftDirection(Vector3 forwardVector) {
            return new Vector3(-forwardVector.z, forwardVector.y, forwardVector.x);
        }

        private Vector3 GetRightPoint(Vector3 forwardVector, float width) {
            return GetRightDirection(forwardVector) * width * 0.5f;
        }

        private Vector3 GetRightDirection(Vector3 forwardVector) {
            return new Vector3(forwardVector.z, forwardVector.y, -forwardVector.x);
        }

        public static bool LineLineIntersection(out Vector3 intersection,
            Vector3 linePoint1, Vector3 lineDirection1,
            Vector3 linePoint2, Vector3 lineDirection2) {

            Vector3 lineVec3 = linePoint2 - linePoint1;
            Vector3 crossVec1and2 = Vector3.Cross(lineDirection1, lineDirection2);
            Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineDirection2);
            float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

            //is coplanar, and not parallel
            if (Mathf.Abs(planarFactor) < 0.0001f
                && crossVec1and2.sqrMagnitude > 0.0001f) {
                float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
                intersection = linePoint1 + (lineDirection1 * s);
                return true;
            }
            else {
                intersection = Vector3.zero;
                return false;
            }
        }

        public struct Point {
            private Vector3 position;
            private Color color;
        }

    }
}
