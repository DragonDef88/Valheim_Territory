using UnityEngine;

public static class ShaderProps
{
	public static readonly int _MainTex = Shader.PropertyToID("_MainTex");

	public static readonly int _Color = Shader.PropertyToID("_Color");

	public static readonly int _EmissionColor = Shader.PropertyToID("_EmissionColor");

	public static readonly int _Style = Shader.PropertyToID("_Style");

	public static readonly int _RippleDistance = Shader.PropertyToID("_RippleDistance");

	public static readonly int _ValueNoise = Shader.PropertyToID("_ValueNoise");
}
