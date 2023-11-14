using Godot;
using System;
using System.Linq;

// Orginal source at https://ask.godotengine.org/32506/how-to-draw-a-curve-in-2d by Dlean Jeans
// Converted to C#
public partial class smooth_path : Path2D
{	
	[Export] public Curve2D curve;
	// If you put points to close together it will flicker, 
	// Most likely it's the edges being hit.
	[Export] int spline_length = 100;
	public float width = 10;
	bool smoothVal = true;
	[Export] bool _smooth {
		get => smoothVal;
		set{
			smoothVal = value;
			smooth(smoothVal);
		}
	}
	bool straightVal = true;
	[Export] bool _straighten{
		get => straightVal;
		set{
			straightVal = value;
			smooth(straightVal);
		}
	}

	void straighten(bool value){
		if(!value) return;
		for(int i = 0; i < curve.PointCount; i++){
			curve.SetPointIn(i, new Vector2());
			curve.SetPointOut(i, new Vector2());
		}
	}

	public void smooth(bool value){
		if(!value) return;

		var point_count = curve.PointCount;
		for(int i = 0; i < point_count; i++){
			var spline = _get_spline(i);
			curve.SetPointIn(i, -spline);
			curve.SetPointOut(i, spline);
		}
	}

	Vector2 _get_spline(int i){
		var last_point = _get_point(i - 1);
		var next_point = _get_point(i + 1);
		Vector2 spline = last_point.DirectionTo(next_point) * spline_length;
		return spline;
	}
	/// <summary>
	/// Mathf.Wrap
	/// if (x < x_min)
    /// 	x = x_max - (x_min - x) % (x_max - x_min);
	/// else
    /// 	x = x_min + (x - x_min) % (x_max - x_min);
	/// </summary>
	Vector2 _get_point(int i){
		var point_count = curve.PointCount;
		var point = Mathf.Wrap(i, 0, point_count);
		return curve.GetPointPosition(point);
	}

	public override void _Draw(){
		var points = curve.GetBakedPoints();
		//GD.Print("Points " + points.Count());
		//Color[] color = Enumerable.Repeat(Colors.Black, points.Length).ToArray();
		if(points != null){
			DrawPolyline(points, Colors.Aqua, width, true);
		}
	}
}
