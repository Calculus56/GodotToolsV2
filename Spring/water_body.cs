using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class water_body : Node2D
{
	[Export] float k = 0.015f, d = 0.03f, spread = 0.002f;
	[Export] int distanceBetweenSprings = 32, springNumber = 6, depth = 1000, startPos;
	[Export] PackedScene waterPoint;
	float target_height, bottom;
	Polygon2D water_polygon;
	List<water_spring> springs = new List<water_spring>();

	List<float> leftDeltas = new List<float>();
	List<float> rightDeltas = new List<float>();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		startPos = distanceBetweenSprings * springNumber/2;
		target_height = GlobalPosition.Y;
		bottom = target_height + depth;
		water_polygon = GetNode<Polygon2D>("Water_Polygon");
		foreach(int i in Enumerable.Range(0, springNumber)){
			var X = -startPos + distanceBetweenSprings * i;
			water_spring w = waterPoint.Instantiate() as water_spring;
			AddChild(w);
			springs.Add(w);
			w.Initialize(X);
		}
		GD.Print(springs.Count());
		Splash(5, 10);
	}

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(double delta)
    {
        foreach(water_spring i in springs){
			i.WaterUpdate(k, d);
		}

		// Initialize the values with an array of zeros.
		foreach(int i in Enumerable.Range(0, springs.Count())){
			leftDeltas.Add(0);
			rightDeltas.Add(0);
		}

		foreach(int i in Enumerable.Range(0, springs.Count())){
			if(i > 0){
				leftDeltas[i] = spread * (springs[i].height - springs[i - 1].height);
				springs[i - 1].velocity += leftDeltas[i];
			}
			if(i < springs.Count() - 1){
				rightDeltas[i] = spread * (springs[i].height - springs[i + 1].height);
				springs[i + 1].velocity += rightDeltas[i];
			}
		}
		draw_water_body();
    }

	void Splash(int index, float speed){
		if(index >= 0 && index < springs.Count()){
			GD.Print("Splashed");
			springs[index].velocity += speed;
		}
	}

	void draw_water_body(){
		var surfacePoints = new List<Vector2>();
		foreach(int i in Enumerable.Range(0, springs.Count())){
			surfacePoints.Add(springs[i].Position);
		}

		var first_index = 0;
		var last_index = surfacePoints.Count()-1;

		var water_polygon_points = surfacePoints;

		water_polygon_points.Add(new Vector2(surfacePoints[last_index].X, bottom));
		water_polygon_points.Add(new Vector2(surfacePoints[first_index].X, bottom));

		Vector2[] water_Points = water_polygon_points.ToArray();
		water_polygon.Polygon = water_Points;
	}
}
