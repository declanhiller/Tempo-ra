using System;
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

        SetBorderColor(_snap, normalColor);
        SetBorderColor(_free, selectedColor);

        _snap.clicked += () => {
            SnapModeSwitched?.Invoke(true);
            SetBorderColor(_snap, selectedColor);
            SetBorderColor(_free, normalColor);
        };
        _free.clicked += () => {
            SnapModeSwitched?.Invoke(false);
            SetBorderColor(_snap, normalColor);
            SetBorderColor(_free, selectedColor);
        };
        return root;
    }

    private void SetBorderColor(VisualElement o, Color color) {
        o.style.borderBottomColor = color;
        o.style.borderLeftColor = color;
        o.style.borderRightColor = color;
        o.style.borderTopColor = color;
    }
    
}
