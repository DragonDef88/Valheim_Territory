using System;
using System.Collections.Generic;
using UnityEngine;

namespace STUWard;

internal sealed class ManagedWardIndex
{
	private sealed class SpatialWardEntry
	{
		internal PrivateArea Area { get; }

		internal int InstanceId { get; }

		internal SpatialWardEntry(PrivateArea area)
		{
			Area = area;
			InstanceId = ((Object)area).GetInstanceID();
		}
	}

	private const float SpatialCellSize = 32f;

	private readonly Func<PrivateArea, bool> _isTrackable;

	private readonly List<PrivateArea> _areas = new List<PrivateArea>();

	private readonly HashSet<int> _areaIds = new HashSet<int>();

	private readonly Dictionary<long, List<SpatialWardEntry>> _spatialIndex = new Dictionary<long, List<SpatialWardEntry>>();

	private readonly Dictionary<int, List<long>> _spatialCellsByInstanceId = new Dictionary<int, List<long>>();

	private readonly Dictionary<int, int> _queryStamps = new Dictionary<int, int>();

	private int _queryStamp;

	internal int Count => _areaIds.Count;

	internal IReadOnlyList<PrivateArea> Areas => _areas;

	internal ManagedWardIndex(Func<PrivateArea, bool> isTrackable)
	{
		_isTrackable = isTrackable;
	}

	internal bool Add(PrivateArea area)
	{
		int instanceID = ((Object)area).GetInstanceID();
		if (!_areaIds.Add(instanceID))
		{
			return false;
		}
		_areas.Add(area);
		return true;
	}

	internal bool Remove(PrivateArea area)
	{
		int instanceID = ((Object)area).GetInstanceID();
		if (!_areaIds.Remove(instanceID))
		{
			return false;
		}
		_areas.Remove(area);
		return true;
	}

	internal bool Contains(int instanceId)
	{
		return _areaIds.Contains(instanceId);
	}

	internal void Clear()
	{
		_areas.Clear();
		_areaIds.Clear();
		ClearSpatialIndex();
	}

	internal void ClearSpatialIndex()
	{
		_spatialIndex.Clear();
		_spatialCellsByInstanceId.Clear();
		_queryStamps.Clear();
		_queryStamp = 0;
	}

	internal void RebuildSpatialIndex()
	{
		ClearSpatialIndex();
		for (int i = 0; i < _areas.Count; i++)
		{
			PrivateArea val = _areas[i];
			if (_isTrackable(val))
			{
				AddAreaToSpatialIndex(val);
			}
		}
	}

	internal void UpdateSpatialIndex(PrivateArea area, int instanceId, bool shouldContain)
	{
		RemoveAreaFromSpatialIndex(instanceId);
		if (shouldContain && _isTrackable(area))
		{
			AddAreaToSpatialIndex(area);
		}
	}

	internal void FillCandidates(Vector3 point, float radius, List<PrivateArea> destination)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		destination.Clear();
		if (_spatialIndex.Count == 0)
		{
			return;
		}
		int num = NextQueryStamp();
		float num2 = Mathf.Max(0f, radius);
		int spatialCellCoordinate = GetSpatialCellCoordinate(point.x - num2);
		int spatialCellCoordinate2 = GetSpatialCellCoordinate(point.x + num2);
		int spatialCellCoordinate3 = GetSpatialCellCoordinate(point.z - num2);
		int spatialCellCoordinate4 = GetSpatialCellCoordinate(point.z + num2);
		for (int i = spatialCellCoordinate; i <= spatialCellCoordinate2; i++)
		{
			for (int j = spatialCellCoordinate3; j <= spatialCellCoordinate4; j++)
			{
				if (!_spatialIndex.TryGetValue(GetSpatialCellKey(i, j), out List<SpatialWardEntry> value))
				{
					continue;
				}
				for (int k = 0; k < value.Count; k++)
				{
					SpatialWardEntry spatialWardEntry = value[k];
					if (!_queryStamps.TryGetValue(spatialWardEntry.InstanceId, out var value2) || value2 != num)
					{
						_queryStamps[spatialWardEntry.InstanceId] = num;
						destination.Add(spatialWardEntry.Area);
					}
				}
			}
		}
	}

	private void AddAreaToSpatialIndex(PrivateArea area)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)area).transform.position;
		float radius = WardSettings.GetRadius(area);
		SpatialWardEntry spatialWardEntry = new SpatialWardEntry(area);
		List<long> list = new List<long>();
		int spatialCellCoordinate = GetSpatialCellCoordinate(position.x - radius);
		int spatialCellCoordinate2 = GetSpatialCellCoordinate(position.x + radius);
		int spatialCellCoordinate3 = GetSpatialCellCoordinate(position.z - radius);
		int spatialCellCoordinate4 = GetSpatialCellCoordinate(position.z + radius);
		for (int i = spatialCellCoordinate; i <= spatialCellCoordinate2; i++)
		{
			for (int j = spatialCellCoordinate3; j <= spatialCellCoordinate4; j++)
			{
				long spatialCellKey = GetSpatialCellKey(i, j);
				if (!_spatialIndex.TryGetValue(spatialCellKey, out List<SpatialWardEntry> value))
				{
					value = new List<SpatialWardEntry>();
					_spatialIndex[spatialCellKey] = value;
				}
				value.Add(spatialWardEntry);
				list.Add(spatialCellKey);
			}
		}
		_spatialCellsByInstanceId[spatialWardEntry.InstanceId] = list;
	}

	private void RemoveAreaFromSpatialIndex(int instanceId)
	{
		if (!_spatialCellsByInstanceId.TryGetValue(instanceId, out List<long> value))
		{
			return;
		}
		for (int i = 0; i < value.Count; i++)
		{
			long key = value[i];
			if (!_spatialIndex.TryGetValue(key, out List<SpatialWardEntry> value2))
			{
				continue;
			}
			for (int num = value2.Count - 1; num >= 0; num--)
			{
				if (value2[num].InstanceId == instanceId)
				{
					value2.RemoveAt(num);
				}
			}
			if (value2.Count == 0)
			{
				_spatialIndex.Remove(key);
			}
		}
		_spatialCellsByInstanceId.Remove(instanceId);
	}

	private static int GetSpatialCellCoordinate(float coordinate)
	{
		return Mathf.FloorToInt(coordinate / 32f);
	}

	private static long GetSpatialCellKey(int cellX, int cellZ)
	{
		return ((long)cellX << 32) ^ (uint)cellZ;
	}

	private int NextQueryStamp()
	{
		if (_queryStamp == int.MaxValue)
		{
			_queryStamp = 1;
			_queryStamps.Clear();
			return _queryStamp;
		}
		_queryStamp++;
		return _queryStamp;
	}
}
