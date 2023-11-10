using Godot;
using System;
using System.Linq;

public partial class RedrawPolygon : Polygon2D
{
	[Export] public Curve2D curve;

	// public override void _Draw(){
	// 	var points = curve.GetBakedPoints();
	// 	GD.Print(points.Count());
	// 	Color[] color = Enumerable.Repeat(Colors.Black, points.Length).ToArray();
	// 	if(points != null){
	// 		DrawPolygon(points, color);
	// 	}
	// }
}
