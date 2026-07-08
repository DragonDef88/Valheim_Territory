using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace Guilds;

public static class ObjectDiff
{
	private static readonly MethodInfo hashSetDiff = AccessTools.DeclaredMethod(typeof(ObjectDiff), "diffHashSets", (Type[])null, (Type[])null);

	public static Dictionary<string[], object?> diff<T>(T old, T cur) where T : notnull
	{
		Dictionary<string[], object> dictionary = new Dictionary<string[], object>();
		diff(old, cur, typeof(T), new List<string>(), dictionary);
		return dictionary;
	}

	private static void diffHashSets<T>(List<string> path, Dictionary<string[], object?> differences, HashSet<T> a, HashSet<T> b) where T : struct
	{
		HashSet<T> b2 = b;
		HashSet<T> a2 = a;
		foreach (T item in a2.Where((T v) => !b2.Contains(v)))
		{
			path.Add(item.ToString());
			path.Add("0");
			differences.Add(path.ToArray(), item);
			path.RemoveAt(path.Count - 1);
			path.RemoveAt(path.Count - 1);
		}
		foreach (T item2 in b2.Where((T v) => !a2.Contains(v)))
		{
			path.Add(item2.ToString());
			path.Add("1");
			differences.Add(path.ToArray(), item2);
			path.RemoveAt(path.Count - 1);
			path.RemoveAt(path.Count - 1);
		}
	}

	private static void diff(object old, object cur, Type t, List<string> path, Dictionary<string[], object?> differences)
	{
		FieldInfo[] fields = t.GetFields(BindingFlags.Instance | BindingFlags.Public);
		foreach (FieldInfo fieldInfo in fields)
		{
			object value = fieldInfo.GetValue(old);
			object value2 = fieldInfo.GetValue(cur);
			if (fieldInfo.FieldType == typeof(string) || fieldInfo.FieldType.IsPrimitive || fieldInfo.FieldType.IsEnum)
			{
				if (!value.Equals(value2))
				{
					path.Add(fieldInfo.Name);
					differences.Add(path.ToArray(), value2);
					path.RemoveAt(path.Count - 1);
				}
			}
			else if (fieldInfo.FieldType.IsGenericType && typeof(HashSet<>) == fieldInfo.FieldType.GetGenericTypeDefinition())
			{
				Type type = fieldInfo.FieldType.GetGenericArguments()[0];
				path.Add(fieldInfo.Name);
				hashSetDiff.MakeGenericMethod(type).Invoke(null, new object[4] { path, differences, value, value2 });
				path.RemoveAt(path.Count - 1);
			}
			else if (typeof(IDictionary).IsAssignableFrom(fieldInfo.FieldType))
			{
				Type t2 = fieldInfo.FieldType.GetGenericArguments()[1];
				path.Add(fieldInfo.Name);
				foreach (object item in ((IDictionary)value).Keys.Cast<object>().Except(((IDictionary)value2).Keys.Cast<object>()))
				{
					path.Add(item.ToString());
					differences.Add(path.ToArray(), null);
					path.RemoveAt(path.Count - 1);
				}
				foreach (object key in ((IDictionary)value2).Keys)
				{
					path.Add(key.ToString());
					if (((IDictionary)value).Contains(key))
					{
						diff(((IDictionary)value)[key], ((IDictionary)value2)[key], t2, path, differences);
					}
					else
					{
						differences.Add(path.ToArray(), ((IDictionary)value2)[key]);
					}
					path.RemoveAt(path.Count - 1);
				}
				path.RemoveAt(path.Count - 1);
			}
			else
			{
				path.Add(fieldInfo.Name);
				diff(value, value2, fieldInfo.FieldType, path, differences);
				path.RemoveAt(path.Count - 1);
			}
		}
	}

	public static void ApplyDiff<T>(ref T baseTarget, Dictionary<string[], object?> differences) where T : notnull
	{
		foreach (KeyValuePair<string[], object> difference in differences)
		{
			List<FieldInfo> list = new List<FieldInfo>();
			Type type = typeof(T);
			object obj = baseTarget;
			object obj2 = null;
			object key = null;
			for (int i = 0; i < difference.Key.Length; i++)
			{
				FieldInfo field = type.GetField(difference.Key[i]);
				if (i == difference.Key.Length - 1)
				{
					if (list.Count == 0)
					{
						field.SetValue(obj, difference.Value);
						break;
					}
					TypedReference obj3 = TypedReference.MakeTypedReference(obj, list.ToArray());
					field.SetValueDirect(obj3, difference.Value);
					break;
				}
				if (field.FieldType.IsGenericType && typeof(HashSet<>) == field.FieldType.GetGenericTypeDefinition())
				{
					Type type2 = field.FieldType.GetGenericArguments()[0];
					string text = difference.Key[i + 2];
					typeof(HashSet<>).MakeGenericType(type2).GetMethod((text == "1") ? "Add" : "Remove").Invoke(field.GetValue(obj), new object[1] { difference.Value });
					break;
				}
				if (typeof(IDictionary).IsAssignableFrom(field.FieldType))
				{
					Type type3 = field.FieldType.GetGenericArguments()[0];
					object obj4 = difference.Key[++i];
					if (type3 != typeof(string))
					{
						obj4 = TypeDescriptor.GetConverter(type3).ConvertFromString((string)obj4) ?? "";
					}
					IDictionary dictionary;
					if (list.Count == 0)
					{
						dictionary = (IDictionary)field.GetValue(obj);
					}
					else
					{
						TypedReference obj5 = TypedReference.MakeTypedReference(obj, list.ToArray());
						dictionary = (IDictionary)field.GetValueDirect(obj5);
					}
					if (difference.Key.Length == i + 1)
					{
						if (difference.Value == null)
						{
							dictionary.Remove(obj4);
						}
						else
						{
							dictionary[obj4] = difference.Value;
						}
						continue;
					}
					if (!dictionary.Contains(obj4))
					{
						break;
					}
					obj = dictionary[obj4];
					key = obj4;
					obj2 = dictionary;
					type = field.FieldType.GetGenericArguments()[1];
					list.Clear();
				}
				else if (field.FieldType.IsValueType)
				{
					list.Add(field);
					type = field.FieldType;
				}
				else
				{
					if (list.Count == 0)
					{
						obj = field.GetValue(obj);
					}
					else
					{
						TypedReference obj6 = TypedReference.MakeTypedReference(obj, list.ToArray());
						obj = field.GetValueDirect(obj6);
					}
					type = field.FieldType;
					list.Clear();
					obj2 = obj;
				}
			}
			if (obj2 == null)
			{
				baseTarget = (T)obj;
			}
			else if (obj2 is IDictionary dictionary2)
			{
				dictionary2[key] = obj;
			}
		}
	}
}
