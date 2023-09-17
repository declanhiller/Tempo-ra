using PathCreator.Interfaces;
using UnityEditor;
using UnityEngine;

namespace PathCreator.Editor {
    public class StraightSegmentRenderer : ISegmentRenderer {

        private readonly Color _lineColor = Color.red;
        
        public bool IsRightTypeOfPath(Segment segment) {
            return segment.StartPoint is StraightPoint && segment.EndPoint is StraightPoint;
        }

        public void Render(Segment segment) {
            using (new Handles.DrawingScope(_lineColor)) {
                Handles.DrawLine(segment.StartPoint.Position, segment.EndPoint.Position, 2f);
            }
        }
    }
}
