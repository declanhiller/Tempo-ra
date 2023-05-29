using UnityEngine;

namespace PathCreator.Editor.MainEditor.Data {
    public class FakePoint {
        public Vector3 Position { get; private set; }

        public FakePoint(Vector3 position) {
            this.Position = position;
        }

    }
}
