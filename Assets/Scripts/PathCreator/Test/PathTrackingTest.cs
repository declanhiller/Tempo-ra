using System;
using System.Collections;
using UnityEngine;

namespace PathCreator.Test {
    
    [ExecuteInEditMode]
    public class PathTrackingTest : MonoBehaviour {

        [SerializeField] private Path path;

        [SerializeField] private float speed;

        [SerializeField, Range(0, 1)] private float lerp;

        private void Start() {
            Invoke(nameof(Setup), 3f);
        }

        private void Setup() {
            StartCoroutine(LerpPath());
        }

        private void Update() {
            Vector3 position = path.Lerp(lerp);
            transform.position = position;
        }

        private IEnumerator LerpPath() {
            float actualSpeed = speed / path.Length;
            float t = 0;
            while (t <= 1) {
                lerp = t;
                t += Time.deltaTime * actualSpeed;
                yield return new WaitForEndOfFrame();
            }
        }
        
        


    }
}
