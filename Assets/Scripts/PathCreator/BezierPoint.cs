using PathCreator.Interfaces;
using UnityEngine;

namespace PathCreator {
    public class BezierPoint : IPoint {

        public Vector3 StartHandle;
        public Vector3 EndHandle;
        public Vector3 Position { get; set; }
    }
}
