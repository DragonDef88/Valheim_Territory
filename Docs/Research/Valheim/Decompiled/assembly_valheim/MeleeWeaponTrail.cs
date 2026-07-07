using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeleeWeaponTrail : MonoBehaviour, IMonoUpdater
{
	[Serializable]
	public class Point
	{
		public float endTime;

		public Vector3 basePosition;

		public Vector3 tipPosition;
	}

	[SerializeField]
	private bool _emit = true;

	[SerializeField]
	private float _emitTime;

	[SerializeField]
	private Material _material;

	[SerializeField]
	private float _lifeTime = 1f;

	[SerializeField]
	private Color[] _colors;

	[SerializeField]
	private float[] _sizes;

	[SerializeField]
	private float _minVertexDistance = 0.1f;

	[SerializeField]
	private float _maxVertexDistance = 10f;

	private readonly float m_minVertexDistanceSqr;

	private readonly float m_maxVertexDistanceSqr;

	[SerializeField]
	private float _maxAngle = 3f;

	[SerializeField]
	private bool _autoDestruct;

	[SerializeField]
	private int subdivisions = 4;

	[SerializeField]
	private Transform _base;

	[SerializeField]
	private Transform _tip;

	private readonly List<Point> m_points = new List<Point>();

	private readonly List<Point> m_smoothedPoints = new List<Point>();

	private const int c_PointsPoolSize = 160;

	private const int c_PointsPoolSizeGrow = 32;

	private readonly List<Point> m_pointsPool = new List<Point>(160);

	private int m_pointsPoolIndex;

	private GameObject m_trailObject;

	private Mesh m_trailMesh;

	private Vector3 m_lastPosition;

	private readonly List<Vector3> m_newVertices = new List<Vector3>(64);

	private readonly List<Vector2> m_newUV = new List<Vector2>(64);

	private readonly List<Color> m_newColors = new List<Color>(64);

	private readonly List<int> m_newTriangles = new List<int>(64);

	private List<Vector3> m_smoothTipList = new List<Vector3>();

	private List<Vector3> m_smoothBaseList = new List<Vector3>();

	private readonly Vector3[] m_tipPoints = (Vector3[])(object)new Vector3[4];

	private readonly Vector3[] m_basePoints = (Vector3[])(object)new Vector3[4];

	public bool Emit
	{
		set
		{
			_emit = value;
			m_pointsPoolIndex = 0;
		}
	}

	public bool Use { get; set; }

	public static List<IMonoUpdater> Instances { get; } = new List<IMonoUpdater>();


	private Point GetPooledPoint()
	{
		if (m_pointsPoolIndex >= m_pointsPool.Count)
		{
			m_pointsPool.Capacity += 32;
			for (int i = 0; i < 32; i++)
			{
				m_pointsPool.Add(new Point());
			}
		}
		return m_pointsPool[m_pointsPoolIndex++];
	}

	private MeleeWeaponTrail()
	{
		m_minVertexDistanceSqr = _minVertexDistance * _minVertexDistance;
		m_maxVertexDistanceSqr = _maxVertexDistance * _maxVertexDistance;
	}

	private void Start()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Expected O, but got Unknown
		m_lastPosition = ((Component)this).transform.position;
		m_trailObject = new GameObject("Trail");
		m_trailObject.transform.parent = null;
		m_trailObject.transform.position = Vector3.zero;
		m_trailObject.transform.rotation = Quaternion.identity;
		m_trailObject.transform.localScale = Vector3.one;
		m_trailObject.AddComponent(typeof(MeshFilter));
		m_trailObject.AddComponent(typeof(MeshRenderer));
		m_trailObject.GetComponent<Renderer>().material = _material;
		m_trailMesh = new Mesh();
		((Object)m_trailMesh).name = ((Object)this).name + "TrailMesh";
		m_trailObject.GetComponent<MeshFilter>().mesh = m_trailMesh;
		for (int i = 0; i < 160; i++)
		{
			m_pointsPool.Add(new Point());
		}
		Use = true;
	}

	private void OnEnable()
	{
		Instances.Add(this);
	}

	private void OnDisable()
	{
		Instances.Remove(this);
		Object.Destroy((Object)(object)m_trailObject);
	}

	public void CustomFixedUpdate(float fixedDeltaTime)
	{
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_053b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0540: Unknown result type (might be due to invalid IL or missing references)
		//IL_055e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0563: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_058f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0594: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_049c: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0505: Unknown result type (might be due to invalid IL or missing references)
		//IL_050a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0714: Unknown result type (might be due to invalid IL or missing references)
		//IL_0719: Unknown result type (might be due to invalid IL or missing references)
		//IL_0720: Unknown result type (might be due to invalid IL or missing references)
		//IL_0725: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_031b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0320: Unknown result type (might be due to invalid IL or missing references)
		//IL_033f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0344: Unknown result type (might be due to invalid IL or missing references)
		//IL_0363: Unknown result type (might be due to invalid IL or missing references)
		//IL_0368: Unknown result type (might be due to invalid IL or missing references)
		//IL_0419: Unknown result type (might be due to invalid IL or missing references)
		//IL_041e: Unknown result type (might be due to invalid IL or missing references)
		//IL_042d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0432: Unknown result type (might be due to invalid IL or missing references)
		//IL_088c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0893: Unknown result type (might be due to invalid IL or missing references)
		//IL_0898: Unknown result type (might be due to invalid IL or missing references)
		//IL_089d: Unknown result type (might be due to invalid IL or missing references)
		//IL_08a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_08ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_08b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_08ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_08bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_08cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_08d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_08dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_08e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_08e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_08f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_08fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_08fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_08fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0903: Unknown result type (might be due to invalid IL or missing references)
		//IL_0905: Unknown result type (might be due to invalid IL or missing references)
		//IL_07bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_07cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_07d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_07d7: Unknown result type (might be due to invalid IL or missing references)
		float time = Time.time;
		if (!Use)
		{
			return;
		}
		if (_emit && _emitTime != 0f)
		{
			_emitTime -= fixedDeltaTime;
			if (_emitTime == 0f)
			{
				_emitTime = -1f;
			}
			if (_emitTime < 0f)
			{
				_emit = false;
			}
		}
		if (!_emit && m_points.Count == 0 && _autoDestruct)
		{
			Object.Destroy((Object)(object)m_trailObject);
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
		if ((Object)(object)Utils.GetMainCamera() == (Object)null)
		{
			return;
		}
		if (_emit)
		{
			Vector3 val = m_lastPosition - ((Component)this).transform.position;
			float sqrMagnitude = ((Vector3)(ref val)).sqrMagnitude;
			if (sqrMagnitude > m_minVertexDistanceSqr)
			{
				bool flag = false;
				if (m_points.Count < 3)
				{
					flag = true;
				}
				else
				{
					List<Point> points = m_points;
					Vector3 tipPosition = points[points.Count - 2].tipPosition;
					List<Point> points2 = m_points;
					Vector3 val2 = tipPosition - points2[points2.Count - 3].tipPosition;
					List<Point> points3 = m_points;
					Vector3 tipPosition2 = points3[points3.Count - 1].tipPosition;
					List<Point> points4 = m_points;
					Vector3 val3 = tipPosition2 - points4[points4.Count - 2].tipPosition;
					if (Vector3.Angle(val2, val3) > _maxAngle || sqrMagnitude > m_maxVertexDistanceSqr)
					{
						flag = true;
					}
				}
				if (flag)
				{
					Point pooledPoint = GetPooledPoint();
					pooledPoint.endTime = time + _lifeTime;
					pooledPoint.basePosition = _base.position;
					pooledPoint.tipPosition = _tip.position;
					m_points.Add(pooledPoint);
					m_lastPosition = ((Component)this).transform.position;
					if (m_points.Count == 1)
					{
						m_smoothedPoints.Add(pooledPoint);
					}
					else if (m_points.Count > 1)
					{
						for (int i = 0; i < 1 + subdivisions; i++)
						{
							m_smoothedPoints.Add(pooledPoint);
						}
					}
					if (m_points.Count >= 4)
					{
						Vector3[] tipPoints = m_tipPoints;
						List<Point> points5 = m_points;
						tipPoints[0] = points5[points5.Count - 4].tipPosition;
						Vector3[] tipPoints2 = m_tipPoints;
						List<Point> points6 = m_points;
						tipPoints2[1] = points6[points6.Count - 3].tipPosition;
						Vector3[] tipPoints3 = m_tipPoints;
						List<Point> points7 = m_points;
						tipPoints3[2] = points7[points7.Count - 2].tipPosition;
						Vector3[] tipPoints4 = m_tipPoints;
						List<Point> points8 = m_points;
						tipPoints4[3] = points8[points8.Count - 1].tipPosition;
						IEnumerable<Vector3> source = Interpolate.NewCatmullRom(m_tipPoints, subdivisions, loop: false);
						Vector3[] basePoints = m_basePoints;
						List<Point> points9 = m_points;
						basePoints[0] = points9[points9.Count - 4].basePosition;
						Vector3[] basePoints2 = m_basePoints;
						List<Point> points10 = m_points;
						basePoints2[1] = points10[points10.Count - 3].basePosition;
						Vector3[] basePoints3 = m_basePoints;
						List<Point> points11 = m_points;
						basePoints3[2] = points11[points11.Count - 2].basePosition;
						Vector3[] basePoints4 = m_basePoints;
						List<Point> points12 = m_points;
						basePoints4[3] = points12[points12.Count - 1].basePosition;
						IEnumerable<Vector3> source2 = Interpolate.NewCatmullRom(m_basePoints, subdivisions, loop: false);
						m_smoothTipList = source.ToList();
						m_smoothBaseList = source2.ToList();
						List<Point> points13 = m_points;
						float endTime = points13[points13.Count - 4].endTime;
						List<Point> points14 = m_points;
						float endTime2 = points14[points14.Count - 1].endTime;
						for (int j = 0; j < m_smoothTipList.Count; j++)
						{
							int num = m_smoothedPoints.Count - (m_smoothTipList.Count - j);
							if (num > -1 && num < m_smoothedPoints.Count)
							{
								Point pooledPoint2 = GetPooledPoint();
								pooledPoint2.basePosition = m_smoothBaseList[j];
								pooledPoint2.tipPosition = m_smoothTipList[j];
								pooledPoint2.endTime = Utils.Lerp(endTime, endTime2, (float)j / (float)m_smoothTipList.Count);
								m_smoothedPoints[num] = pooledPoint2;
							}
						}
					}
				}
				else
				{
					List<Point> points15 = m_points;
					points15[points15.Count - 1].basePosition = _base.position;
					List<Point> points16 = m_points;
					points16[points16.Count - 1].tipPosition = _tip.position;
					List<Point> smoothedPoints = m_smoothedPoints;
					smoothedPoints[smoothedPoints.Count - 1].basePosition = _base.position;
					List<Point> smoothedPoints2 = m_smoothedPoints;
					smoothedPoints2[smoothedPoints2.Count - 1].tipPosition = _tip.position;
				}
			}
			else
			{
				if (m_points.Count > 0)
				{
					List<Point> points17 = m_points;
					points17[points17.Count - 1].basePosition = _base.position;
					List<Point> points18 = m_points;
					points18[points18.Count - 1].tipPosition = _tip.position;
				}
				if (m_smoothedPoints.Count > 0)
				{
					List<Point> smoothedPoints3 = m_smoothedPoints;
					smoothedPoints3[smoothedPoints3.Count - 1].basePosition = _base.position;
					List<Point> smoothedPoints4 = m_smoothedPoints;
					smoothedPoints4[smoothedPoints4.Count - 1].tipPosition = _tip.position;
				}
			}
		}
		RemoveOldPoints(m_points, time);
		if (m_points.Count == 0)
		{
			m_trailMesh.Clear();
		}
		RemoveOldPoints(m_smoothedPoints, time);
		if (m_smoothedPoints.Count == 0)
		{
			m_trailMesh.Clear();
		}
		List<Point> smoothedPoints5 = m_smoothedPoints;
		if (smoothedPoints5.Count <= 1)
		{
			return;
		}
		int num2 = smoothedPoints5.Count * 2;
		int num3 = (smoothedPoints5.Count - 1) * 6;
		if (m_newVertices.Capacity < num2)
		{
			m_newVertices.Capacity = num2;
			m_newUV.Capacity = num2;
			m_newColors.Capacity = num2;
		}
		if (m_newTriangles.Capacity < num3)
		{
			m_newTriangles.Capacity = num3;
		}
		ListExtra.Resize<Vector3>(m_newVertices, num2);
		ListExtra.Resize<Vector2>(m_newUV, num2);
		ListExtra.Resize<Color>(m_newColors, num2);
		ListExtra.Resize<int>(m_newTriangles, num3);
		Vector3[] array = m_newVertices.ToArray();
		Vector2[] array2 = m_newUV.ToArray();
		Color[] array3 = m_newColors.ToArray();
		int[] array4 = m_newTriangles.ToArray();
		float num4 = time + _lifeTime;
		for (int k = 0; k < smoothedPoints5.Count; k++)
		{
			Point point = smoothedPoints5[k];
			float num5 = (num4 - point.endTime) / _lifeTime;
			Color val4 = Color.Lerp(Color.white, Color.clear, num5);
			Color[] colors = _colors;
			int num6 = ((colors != null) ? colors.Length : 0);
			if (num6 > 0)
			{
				float num7 = num5 * (float)(num6 - 1);
				float num8 = Mathf.Floor(num7);
				float num9 = Utils.Clamp(Mathf.Ceil(num7), 1f, (float)(num6 - 1));
				float num10 = Mathf.InverseLerp(num8, num9, num7);
				if (num8 >= (float)num6)
				{
					num8 = num6 - 1;
				}
				if (num8 < 0f)
				{
					num8 = 0f;
				}
				if (num9 >= (float)num6)
				{
					num9 = num6 - 1;
				}
				if (num9 < 0f)
				{
					num9 = 0f;
				}
				val4 = Color.Lerp(_colors[(int)num8], _colors[(int)num9], num10);
			}
			float num11 = 0f;
			float[] sizes = _sizes;
			int num12 = ((sizes != null) ? sizes.Length : 0);
			if (num12 > 0)
			{
				float num13 = num5 * (float)(num12 - 1);
				float num14 = Mathf.Floor(num13);
				float num15 = Utils.Clamp(Mathf.Ceil(num13), 1f, (float)(num12 - 1));
				float num16 = Mathf.InverseLerp(num14, num15, num13);
				if (num14 >= (float)num12)
				{
					num14 = num12 - 1;
				}
				if (num14 < 0f)
				{
					num14 = 0f;
				}
				if (num15 >= (float)num12)
				{
					num15 = num12 - 1;
				}
				if (num15 < 0f)
				{
					num15 = 0f;
				}
				num11 = Mathf.Lerp(_sizes[(int)num14], _sizes[(int)num15], num16);
			}
			Vector3 val5 = point.tipPosition - point.basePosition;
			array[k * 2] = point.basePosition - val5 * (num11 * 0.5f);
			array[k * 2 + 1] = point.tipPosition + val5 * (num11 * 0.5f);
			array3[k * 2] = (array3[k * 2 + 1] = val4);
			float x = (float)k / (float)smoothedPoints5.Count;
			array2[k * 2].x = x;
			array2[k * 2].y = 0f;
			array2[k * 2 + 1].x = x;
			array2[k * 2 + 1].y = 1f;
			if (k > 0)
			{
				array4[(k - 1) * 6] = k * 2 - 2;
				array4[(k - 1) * 6 + 1] = k * 2 - 1;
				array4[(k - 1) * 6 + 2] = k * 2;
				array4[(k - 1) * 6 + 3] = k * 2 + 1;
				array4[(k - 1) * 6 + 4] = k * 2;
				array4[(k - 1) * 6 + 5] = k * 2 - 1;
			}
		}
		m_trailMesh.Clear();
		m_trailMesh.vertices = array;
		m_trailMesh.colors = array3;
		m_trailMesh.uv = array2;
		m_trailMesh.triangles = array4;
	}

	private void RemoveOldPoints(List<Point> pointList, float time)
	{
		for (int num = pointList.Count - 1; num >= 0; num--)
		{
			Point point = pointList[num];
			if (time > point.endTime)
			{
				pointList.RemoveAt(num);
			}
		}
	}
}
