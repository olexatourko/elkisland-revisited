using System;
using Godot;

namespace WaypointPlanning {
    public class Waypoint {
        public Vector2 Position;
        public Waypoint (Vector2 position) {
            Position = position;
        }
    }
    // An undirected edge
    public readonly struct Edge {
        public readonly Waypoint WaypointA;
        public readonly Waypoint WaypointB;
        public Edge(Waypoint waypointA, Waypoint waypointB) {
            WaypointA = waypointA;
            WaypointB = waypointB;
        }
        // https://learn.microsoft.com/en-us/dotnet/api/system.object.equals?view=net-7.0
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;
            Edge edge = (Edge)obj;
            return (
                (WaypointA == edge.WaypointA && WaypointB == edge.WaypointB)
                || (WaypointB == edge.WaypointA && WaypointA == edge.WaypointB)
            );
        }
        // By default GetHashCode works off the instance reference
        // https://stackoverflow.com/questions/34505/is-object-gethashcode-unique-to-a-reference-or-a-value
        public override int GetHashCode()
        {
            if (WaypointA.GetHashCode() < WaypointB.GetHashCode()) {
                return HashCode.Combine(WaypointA, WaypointB);
            } else {
                return HashCode.Combine(WaypointB, WaypointA);
            }
        }
    }

    public readonly struct Triangle {
        public readonly Waypoint WaypointA;
        public readonly Waypoint WaypointB;
        public readonly Waypoint WaypointC;
        public readonly Edge[] Edges;
        public Triangle(Waypoint waypointA, Waypoint waypointB, Waypoint waypointC) {
            WaypointA = waypointA;
            WaypointB = waypointB;
            WaypointC = waypointC;
            Edges = new Edge[]{
                    new Edge(WaypointA, WaypointB),
                    new Edge(WaypointB, WaypointC),
                    new Edge(WaypointC, WaypointA)
            };
        }
        public Circle Circumcircle {
            get {
                Vector2 a = WaypointA.Position;
                Vector2 b = WaypointB.Position;
                Vector2 c = WaypointC.Position;
                float d = 2 * (a.x * (b.y - c.y)
                    + b.x * (c.y - a.y)
                    + c.x * (a.y - b.y));
                float aSq = (a.x * a.x) + (a.y * a.y);
                float bSq = (b.x * b.x) + (b.y * b.y);
                float cSq = (c.x * c.x) + (c.y * c.y);
                float x = (
                    aSq * (b.y - c.y)
                    + bSq * (c.y - a.y)
                    + cSq * (a.y - b.y)
                    ) / d;
                float y = (
                    aSq * (c.x - b.x)
                    + bSq * (a.x - c.x)
                    + cSq * (b.x - a.x)
                    ) / d;
                Waypoint circumcenter = new Waypoint(new Vector2(x, y));
                return new Circle(
                    circumcenter,
                    (a - circumcenter.Position).Length()
                );
            }
        }
        public bool IsInCircumcircle(Waypoint waypoint) {
            Circle circumcircle = this.Circumcircle;
            return (circumcircle.Center.Position - waypoint.Position).Length() < circumcircle.Radius;
        }
    }

    public readonly struct Circle {
        public readonly Waypoint Center;
        public readonly float Radius;
        public Circle(Waypoint center, float radius) {
            Center = center;
            Radius = radius;
        }
    }
}