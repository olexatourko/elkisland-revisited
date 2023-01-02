using Godot;
using System;
using System.Collections.Generic;
// using System.Linq;

public partial class WaypointPlanner : Node
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Create a list of verts from all the trees
		List<Vector2> treePositions = new List<Vector2>();
		Node treesNode = GetTree().Root.GetNode("root/Trees");
		foreach (var child in treesNode.GetChildren()) {
			if (child is Node3D) {
				Vector3 position3d = (child as Node3D).GlobalPosition;
				Vector2 position2d = new Vector2(position3d.x, position3d.z);
				treePositions.Add(position2d);
			}
		}

		// Triangulate
		List<Vector2> verts = new List<Vector2>();
		List<Triangle> tris = new List<Triangle>();
		var (a, b, c) = WaypointPlanner.FindSuperTriangle(treePositions);
		Triangle superTriangle = new Triangle(verts, a, b, c); // Vert indices
		tris.Add(superTriangle);

		foreach (var pos in treePositions) {
			// Invalidate triangles
			HashSet<Triangle> invalidTris = new HashSet<Triangle>();
			foreach (var tri in tris) {
				var (circlePos, circleRadius) = FindCircumcircle(tri.vertA, tri.vertB, tri.vertC);
				if (IsInCircumcircle(pos, circlePos, circleRadius)) {
					invalidTris.Add(tri);
				}
			}

			// Build hole polygon
			HashSet<Edge> polygon = new HashSet<Edge>();
			Dictionary<Edge, int> edgeCounts = new Dictionary<Edge, int>();
			foreach (var tri in invalidTris) {
				Edge[] triEdges = { tri.edgeA, tri.edgeB, tri.edgeC };
				foreach (var edge in triEdges) {
					if (!edgeCounts.ContainsKey(edge)) edgeCounts[edge] = 0;
					edgeCounts[edge] += 1;
					
				}
			}
			foreach (var (edge, count) in edgeCounts) {
				if (count == 1) polygon.Add(edge);
			}

			// Remove invalid triangles
			foreach (var tri in invalidTris) {
				tris.Remove(tri);
			}

			// Create new triangle from every edge and the current point
			verts.Add(pos);
			foreach (var edge in polygon) {
				Triangle tri = new Triangle(verts, edge.vertAIdx, edge.vertBIdx, verts.Count - 1);
				tris.Add(tri);
			}
		}

		HashSet<Edge> edges = new HashSet<Edge>();
		// edges.Add(superTriangle.edgeA);
		// edges.Add(superTriangle.edgeB);
		// edges.Add(superTriangle.edgeC);
		foreach (var tri in tris) {
			Edge[] triEdges = { tri.edgeA, tri.edgeB, tri.edgeC };
			foreach (var edge in triEdges) {
				// Ignore edges which are connected to the super-triangle
				if (edge.vertAIdx <= 2 || edge.vertBIdx <= 2) continue;
				edges.Add(edge);
			}
		}

		// Setup debug edge renderer
		ImmediateMesh immediateMesh = new ImmediateMesh();
		MeshInstance3D meshInstance3D = new MeshInstance3D();
		meshInstance3D.Mesh = immediateMesh;
		this.AddChild(meshInstance3D);
		StandardMaterial3D material = new StandardMaterial3D();
		material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
		material.VertexColorUseAsAlbedo = true;
		meshInstance3D.MaterialOverride = material;

		immediateMesh.ClearSurfaces();
		immediateMesh.SurfaceBegin(Mesh.PrimitiveType.Lines);
		foreach (var edge in edges) {
			immediateMesh.SurfaceSetColor(Colors.Red);
			immediateMesh.SurfaceAddVertex(new Vector3(edge.vertA.x, 1, edge.vertA.y));
			immediateMesh.SurfaceAddVertex(new Vector3(edge.vertB.x, 1, edge.vertB.y));
		}
		immediateMesh.SurfaceEnd();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public static (Vector2, float) FindCircumcircle(Vector2 a, Vector2 b, Vector2 c) {
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
		Vector2 circumcenter = new Vector2(x, y);
		float r = (a - circumcenter).Length();
		return (circumcenter, r);
	}

	public static (Vector2, Vector2, Vector2) FindSuperTriangle(IEnumerable<Vector2> verts) {
		List<Nayuki.Point> points = new List<Nayuki.Point>();
		foreach (var vert in verts) {
			points.Add(new Nayuki.Point(vert.x, vert.y));
		}

		Nayuki.Circle circle = Nayuki.SmallestEnclosingCircle.MakeCircle(points);
		Vector2 circlePos = new Vector2(
			(float) circle.c.x,
			(float) circle.c.y
		);

		Vector2 a = new Vector2(
			circlePos.x + ((float) circle.r * 2),
			circlePos.y
		);
		Vector2 b = (a - circlePos).Rotated(2 * (float) Math.PI / 3) + circlePos;
		Vector2 c = (a - circlePos).Rotated(4 * (float) Math.PI / 3) + circlePos;
		return (a, b, c);
	}

	public static bool IsInCircumcircle(Vector2 point, Vector2 circlePos, float circleRadius) {
		return (circlePos - point).Length() < circleRadius;
	}
}

public struct Triangle {
	public IList<Vector2> verts;
	public int vertAIdx;
	public int vertBIdx;
	public int vertCIdx;
	public Edge edgeA;
	public Edge edgeB;
	public Edge edgeC;
	public Vector2 vertA {
		get { return verts[vertAIdx]; }
	}
	public Vector2 vertB {
		get { return verts[vertBIdx]; }
	}
	public Vector2 vertC {
		get { return verts[vertCIdx]; }
	}

	public Triangle(IList<Vector2> verts, int a, int b, int c) {
		this.verts = verts;
		this.vertAIdx = a;
		this.vertBIdx = b;
		this.vertCIdx = c;
		this.edgeA = new Edge(verts, a, b);
		this.edgeB = new Edge(verts, b, c);
		this.edgeC = new Edge(verts, c, a);
	}
	public Triangle(IList<Vector2> verts, Vector2 a, Vector2 b, Vector2 c) {
		this.verts = verts;
		verts.Add(a);
		verts.Add(b);
		verts.Add(c);
		this.vertAIdx = verts.Count - 3;
		this.vertBIdx = verts.Count - 2;
		this.vertCIdx = verts.Count - 1;
		this.edgeA = new Edge(verts, vertAIdx, vertBIdx);
		this.edgeB = new Edge(verts, vertBIdx, vertCIdx);
		this.edgeC = new Edge(verts, vertCIdx, vertAIdx);
	}
}

public struct Edge {
	public IList<Vector2> verts;
	public int vertAIdx;
	public int vertBIdx;
	public Vector2 vertA {
		get { return verts[vertAIdx]; }
	}
	public Vector2 vertB {
		get { return verts[vertBIdx]; }
	}
	public Edge(IList<Vector2> verts, int a, int b) {
		this.verts = verts;
		// Ensure lower-indexed vert is always first
		if (a > b) {
			(b, a) = (a, b);
		}
		this.vertAIdx = a;
		this.vertBIdx = b;
	}
	public Edge(IList<Vector2> verts, Vector2 a, Vector2 b) {
		this.verts = verts;
		verts.Add(a);
		verts.Add(b);
		this.vertAIdx = verts.Count - 2;
		this.vertBIdx = verts.Count - 1;
	}
}

public struct Circle {
	public IList<Vector2> verts;
	public int posIdx;
	public float r;
	public Vector2 pos {
		get { return verts[posIdx]; }
	}
	public Circle(IList<Vector2> verts, int pos, float r) {
		this.verts = verts;
		this.posIdx = pos;
		this.r = r;
	}
	public Circle(IList<Vector2> verts, Vector2 pos, float r) {
		this.verts = verts;
		verts.Add(pos);
		this.posIdx = verts.Count - 1;
		this.r = r;
	}
}