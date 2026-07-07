using UnityEngine;

public class ConditionalHideAttribute : PropertyAttribute
{
	public string ConditionalSourceField = "";

	public bool HideWhenTrue;

	public ConditionalHideAttribute(string conditionalSourceField, bool hideWhenTrue = false)
	{
		ConditionalSourceField = conditionalSourceField;
		HideWhenTrue = hideWhenTrue;
	}
}
