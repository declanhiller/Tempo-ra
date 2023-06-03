using System;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace PathCreator.Editor.MainEditor.Tools.Move {
    [Overlay(typeof(SceneView), "Move Tool Settings")]
    public class MoveToolOverlay : Overlay, ITransientOverlay {

        public bool visible => MoveTool.IsActive;

        public static MoveToolOverlay Instance;

        private static readonly Color selectedColor = new Color(1, 1, 0.769f);
        private static readonly Color normalColor = new Color(0, 0, 0);

        public static event Action<Vector3> PositionChanged;
        
        private FloatField _x;
        private FloatField _y;
        private FloatField _z;

        private Button _snap;
        private Button _free;

        public override VisualElement CreatePanelContent() {

            Instance = this;

            VisualTreeAsset uxml =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/PathCreator/Editor/XML/MoveToolInfo.uxml");
            VisualElement root = new VisualElement {
                name = "Move Tool Settings",
                style = {
                    minWidth = 300
                }
            };
            uxml.CloneTree(root);
        
            _x = root.Query<FloatField>("xField").First();
            _y = root.Query<FloatField>("yField").First();
            _z = root.Query<FloatField>("zField").First();

            _x.SetValueWithoutNotify(0);
            _y.SetValueWithoutNotify(0);
            _z.SetValueWithoutNotify(0);

            _x.ElementAt(0).style.minWidth = 10;
            _y.ElementAt(0).style.minWidth = 10;
            _z.ElementAt(0).style.minWidth = 10;

            _x.ElementAt(1).style.minWidth = 30;
            _y.ElementAt(1).style.minWidth = 30;
            _z.ElementAt(1).style.minWidth = 30;
            _x.ElementAt(1).style.unityTextAlign = TextAnchor.MiddleCenter;
            _y.ElementAt(1).style.unityTextAlign = TextAnchor.MiddleCenter;
            _z.ElementAt(1).style.unityTextAlign = TextAnchor.MiddleCenter;


            _x.isDelayed = false;
            _y.isDelayed = false;
            _z.isDelayed = false;

            _x.RegisterValueChangedCallback(evt => { PositionChanged?.Invoke(new Vector3(evt.newValue, _y.value, _z.value)); });


            _y.RegisterValueChangedCallback(evt => { PositionChanged?.Invoke(new Vector3(_x.value, evt.newValue, _z.value)); });

            _z.RegisterValueChangedCallback(evt => { PositionChanged?.Invoke(new Vector3(_x.value, _y.value, evt.newValue)); });

            _snap = root.Query<Button>("snap").First();
            _free = root.Query<Button>("free").First();
        
            SwitchSnapType(PathEditorState.Instance.snapType);
            
            _snap.clicked += () => {
                PathEditorState.Instance.snapType = PathEditorState.SnapType.Snap;
            };
            _free.clicked += () => {
                PathEditorState.Instance.snapType = PathEditorState.SnapType.Free;
            };

            PathEditorState.SnapTypeChanged += SwitchSnapType;
            
            return root;
        }

        public override void OnWillBeDestroyed() {
            PathEditorState.SnapTypeChanged -= SwitchSnapType;
        }

        private void SwitchSnapType(PathEditorState.SnapType type) {
            switch (type) {
                case PathEditorState.SnapType.Free:
                    SetBorderColor(_snap, normalColor);
                    SetBorderColor(_free, selectedColor);
                    break;
                case PathEditorState.SnapType.Snap:
                    SetBorderColor(_snap, selectedColor);
                    SetBorderColor(_free, normalColor);
                    break;
            }
        }
        
        private void SetBorderColor(VisualElement o, Color color) {
            o.style.borderBottomColor = color;
            o.style.borderLeftColor = color;
            o.style.borderRightColor = color;
            o.style.borderTopColor = color;
        }

        public void UpdateValues(Vector3 position) {
            _x.SetValueWithoutNotify(position.x);
            _y.SetValueWithoutNotify(position.y);
            _z.SetValueWithoutNotify(position.z);
        }

    }
}
