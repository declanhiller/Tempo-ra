using System;
using System.Collections.Generic;
using DefaultNamespace;
using PathCreator;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ConveyorBelt))]
public class ConveyorBeltEditor : Editor {

    private ConveyorBelt conveyorBelt;
    private Path path;
    
    
    private void OnSceneGUI() {
        using (new Handles.DrawingScope(Color.blue)) {

            List<Vector3> points = conveyorBelt.proposedPoints;
            int crossSectionPointsCount = conveyorBelt.crossSection.points.Count;
            int pointCounter = 0;
            for (int i = 0; i < points.Count - 1; i++) {
                Vector3 startPosition = points[i];
                Vector3 endPosition = points[i + 1];
                Handles.DrawLine(startPosition, endPosition, 4f);
                pointCounter++;
                if (pointCounter >= crossSectionPointsCount - 1) {
                    pointCounter = 0;
                    Vector3 crossSectionBeginningPosition = points[i - crossSectionPointsCount + 2];
                    Handles.DrawLine(endPosition, crossSectionBeginningPosition, 4f);
                    i++;
                }
            }
            //
            // foreach (Vector3 point in points) {
            //     Handles.FreeMoveHandle(point, Quaternion.identity, 0.1f, Vector2.zero, Handles.SphereHandleCap);
            // }
        }
    }

    
    
    private void OnEnable() {
        conveyorBelt = (ConveyorBelt) target;
        path = conveyorBelt.GetComponent<Path>();
        path.PathChanged += CreateNewMesh;
    }

    private void CreateNewMesh() {
        conveyorBelt.CreateBeltMesh(conveyorBelt.GetComponent<MeshFilter>());
    }

    private void OnDisable() {
        path.PathChanged -= CreateNewMesh;
    }

}
