using System;
using DefaultNamespace;
using PathCreator;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ConveyorBelt))]
public class ConveyorBeltEditor : Editor {

    private ConveyorBelt conveyorBelt;
    private Path path;
    
    private void OnSceneGUI() {
        
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
