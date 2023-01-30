shader_type spatial;
render_mode unshaded;

void fragment() {
	vec4 world_vert = INV_VIEW_MATRIX * vec4(VERTEX, 1.0);
	vec4 local_vert = inverse(MODEL_MATRIX) * world_vert;
	ALBEDO = local_vert.xyz;
}
