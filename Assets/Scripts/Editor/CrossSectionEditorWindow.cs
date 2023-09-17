using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor {
    public class CrossSectionEditorWindow : EditorWindow {

        private VisualTreeAsset baseXML;
        
        [MenuItem("Window/Map Creator/Cross Section Editor")]
        public static void ShowWindow() {
            CrossSectionEditorWindow crossSectionEditorWindow = GetWindow<CrossSectionEditorWindow>();
            crossSectionEditorWindow.titleContent = new GUIContent("Cross Section Editor");
        }

        private void CreateGUI() {
            VisualElement root = rootVisualElement;
            baseXML.CloneTree(root);
        }

        
        
    }
}
