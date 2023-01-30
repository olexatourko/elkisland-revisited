// Godot 4 port of https://github.com/SIsilicon/Godot-God-Rays-Plugin

shader_type spatial;
render_mode unshaded, cull_disabled, skip_vertex_transform, blend_add;

uniform sampler2D DEPTH_TEXTURE : hint_depth_texture, filter_linear_mipmap;
uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;

uniform vec4 light_color: source_color;
uniform vec3 light_pos;
uniform float size = 1.0;
uniform int light_type = 0; //0 = directional, 1 = omni

uniform float exposure: hint_range(0,2);
uniform float attenuate = 1.0;

uniform int num_samples = 100;
uniform float dither = 1.0;
uniform bool use_pcf5 = true;

const float HALF_PI = PI/2.0;

varying mat4 inv_project_mat;
varying vec4 light_screen_pos;
varying vec3 camera_pos;
varying float far_plane;
varying float attenuate_size;

void vertex() {
	// Convert light position from world space to view space
	light_screen_pos = VIEW_MATRIX * vec4(light_pos, float(light_type));	
	// Convert light position from view space to clip space
	light_screen_pos = PROJECTION_MATRIX * light_screen_pos;
	// Perspective divisiontg
	
	light_screen_pos.xyz /= light_screen_pos.w;
	// Adjust XY to be at bottom-left of screen
	// https://learnopengl.com/img/getting-started/coordinate_systems.png
	light_screen_pos.xy = light_screen_pos.xy * 0.5 + 0.5;
	light_screen_pos.z = -(VIEW_MATRIX * vec4(light_pos, float(light_type))).z;
		
	// Fullscreen quad
	POSITION = vec4(VERTEX, 1.0);
	
	// Set up other variables that will be used in the fragment shader
	camera_pos = INV_VIEW_MATRIX[3].xyz; // In world space, I assume
	inv_project_mat = INV_PROJECTION_MATRIX;
	vec4 _far_plane = (INV_PROJECTION_MATRIX * vec4(0,0,1,1));
	far_plane = -_far_plane.z / _far_plane.w;
	attenuate_size = 1.0 / length(light_screen_pos) * size;
}

// Returns a normalized ray in world space
vec3 uv_to_ray(vec2 uv, mat4 view_matrix) {
	// Clip space to view space
	vec4 view_space_ray = inv_project_mat * vec4(uv * 2.0 - 1.0, 0, 1);
	view_space_ray.xyz /= view_space_ray.w;
	view_space_ray.w = 0.0; // w-component is only a thing in clip space
	// Normalize ray
	view_space_ray = normalize(view_space_ray);
	// Seems to be the equivelant of multpliying by the inverse view matrix,
	// converting from view space to world space
	return (view_space_ray * view_matrix).xyz;
}

// Returns the contents of the depth buffer (tex) for some screen UV
float get_depth(sampler2D tex, vec2 uv) {
	float depth = clamp(uv, 0.0, 1.0) == uv ? texture(tex, uv).r : 1.0;
	// Convert from clip space to view space
	// TODO: Use a preprocessor directive to use appropriate NDC for OpenGL and Vulkan
	// This tutorial assumes the use of the Vulkan renderer, which uses NDCs with a Z-range
	// https://godotengine.org/article/status-of-opengl-renderer/
	vec4 upos = inv_project_mat * vec4(uv * 2.0 - 1.0, depth, 1.0); // Vulkan
	// vec4 upos = inv_project_mat * vec4(uv * 2.0 - 1.0, depth * 2.0 - 1.0, 1.0); // OpenGL
	return -upos.z / upos.w;
}

/*
depth: depth buffer texture
ray_o: ray origin in world space
ray_d: ray desitination in world space
uv: screen space pixel
tex_size: ?
*/
float get_luminance(sampler2D depth, vec3 ray_o, vec3 ray_d, vec2 uv, vec2 tex_size) {
	float light_depth = mix(light_screen_pos.z, far_plane, float(1-light_type));
	float is_obstacle = float(get_depth(depth, uv) < light_depth);
	
	if(use_pcf5) {
		is_obstacle += float(get_depth(depth, uv + tex_size*vec2(-1,0)) < light_depth);
		is_obstacle += float(get_depth(depth, uv + tex_size*vec2(1,0)) < light_depth);
		is_obstacle += float(get_depth(depth, uv + tex_size*vec2(0,1)) < light_depth);
		is_obstacle += float(get_depth(depth, uv + tex_size*vec2(0,-1)) < light_depth);
		is_obstacle /= 5.0;
	}
	
	float luminance = 0.0;
	if(light_type == 0) {
		luminance = smoothstep(
			1.0,
			0.0,
			acos(dot(light_pos, ray_d)) / HALF_PI / size
		);
	} else if(light_type == 1) {
		luminance = smoothstep(
			1.0,
			0.0,
			// Angle between the camera->light vector and pixel->light vector
			acos(dot(normalize(light_pos - ray_o), ray_d)) / HALF_PI / attenuate_size 
		);
	}
	
	luminance *= 1.0 - is_obstacle;
	
	return max(luminance, 0.0);
}

float variable_smoothstep(float x, float N) {
	if(N > 0.0) {
		return pow(x, N);
	} else if(N < 0.0) {
		if(x <= 0.5) {
			return pow(2.0*x, -N) / 2.0;
		} else {
			return 1.0 - pow(2.0*(1.0-x), -N) / 2.0;
		}
	}
	
	return 0.0;
}

uint hash(uint x) {
	x = ((x >> uint(16)) ^ x) * uint(73244475);
	x = ((x >> uint(16)) ^ x) * uint(73244475);
	x = (x >> uint(16)) ^ x;
	return x;
}

float rand_from_seed(inout uint seed) {
	int k;
	int s = int(seed);
	if (s == 0)
	s = 305420679;
	k = s / 127773;
	s = 16807 * (s - k * 127773) - 2836 * k;
	if (s < 0)
		s += 2147483647;
	seed = uint(s);
	return float(seed % uint(65536)) / 65535.0;
}

void fragment() {
	if (length(light_color) <= 0.001)
		discard;
	
	vec2 pixel_size = 1.0 / vec2(textureSize(SCREEN_TEXTURE, 0));
	vec2 screen_uv = SCREEN_UV;
	// How far this pixel is from the light source (in screen cordinates)
	// Divided by n_samples because delta_uv screen_uv will increase by delta_uv for every sampling step
	vec2 delta_uv = (light_screen_pos.xy - screen_uv) / (float(num_samples));
	
	// Apply dithering: randomly offset SCREEN_UV a bit
	uint seed = hash(uint(FRAGCOORD.x + FRAGCOORD.y * float(textureSize(SCREEN_TEXTURE, 0).x)));
	screen_uv += delta_uv * dither * rand_from_seed(seed);
	
	float light = 0.0;
	for (int i = 0; i < num_samples; i++) {
		// Ray in world space
		vec3 ray = uv_to_ray(screen_uv, VIEW_MATRIX);
		float sample = get_luminance(DEPTH_TEXTURE, camera_pos, ray, screen_uv, pixel_size);
		
		light += sample;
		screen_uv += delta_uv;
	}
	light = light / float(num_samples);
	
	vec3 light_dir = light_type == 0 ? light_pos : normalize(light_pos - camera_pos);
	float facing_weight = dot(uv_to_ray(vec2(0.5), VIEW_MATRIX), light_dir);
	
	ALBEDO = variable_smoothstep(light, attenuate) * exposure * light_color.rgb * max(facing_weight * facing_weight, 0.0);
	
	// Debug
	//ALBEDO = vec3(1.0, 0.0, 0.0);
}