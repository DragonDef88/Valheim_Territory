using System.Collections.Generic;
using UnityEngine;

public class PointGenerator
{
	private int m_amount;

	private float m_gridSize = 8f;

	private Vector2Int m_currentCenterGrid = new Vector2Int(99999, 99999);

	private int m_currentGridWith;

	private List<Vector3> m_points = new List<Vector3>();

	public PointGenerator(int amount, float gridSize)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		m_amount = amount;
		m_gridSize = gridSize;
	}

	public void Update(Vector3 center, float radius, List<Vector3> newPoints, List<Vector3> removedPoints)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		Vector2Int grid = GetGrid(center);
		if (m_currentCenterGrid == grid)
		{
			newPoints.Clear();
			removedPoints.Clear();
			return;
		}
		int num = Mathf.CeilToInt(radius / m_gridSize);
		if (m_currentCenterGrid != grid || m_currentGridWith != num)
		{
			RegeneratePoints(grid, num);
		}
	}

	private void RegeneratePoints(Vector2Int centerGrid, int gridWith)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		m_currentCenterGrid = centerGrid;
		State state = Random.state;
		m_points.Clear();
		Vector3 item = default(Vector3);
		for (int i = ((Vector2Int)(ref centerGrid)).y - gridWith; i <= ((Vector2Int)(ref centerGrid)).y + gridWith; i++)
		{
			for (int j = ((Vector2Int)(ref centerGrid)).x - gridWith; j <= ((Vector2Int)(ref centerGrid)).x + gridWith; j++)
			{
				Random.InitState(j + i * 100);
				Vector3 gridPos = GetGridPos(new Vector2Int(j, i));
				for (int k = 0; k < m_amount; k++)
				{
					((Vector3)(ref item))._002Ector(Random.Range(gridPos.x - m_gridSize, gridPos.x + m_gridSize), Random.Range(gridPos.z - m_gridSize, gridPos.z + m_gridSize));
					m_points.Add(item);
				}
			}
		}
		Random.state = state;
	}

	public Vector2Int GetGrid(Vector3 point)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		int num = Mathf.FloorToInt((point.x + m_gridSize / 2f) / m_gridSize);
		int num2 = Mathf.FloorToInt((point.z + m_gridSize / 2f) / m_gridSize);
		return new Vector2Int(num, num2);
	}

	public Vector3 GetGridPos(Vector2Int grid)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3((float)((Vector2Int)(ref grid)).x * m_gridSize, 0f, (float)((Vector2Int)(ref grid)).y * m_gridSize);
	}
}
