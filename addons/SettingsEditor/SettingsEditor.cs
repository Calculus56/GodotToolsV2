#if TOOLS
using Godot;
using System;

// Created whenever you create an editor plugin.
[Tool]
public partial class SettingsEditor : EditorPlugin
{
	public override void _EnterTree()
	{
		// Initialization of the plugin goes here.
	}

	public override void _ExitTree()
	{
		// Clean-up of the plugin goes here.
	}
}
#endif
