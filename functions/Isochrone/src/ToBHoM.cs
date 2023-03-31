using BH.oM.Base;
using BH.oM.Geometry;
using Elements;
using Elements.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Isochrone
{
    public static partial class Convert
    {
        public static object IToBHom(this Element element)
        {
            return ToBHoM(element as dynamic);
        }

        /***************************************************/

        public static BH.oM.Geometry.Point ToBHoM(this Vector3 vector3)
        {
            return BH.Engine.Geometry.Create.Point(vector3.X, vector3.Y, vector3.Z);
        }

        /***************************************************/
        /**** Public Methods  - Curves                  ****/
        /***************************************************/
        public static BH.oM.Geometry.ICurve ToBHoM(this Curve curve)
        {
            return ToBHoM(curve as dynamic);
        }

        /***************************************************/

        public static BH.oM.Geometry.Line ToBHoM(this Elements.Geometry.Line line)
        {
           return BH.Engine.Geometry.Create.Line(line.Start.ToBHoM(), line.End.ToBHoM());
        }

        /***************************************************/

        private static BH.oM.Geometry.Polyline ToBHoM(this Elements.Geometry.Polyline polyline)
        {
            List<BH.oM.Geometry.Point> pts = new List<Point>();
            foreach (var v in polyline.Vertices)
                pts.Add(v.ToBHoM());

            return BH.Engine.Geometry.Create.Polyline(pts);
        }

        /***************************************************/
        /**** Private Methods - Fallback                ****/
        /***************************************************/

        private static object ToBHoM(this IObject obj)
        {
            return null;
        }
    }
}
