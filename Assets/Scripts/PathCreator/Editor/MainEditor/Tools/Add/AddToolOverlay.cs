using System;
using PathCreator.Editor;
using PathCreator.Editor.MainEditor.Tools.Add;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[Overlay(typeof(SceneView), "Add Tool Settings")]
public class AddToolOverlay : Overlay, ITransientOverlay {
    
    public bool visible => AddTool.IsActive;

    public static AddToolOverlay Instance;

    private static readonly Color selectedColor = new Color(1, 1, 0.769f);
    private static readonly Color normalColor = new Color(0, 0, 0);
    
    public static event Action<bool> SnapModeSwitched;
    

    private Button _snap;
    private Button _free;

    public override VisualElement CreatePanelContent() {

        Instance = this;

        VisualTreeAsset uxml =
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/PathCreator/Editor/XML/AddToolOverlay.uxml");
        VisualElement root = new VisualElement {
            name = "Move Tool Settings",
            style = {
                minWidth = 300
            }
        };
        uxml.CloneTree(root);

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
    
}
