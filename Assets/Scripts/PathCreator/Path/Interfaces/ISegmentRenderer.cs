namespace PathCreator.Interfaces {
    public interface ISegmentRenderer {
        public bool IsRightTypeOfPath(Segment segment);

        public void Render(Segment segment);

    }
}
