using UnityEngine;

namespace DefaultNamespace {
    public class Sushi : MonoBehaviour {
        public void TappedOn() {
            Metronome.Accuracy accuracy = Metronome.INSTANCE.GetAccuracy();
            Destroy(gameObject);
        }
    }
}
