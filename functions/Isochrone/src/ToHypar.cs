using BH.oM.Base;
using Elements.Geometry;
using Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Isochrone
{
    public static partial class Convert
    {
        public static object IToHypar(this IBHoMObject bHoMObject)
        {
            return ToHypar(bHoMObject as dynamic);
        }

        /***************************************************/

        public static Elements.Geometry.Vector3 ToHypar(this BH.oM.Geometry.Point point)
        {
            return new Vector3(point.X, point.Y, point.Z);
        }

        /***************************************************/
        /**** Public Methods  - Curves                  ****/
        /***************************************************/
        public static Elements.Geometry.Curve ToHypar(this BH.oM.Geometry.ICurve curve)
        {
            return ToHypar(curve as dynamic);
        }

        /***************************************************/

        public static Elements.Geometry.Line ToHypar(this BH.oM.Geometry.Line line)
        {
            return new Elements.Geometry.Line(line.Start.ToHypar(), line.End.ToHypar());
        }

        /***************************************************/

        private static Elements.Geometry.Polyline ToHypar(this BH.oM.Geometry.Polyline polyline)
        {
            List<Vector3> pts = new List<Vector3>();
            foreach (var v in polyline.ControlPoints)
                pts.Add(v.ToHypar());

            return new Elements.Geometry.Polyline(pts);
        }

        /***************************************************/
        /**** Private Methods - Fallback                ****/
        /***************************************************/

        private static object ToHypar(this IObject obj)
        {
            return null;
        }
    }
}
