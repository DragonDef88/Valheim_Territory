using UnityEngine;

public class ItemStyle : MonoBehaviour, IEquipmentVisual
{
	public void Setup(int style)
	{
		MaterialMan.instance.SetValue(((Component)this).gameObject, ShaderProps._Style, style);
	}
}
