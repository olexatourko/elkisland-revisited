using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace WaypointPlanning {
    public class WaypointPlanner {
        public static IList<Edge> CreateGraph(IList<Waypoint> waypoints) {
            List<Waypoint> activeWaypoints = new List<Waypoint>();
            List<Triangle> triangles = new List<Triangle>();

            // Find a supertriangle containing all the waypoints
            Triangle superTriangle = FindSuperTriangle(waypoints);
            activeWaypoints.Add(superTriangle.WaypointA);
            activeWaypoints.Add(superTriangle.WaypointB);
            activeWaypoints.Add(superTriangle.WaypointC);
            triangles.Add(superTriangle);

            foreach (var waypoint in waypoints) {
                activeWaypoints.Add(waypoint);
                // Find invalid triangles for this waypoint:
                // Triangles who's circumcircle contains the waypoint
                List<Triangle> invalidTriangles = new List<Triangle>();
                foreach (var triangle in triangles) {
                    if (triangle.IsInCircumcircle(waypoint)) invalidTriangles.Add(triangle);
                }
                // Remove invalid triangles from active triangles list
                triangles = triangles.Except(invalidTriangles).ToList();

                // Build a polygon/boundary around the hole that needs to be retriangulated
                HashSet<Edge> boundary = new HashSet<Edge>();
                Dictionary<Edge, int> edgeCounts = new Dictionary<Edge, int>();
                foreach (var triangle in invalidTriangles) {
                    foreach (var edge in triangle.Edges) {
                        if (!edgeCounts.ContainsKey(edge)) edgeCounts[edge] = 1;
                        else edgeCounts[edge] += 1;
                    }
                }
                // TODO: Possibly replace this with a LINQ query
                foreach (var (edge, count) in edgeCounts) {
                    if (count == 1) boundary.Add(edge);
                }
                // Create a new triangle between every edge in the boundary and the current waypoint
                foreach (var edge in boundary) {
                    triangles.Add(new Triangle(
                        waypoint,
                        edge.WaypointA,
                        edge.WaypointB
                    ));
                }
            }
            
            IList<Edge> finalEdges = new List<Edge>();
            ISet<Waypoint> excludedWaypoints = new HashSet<Waypoint> {
                superTriangle.WaypointA, superTriangle.WaypointB, superTriangle.WaypointC
            };
            foreach (var triangle in triangles) {
                foreach (var edge in triangle.Edges) {
                    if (excludedWaypoints.Contains(edge.WaypointA) || excludedWaypoints.Contains(edge.WaypointB)) continue;
                    finalEdges.Add(edge);
                }
            }
            return finalEdges;
        }

        // Finds a triangle containing all the given points
        protected static Triangle FindSuperTriangle(IEnumerable<Waypoint> waypoints) {
            List<Nayuki.Point> points = new List<Nayuki.Point>();
            foreach(var waypoint in waypoints) {
                points.Add(new Nayuki.Point(waypoint.Position.x, waypoint.Position.y));
            }
            Nayuki.Circle circle = Nayuki.SmallestEnclosingCircle.MakeCircle(points);
            Vector2 circleCenter = new Vector2(
                (float) circle.c.x,
                (float) circle.c.y
            );
            Vector2 a = new Vector2(
                circleCenter.x + ((float) circle.r * 2),
                circleCenter.y
            );
            Vector2 b = (a - circleCenter).Rotated(2 * (float) Math.PI / 3) + circleCenter;
            Vector2 c = (a - circleCenter).Rotated(4 * (float) Math.PI / 3) + circleCenter;
            return new Triangle(
                new Waypoint(a),
                new Waypoint(b),
                new Waypoint(c)
            );
        }
    }
}