using BH.Engine.Analytical;
using BH.oM.Analytical.Elements;
using BH.oM.Structure.Elements;
using Elements;
using Elements.Geometry;
using System.Collections.Generic;
using BH_oM_Geo = BH.oM.Geometry;
using BH.oM.SpaceSyntax;
using BH.Engine.SpaceSyntax;
using Amazon.S3.Model.Internal.MarshallTransformations;
using BH.oM.Geometry;
using BH.Engine.Geometry;

namespace Isochrone
{
    public static class Isochrone
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model">The input model.</param>
        /// <param name="input">The arguments to the execution.</param>
        /// <returns>A IsochroneOutputs instance containing computed results and the model with any new elements.</returns>
        public static IsochroneOutputs Execute(Dictionary<string, Model> inputModels, IsochroneInputs input)
        {
            var output = new IsochroneOutputs();
            inputModels.TryGetValue("Streets", out var model);

            if (model == null || model.AllElementsOfType<ModelCurve>().Count() == 0)
            {
                output.Errors.Add($"No street network found.");
                return output;
            }
            var modelCurves = model.AllElementsOfType<ModelCurve>();
            string report = "modelcurves found\n";
            output = new IsochroneOutputs(modelCurves.Count(), report);

            List<BH_oM_Geo.ICurve> connectingCurves = new List<BH_oM_Geo.ICurve>();
            foreach (var modelCurve in modelCurves)
            {
                connectingCurves.Add(modelCurve.Curve.ToBHoMCurve());
            }

            //Create the graph
            BH.oM.SpaceSyntax.Node prototypeNode = new BH.oM.SpaceSyntax.Node();
            prototypeNode.Position = new BH_oM_Geo.Point();
            prototypeNode.Name = "node";
            Graph graph = BH.Engine.Analytical.Create.Graph(connectingCurves, prototypeNode, relationDirection: BH.oM.Analytical.Elements.RelationDirection.Both);
            List<Graph> subgraphs = graph.SubGraphs();
            Graph mainGraph = new Graph();
            foreach (var subgraph in subgraphs)
            {
                if (subgraph.Entities.Count() > mainGraph.Entities.Count())
                    mainGraph = subgraph;
            }
            report += "graph constructed with " + mainGraph.Entities.Count + "entities\n";
            var start = (BH.oM.SpaceSyntax.Node)mainGraph.Entity("node_697");
            var curveGroups = BH.Engine.SpaceSyntax.Compute.IsochroneCurveSet(mainGraph, start, input.TravelSpeed, input.TimeBand);
            report += "Isochrone computed with " + curveGroups.Count + "zone bands\n";
            output = new IsochroneOutputs(modelCurves.Count(), report);
            int g = 0;
            foreach (var group in curveGroups)
            {
                foreach (var curve in group)
                {
                    var col = new Color(System.Drawing.Color.FromArgb(m_Colors[g][0], m_Colors[g][1], m_Colors[g][2]));
                    ModelCurve modelCurve = new ModelCurve(curve.ToHyparLine(), new Material(g.ToString(), col) { EdgeDisplaySettings = new EdgeDisplaySettings { LineWidth = 5 } });
                    output.Model.AddElement(modelCurve);
                }
                g++;
                if (g > m_Colors.Count - 1)
                    g = 0;
            }

            return output;
        }

        private static List<List<int>> m_Colors = new List<List<int>>()
        {
            new List<int>(){243,57,0 },
            new List<int>(){246,161,20},
            new List<int>(){204,231,29},
            new List<int>(){64,184,29},
            new List<int>(){11,184,147},
            new List<int>(){23,122,189},
            new List<int>(){87,18,130},
            new List<int>(){197,0,118},
            new List<int>(){235,26,62},
            new List<int>(){243,57,0} 
        };

        ///////////////////////////////////////////////////
        ///BHoM to hypar converts 

        private static BH_oM_Geo.ICurve ToBHoMCurve(this Curve curve)
        {
            Elements.Geometry.Polyline pl = curve.ToPolyline(1);
            return BH.Engine.Geometry.Create.Line(pl.Vertices[0].ToBHoMPoint(), pl.Vertices.Last().ToBHoMPoint());
        }

        private static BH_oM_Geo.Point ToBHoMPoint(this Vector3 vector3)
        {
            return BH.Engine.Geometry.Create.Point(vector3.X, vector3.Y, vector3.Z);    
        }

        private static Vector3 ToHyparPoint(this Point point)
        {
            return new Vector3(point.X, point.Y, point.Z);
        }

        private static Elements.Geometry.Line ToHyparLine(this ICurve curve)
        {
            return new Elements.Geometry.Line(curve.IStartPoint().ToHyparPoint(),curve.IEndPoint().ToHyparPoint());
        }
    }
}