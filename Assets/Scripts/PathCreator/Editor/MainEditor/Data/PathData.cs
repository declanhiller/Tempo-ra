using DefaultNamespace;

namespace PathCreator.Editor.MainEditor.Data {
    public class PathData {
        public Path path { get; }

        public Grid2D grid { get;}

        public PathData(Path path, Grid2D grid) {
            this.path = path;
            this.grid = grid;
        }

    }
}
