using SimpleCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCAD.Commands
{
    class Command_Control
    {
    }
    public class ControlMove: Command
    {
        ControlPoint cp;
        Drawable item;

        public ControlMove()
        {
            cp = null;
            item = null;
        }
        public ControlMove(Drawable item, ControlPoint mouseDownCP)
        {
            this.item = item;
            this.cp = mouseDownCP;
        }

        public override string RegisteredName => "Control.Move";
        public override string Name => "Move";
        public override async Task Apply(CADDocument doc, params string[] args)
        {
            if(cp== null ||item == null)
            {
                return;
            }
            Editor ed = doc.Editor;

            Drawable consItem = item.Clone();
            doc.Transients.Add(consItem);

            ResultMode result = ResultMode.Cancel;
            Matrix2D trans = Matrix2D.Identity;

            if (cp.Type == ControlPointType.Point)
            {
                var res = await doc.Editor.GetPoint(cp.Name, cp.BasePoint,
                    (p) =>
                    {
                        consItem.TransformControlPoints(new int[] { cp.Index }, trans.Inverse);
                        trans = Matrix2D.Translation(p - cp.BasePoint);
                        consItem.TransformControlPoints(new int[] { cp.Index }, trans);
                    });
                trans = Matrix2D.Translation(res.Value - cp.BasePoint);
                result = res.Result;
            }
            else if (cp.Type == ControlPointType.Angle)
            {
                float orjVal = (cp.Location - cp.BasePoint).Angle;
                var res = await doc.Editor.GetAngle(cp.Name, cp.BasePoint,
                    (p) =>
                    {
                        consItem.TransformControlPoints(new int[] { cp.Index }, trans.Inverse);
                        trans = Matrix2D.Rotation(cp.BasePoint, p - orjVal);
                        consItem.TransformControlPoints(new int[] { cp.Index }, trans);
                    });
                trans = Matrix2D.Rotation(cp.BasePoint, res.Value - orjVal);
                result = res.Result;
            }
            else if (cp.Type == ControlPointType.Distance)
            {
                Vector2D dir = (cp.Location - cp.BasePoint).Normal;
                float orjVal = (cp.Location - cp.BasePoint).Length;
                var res = await doc.Editor.GetDistance(cp.Name, cp.BasePoint,
                    (p) =>
                    {
                        consItem.TransformControlPoints(new int[] { cp.Index }, trans.Inverse);
                        trans = Matrix2D.Scale(cp.BasePoint, p / orjVal);
                        consItem.TransformControlPoints(new int[] { cp.Index }, trans);
                    });
                trans = Matrix2D.Scale(cp.BasePoint, res.Value / orjVal);
                result = res.Result;
            }

            doc.Transients.Remove(consItem);

            // Transform the control point
            if (result == ResultMode.OK)
            {
                item.TransformControlPoints(new int[] { cp.Index }, trans);
            }
        }
    }
}
