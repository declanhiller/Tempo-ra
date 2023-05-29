using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace PathCreator {
    [Serializable]
    public class PathPoint {
        public Vector3 position;
        public int index;
        [FormerlySerializedAs("roundness")] public float radius; //How far away that circle that dictates the path is
    }
}
