using PathCreator.Interfaces;

namespace PathCreator {
    public class Segment {
        public IPoint StartPoint { get; set; }
        public IPoint EndPoint { get; set; }
    }
}
