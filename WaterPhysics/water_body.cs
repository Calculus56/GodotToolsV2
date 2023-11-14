using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class water_body : Node2D
{
	[Export] float k = 0.015f, d = 0.03f, spread = 0.002f;
	[Export] int distanceBetweenSprings = 32, springNumber = 6, depth = 1000, startPos;
	[Export] PackedScene waterPoint;
	float targetHeight, bottom, border_thickness = 6f;
	Polygon2D water_polygon;
	smooth_path water_border;
	CollisionShape2D collisionShape;
	Area2D waterBodyArea;
	List<water_spring> springs = new List<water_spring>();

	List<float> leftDeltas = new List<float>();
	List<float> rightDeltas = new List<float>();

	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		water_polygon = GetNode<Polygon2D>("Water_Polygon");
		water_border = GetNode<smooth_path>("Water_Border");
		collisionShape = GetNode<CollisionShape2D>("waterBodyArea/CollisionShape2D");
		waterBodyArea = GetNode<Area2D>("waterBodyArea");
		
		startPos = distanceBetweenSprings * springNumber/2;
		targetHeight = GlobalPosition.Y;
		bottom = targetHeight + depth;
		water_border.width = border_thickness;

		foreach(int i in Enumerable.Range(0, springNumber)){
			var X = -startPos + distanceBetweenSprings * i;
			water_spring w = waterPoint.Instantiate() as water_spring;
			AddChild(w);
			springs.Add(w);
			w.Initialize(X, i);
			w.set_collsion_shape(distanceBetweenSprings);
			w.splash += Splash;
		}
		var total_length = distanceBetweenSprings * (springNumber - 1);
		var rectangle = new RectangleShape2D();

		var rect_position = new Vector2(0, depth / 2);
		var rect_extents = new Vector2(total_length, depth);

		waterBodyArea.Position = rect_position;
		rectangle.Size = rect_extents;
		collisionShape.Shape = rectangle;
		// Splash(springNumber/2, 20);
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
		new_border();
		draw_water_body();
    }

	void new_border(){
		var curve = new Curve2D();

		var surfacePoints = new List<Vector2>();
		
		for(int i = 0; i < springs.Count(); i++){
			surfacePoints.Add(springs[i].Position);
		}

		foreach(Vector2 surfacePoint in surfacePoints){
			curve.AddPoint(surfacePoint);
		}

		water_border.curve = curve;
		water_border.smooth(true);
		water_border.QueueRedraw();
		//water_polygon.QueueRedraw();
	}

	public void Splash(int index, float speed){
		if(index >= 0 && index < springs.Count()){
			//GD.Print("Splashed");
			springs[index].velocity += speed;
		}
	}

	void draw_water_body(){
		// When using 30 points.
		// water_border.curve.GetBakedPoints() generates about 500 points
		// springs.Count() only generates 30 points.
		//GD.Print(water_border.curve.GetBakedPoints().Length/6);
		var tempPoints = new List<Vector2>(water_border.curve.GetBakedPoints());
		var surfacePoints = new List<Vector2>();
		// Has to at least be 1. If max points go over 100 then polygon will start blinking
		// so the modifier should be between 6-8.
		var modifier = 10;
		for(int i = 0; i < tempPoints.Count / modifier; i++){
			try{
				surfacePoints.Add(tempPoints[i * modifier]);
			}catch(Exception e){
				//GD.Print(i * 10);s
			}
		}
		// foreach(int i in Enumerable.Range(0, springs.Count())){
		// 	surfacePoints.Add(springs[i].Position);
		// }

		var first_index = 0;
		var last_index = surfacePoints.Count()-1;

		var water_polygon_points = surfacePoints;
		
		water_polygon_points.Add(new Vector2(surfacePoints[last_index].X, bottom));
		water_polygon_points.Add(new Vector2(surfacePoints[first_index].X, bottom));

		Vector2[] water_Points = water_polygon_points.ToArray();
		water_polygon.Polygon = water_Points;
	}

	// If you want the collision to show you might want to make the object
	// local to the scene.
	void _on_water_body_area_body_entered(Node2D node){
		GD.Print(node);
	}
}
