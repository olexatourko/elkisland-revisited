using Godot;
using System;

[Tool] // https://docs.godotengine.org/en/latest/tutorials/plugins/running_code_in_the_editor.html
public partial class VolumetricScatterLight : MeshInstance3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {

	}

	
	public override void _Process(double delta) {
		Material material = this.GetActiveMaterial(0);
		if (material is ShaderMaterial) {
			(material as ShaderMaterial).SetShaderParameter("light_position", this.GlobalPosition);
		}
	}
}
