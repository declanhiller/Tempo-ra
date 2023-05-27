using System;
using System.Collections;
using UnityEngine;

namespace PathCreator.Test {
    
    public class PathTrackingTest : MonoBehaviour {

        [SerializeField] private Path path;

        [SerializeField] private float speed;

        private void Start() {
            Invoke(nameof(Setup), 3f);
        }

        private void Setup() {
            StartCoroutine(LerpPath());
        }

        private IEnumerator LerpPath() {
            float actualSpeed = speed / path.Length;
            float t = 0;
            while (t <= 1) {
                Vector3 position = path.Lerp(t);
                transform.position = position;
                t += Time.deltaTime * actualSpeed;
                yield return new WaitForEndOfFrame();
            }
        }


    }
}
