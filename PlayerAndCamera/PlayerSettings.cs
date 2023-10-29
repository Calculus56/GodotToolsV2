
using System;
using System.Linq;
using Godot;

// Most likely will need it as a global variable. Doesn't work if you attach it to a Node. 
// Or you can just organzie the player script and copy and paste the code.
// Converted to C# by hand, GDScript source at http://kehomsforge.com/tutorials/single/gdConditionalProperty

// Using GlobalClass will add the resource class to the resource registry.
[Tool, GlobalClass]
public partial class PlayerSettings : Resource
{
	public enum InputType{
		Keyboard,
		Controller,
	}
	// SideScroller means adding gravity.
	public enum GameType{
		TopDown,
		SideScroller,
	}

	// Velocty Type means changing the velocities to move the character.
	// Character Type means rotating the character around it's axis, but keeping
	// really only using the X velocity.
	public enum RotationType{
		Velocity,
		Character,
	}
	// When exporting variables you need to click build in Godot.
	//[Export]
	public InputType inputType {get;set;}
	public GameType gameType {get;set;}
	// public GameType gameType {
	// 	get => tempGameType;
	// 	set{
	// 		tempGameType = value;
	// 		// When switching GameTypes, property list will be called.
	// 		NotifyPropertyListChanged();
	// 	}
	// }
	public GameType tempGameType { get; set; }

	public RotationType rotationType { get; set; }

	PackedScene editorAddon = GD.Load<PackedScene>("res://PlayerAndCamera/EditorPlugin.tscn");
	// A try at EdiotrPlugins see notes for more.
	//#region
    // Control dockedScene;

	// // Turning Global Variables on and off will reload the script.
    // public override void _EnterTree()
    // {
    //     dockedScene = editorAddon.Instantiate() as Control;
	// 	AddControlToDock(DockSlot.LeftUr, dockedScene);
    // }

    // public override void _ExitTree()
    // {
	// 	RemoveControlFromDocks(dockedScene);
    //     dockedScene.QueueFree();
    // }
	//#endregion

    public override Godot.Collections.Array<Godot.Collections.Dictionary> _GetPropertyList()
    {
        // By default, `rotationType` is not visible in the editor.
        //var propertyUsage = PropertyUsageFlags.NoEditor;
        var propertyUsage = PropertyUsageFlags.Default;
		

        //if (gameType == GameType.TopDown)
        //{
        //    propertyUsage = PropertyUsageFlags.Default;
        //}

        var properties = new Godot.Collections.Array<Godot.Collections.Dictionary>();
		properties.Add(new Godot.Collections.Dictionary()
        {
            { "name", "Settings/GameType" },
            { "type", (int)Variant.Type.Int},
            { "usage", (int)propertyUsage },
			{ "hint", (int)PropertyHint.Enum },
            { "hint_string", "TopDown,SideScoller" }
        });
		if(gameType == GameType.TopDown){
			properties.Add(new Godot.Collections.Dictionary()
			{
				{ "name", "Settings/RotationType" },
				{ "type", (int)Variant.Type.Int},
				{ "usage", (int)propertyUsage },
				{ "hint", (int)PropertyHint.Enum },
				{ "hint_string", "Velocity,Character" }
			});
		}
		properties.Add(new Godot.Collections.Dictionary()
			{
				{ "name", "Input/InputType" },
				{ "type", (int)Variant.Type.Int},
				{ "usage", (int)propertyUsage },
				{ "hint", (int)PropertyHint.Enum },
				{ "hint_string", "Keyboard,Controller" }
			});


        return properties;
    }

	
	// _Get is the new _Process for tools.

	// The value argument must be a variant, which we can't explicitly tell through static typing.
	// This function must return true if the property actually exists.
    public override bool _Set(StringName prop_name, Variant value)
    {
		string name = SeperateGroups(prop_name);
		// Too make the path scalable, you should 
		bool retval = true;
		switch(name){
			case "GameType":
				// Need to cast the Variant to int, then cast Rotation type so it could get the appropiate enum.
				gameType = (GameType)(int)value;
				NotifyPropertyListChanged();
				break;
			case "RotationType":
				rotationType = (RotationType)(int)value;
				break;
			case "InputType":
				inputType = (InputType)(int)value;
				break;
			default:
				retval = false;
				break;
		}
		return retval;
    }

	// This function must return a value, which is basically the one related to the property name.
	// However it is a variant, which we can't define explicitly through static typing.
    // The _Get function is constantly called.
	// Using it would mean the script disappears.
	public override Variant _Get(StringName prop_name)
    {
		string name = SeperateGroups(prop_name);
		switch(name){
			case "GameType":
				return (int)gameType;
			case "RotationType":
				return (int)rotationType;
			case "InputType":
				return (int)inputType;
		}
		return new Variant();
    }

	string SeperateGroups(string path){
		// Splits the path into groups, then returns the last group which is the name of the property.
		return path.Split('/').Last();
	}
}
