using System;
using PathCreator.Editor.MainEditor.Data;

namespace PathCreator.Editor {
    public class PathEditorState {

        private static PathEditorState _instance;
        private MoveType _moveType;
        private SnapType _snapType;
        
        public static PathEditorState Instance => _instance ??= new PathEditorState();

        public Path Path { get; set; }
        public Grid2D Grid { get; set; }

        public static event Action<MoveType> MoveTypeChanged;
        public MoveType moveType {
            get => _moveType;
            set {
                _moveType = value;
                MoveTypeChanged?.Invoke(_moveType);
            }
        }

        public static event Action<SnapType> SnapTypeChanged;
        public SnapType snapType {
            get => _snapType;
            set {
                _snapType = value;
                SnapTypeChanged?.Invoke(_snapType);
            }
        }


        public enum MoveType {
            TwoDimensional, ThreeDimensional
        }

        public enum SnapType {
            Snap, Free
        }
        
    }
}
