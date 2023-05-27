using System;
using System.Collections.Generic;
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
        
        private void Start() {
            // CreateBeltMesh();
        }

        public void CreateBeltMesh(MeshFilter meshFilter) {

            if (path == null) {
                path = GetComponent<Path>();
            }
            
            if (path.Count < 2) {
                throw new Exception("Your path needs to have at least 2 points to generate a mesh");
            }
            

            Vector3[] vertices = new Vector3[path.Count * 2];
            Vector2[] uvs = new Vector2[vertices.Length];
            int[] tris = new int[3 * (2 * (path.Count - 1))];
            int vertIndex = 0;
            int triIndex = 0;
            int index = 0;
            Vector3 zero = path.GetPoint(0).position;

            for (int i = 0; i < path.Count; i++) {
                PathPoint endPoint = path.GetPoint(i);
                if (i == 0) {
                    Vector3 endPosition = endPoint.position;
                    
                    Vector3 left = GetLeftPoint(path.GetNormalizedForwardVector(endPoint), width) + endPosition;
                    Vector3 right = GetRightPoint(path.GetNormalizedForwardVector(endPoint), width) + endPosition;
                    
                    vertices[vertIndex] = left - zero;
                    vertices[vertIndex + 1] = right - zero;
                }
                else {
                    PathPoint startPoint = path.GetPoint(index - 1);
                    
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
                    LineLineIntersection(out leftPointPosition, positionLeftOfMidpoint, midpointForwardVector, endPosition,
                        endPositionLeftDirection);
                    LineLineIntersection(out rightPointPosition, positionRightOfMidpoint, midpointForwardVector,
                        endPosition, endPositionRightDirection);

                    vertices[vertIndex] = leftPointPosition - zero;
                    vertices[vertIndex + 1] = rightPointPosition - zero;
                }

                float completionPercent = index / (float)(path.Count - 1);
                index++;
                float v = 1 - Mathf.Abs(2 * completionPercent - 1);
                uvs[vertIndex] = new Vector2(0, v);
                uvs[vertIndex + 1] = new Vector2(1, v);

                if (i < path.Count - 1) {
                    tris[triIndex] = vertIndex;
                    if (triIndex + 1 >= 0 && triIndex + 1 < tris.Length) tris[triIndex + 1] = vertIndex + 2;
                    tris[triIndex + 2] = vertIndex + 1;
                
                    tris[triIndex + 3] = vertIndex + 2;
                    tris[triIndex + 4] = vertIndex + 3;
                    tris[triIndex + 5] = vertIndex + 1;
                
                    triIndex += 6;
                }
                
                vertIndex += 2;
                
                

            }

            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = tris;
            mesh.uv = uvs;
            meshFilter.mesh = mesh;
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
            Vector3 linePoint2, Vector3 lineDirection2)
        {
 
            Vector3 lineVec3 = linePoint2 - linePoint1;
            Vector3 crossVec1and2 = Vector3.Cross(lineDirection1, lineDirection2);
            Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineDirection2);
            float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);
 
            //is coplanar, and not parallel
            if (Mathf.Abs(planarFactor) < 0.0001f
                && crossVec1and2.sqrMagnitude > 0.0001f)
            {
                float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
                intersection = linePoint1 + (lineDirection1 * s);
                return true;
            }
            else
            {
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
