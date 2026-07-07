using System;
using System.Collections.Generic;
using System.Linq;

public static class EnumUtils
{
	public static T[] GetValues<T>() where T : Enum
	{
		return (T[])Enum.GetValues(typeof(T));
	}

	public static void GetRange<T>(out bool isContiguous, out int minValueInclusive, out int maxValueInclusive) where T : Enum
	{
		IEnumerable<int> source = GetValues<T>().Cast<int>();
		minValueInclusive = source.Min();
		maxValueInclusive = source.Max();
		isContiguous = true;
		for (int i = minValueInclusive; i <= maxValueInclusive; i++)
		{
			if (!Enum.IsDefined(typeof(T), i))
			{
				isContiguous = false;
				break;
			}
		}
	}

	public static ReadOnlySpan<char> GetDisplayName<T>(T value, EnumWordSeparationMode wordSeparationMode = EnumWordSeparationMode.SpaceBeforeUpperCaseFollowedByNonUpperCase, EnumWordCasing wordCasing = EnumWordCasing.LowerCaseExceptFirstLetterInFirstWordAndAllCapsWords) where T : Enum
	{
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		char[] array = null;
		int num = 0;
		string text = value.ToString();
		switch (wordSeparationMode)
		{
		case EnumWordSeparationMode.None:
			array = new char[text.Length];
			text.CopyTo(0, array, 0, text.Length);
			num = text.Length;
			break;
		case EnumWordSeparationMode.SpaceBeforeUpperCaseFollowedByNonUpperCase:
		{
			array = new char[text.Length + text.Length / 2];
			for (int j = 0; j < text.Length; j++)
			{
				if (j != 0 && !char.IsLower(text[j]) && (char.IsLower(text[j - 1]) || (j + 1 < text.Length && char.IsLower(text[j + 1]))))
				{
					array[num++] = ' ';
				}
				array[num++] = text[j];
			}
			break;
		}
		case EnumWordSeparationMode.SpaceBeforeUpperCase:
		{
			array = new char[text.Length * 2 - 1];
			for (int i = 0; i < text.Length; i++)
			{
				if (i != 0 && !char.IsLower(text[i]))
				{
					array[num++] = ' ';
				}
				array[num++] = text[i];
			}
			break;
		}
		default:
			throw new NotImplementedException();
		}
		int num2;
		int num3;
		int num4;
		bool flag;
		switch (wordCasing)
		{
		case EnumWordCasing.LowerCase:
			num2 = 0;
			goto IL_014e;
		case EnumWordCasing.LowerCaseExceptFirstLetterInFirstWord:
			num2 = 1;
			goto IL_014e;
		case EnumWordCasing.LowerCaseExceptAllCapsWords:
			num3 = 0;
			goto IL_0174;
		case EnumWordCasing.LowerCaseExceptFirstLetterInFirstWordAndAllCapsWords:
			num3 = 1;
			goto IL_0174;
		case EnumWordCasing.UpperCase:
		{
			for (int k = 0; k < num; k++)
			{
				array[k] = char.ToUpper(array[k]);
			}
			break;
		}
		default:
			throw new NotImplementedException();
		case EnumWordCasing.Keep:
			break;
			IL_0174:
			num4 = num3;
			flag = true;
			for (int l = num4; l < num; l++)
			{
				if (char.IsWhiteSpace(array[l]))
				{
					flag = true;
					num4 = l + 1;
				}
				else if (flag && !char.IsUpper(array[l]))
				{
					flag = false;
					array[num4] = char.ToLower(array[num4]);
				}
			}
			break;
			IL_014e:
			for (int m = num2; m < num; m++)
			{
				array[m] = char.ToLower(array[m]);
			}
			break;
		}
		return new ReadOnlySpan<char>(array, 0, num);
	}
}
