using UnityEngine;

public class InstancedMaterialProperties : MonoBehaviour {

	static MaterialPropertyBlock propertyBlock;

	static int colorId = Shader.PropertyToID("_BaseColor");
	static int cutoffId = Shader.PropertyToID("_Cutoff");
	static int metallicId = Shader.PropertyToID("_Metallic");
	static int smoothnessId = Shader.PropertyToID("_Smoothness");
	static int emissionColorId = Shader.PropertyToID("_EmissionColor");

	[SerializeField]
	Color color = Color.white;

	[SerializeField, Range(0f, 1f)]
	float alphaCutoff = 0.5f, metallic = 0f, smoothness = 0.5f;

	[SerializeField, ColorUsage(false, true)]
	Color emissionColor = Color.black;

	void Awake () {
		OnValidate();
	}

	void OnValidate () {
		if (propertyBlock == null) {
			propertyBlock = new MaterialPropertyBlock();
		}
		propertyBlock.SetColor(colorId, color);
		propertyBlock.SetFloat(metallicId, metallic);
		propertyBlock.SetFloat(smoothnessId, smoothness);
		propertyBlock.SetFloat(cutoffId, alphaCutoff);
		propertyBlock.SetColor(emissionColorId, emissionColor);
		GetComponent<MeshRenderer>().SetPropertyBlock(propertyBlock);
	}
}