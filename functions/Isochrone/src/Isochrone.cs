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
using BH.oM.Base;
using BH.oM.Dimensional;

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
            inputModels.TryGetValue("Links", out var model);

            if (model == null || model.AllElementsOfType<ModelCurve>().Count() == 0)
            {
                output.Errors.Add($"No travel network found.");
                return output;
            }
            var modelCurves = model.AllElementsOfType<ModelCurve>();
            string report = "modelcurves found\n";
            output = new IsochroneOutputs(modelCurves.Count(), report);

            List<BH_oM_Geo.ICurve> connectingCurves = new List<BH_oM_Geo.ICurve>();
            foreach (var modelCurve in modelCurves)
            {
                connectingCurves.Add(modelCurve.Curve.ToBHoM());
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
            var start = FindStartNode(mainGraph, input.ClosestPointToStart);
            List<ModelCurve> curves = new List<ModelCurve>();

            if(input.VisualisationMethod == IsochroneInputsVisualisationMethod.Render_graph_link_lines)
                output.Model.AddElements(IsoCurves(mainGraph, start, input.TravelSpeed, input.TimeBand));

            else
                output.Model.AddElements(IsoNodes(mainGraph, start, input.TravelSpeed, input.TimeBand, input.NodeRadius));
            return output;
            //hypar test generate --workflow-id=59c4ea6b-a938-441c-8b8a-37f4667b76ba
        }

        private static List<ModelCurve> IsoCurves(Graph graph, BH.oM.SpaceSyntax.Node start, double speed, double timeband)
        {
            var curveGroups = BH.Engine.SpaceSyntax.Compute.IsochroneCurveSet(graph, start, speed, timeband);
            List<ModelCurve> curves = new List<ModelCurve>();
            int g = 0;
            foreach (var group in curveGroups)
            {
                foreach (var curve in group)
                {
                    var col = new Color(System.Drawing.Color.FromArgb(m_Colors[g][0], m_Colors[g][1], m_Colors[g][2]));
                    ModelCurve modelCurve = new ModelCurve(curve.ToHypar(), new Material(g.ToString(), col) { EdgeDisplaySettings = new EdgeDisplaySettings { LineWidth = 5 } });
                    curves.Add(modelCurve);
                }
                g++;
                if (g > m_Colors.Count - 1)
                    g = 0;
            }
            return curves;
        }

        private static List<ModelCurve> IsoNodes(Graph graph, BH.oM.SpaceSyntax.Node start, double speed, double timeband, double radius)
        {
            var pointGroups = BH.Engine.SpaceSyntax.Compute.IsochronePointSet(graph, start, speed, timeband);
            List<ModelCurve> points = new List<ModelCurve>();
            int g = 0;
            foreach (var group in pointGroups)
            {
                foreach (var point in group)
                {
                    
                    var col = new Color(System.Drawing.Color.FromArgb(m_Colors[g][0], m_Colors[g][1], m_Colors[g][2]));
                    var dot = new Elements.Geometry.Circle(point.Node.Position.ToHypar(), radius);
                    ModelCurve modelCurve = new ModelCurve(dot, new Material(g.ToString(), col) { EdgeDisplaySettings = new EdgeDisplaySettings { LineWidth = 5 } });
                    //ModelPoint 
                    points.Add(modelCurve);
                }
                g++;
                if (g > m_Colors.Count - 1)
                    g = 0;
            }
            return points;
        }

        private static BH.oM.SpaceSyntax.Node FindStartNode(Graph graph, Vector3 point)
        {
            Point sPoint = point.ToBHoM();
            BH.oM.SpaceSyntax.Node start = new BH.oM.SpaceSyntax.Node();
            double minSq = double.MaxValue;
            foreach(var entity in graph.Entities)
            {
                var n = (BH.oM.SpaceSyntax.Node)entity.Value;
                
                double sqd = n.Position.SquareDistance(sPoint);
                if(sqd < minSq) 
                { 
                    minSq = sqd;
                    start = n;
                }
            }
            return start;
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

        
    }
}