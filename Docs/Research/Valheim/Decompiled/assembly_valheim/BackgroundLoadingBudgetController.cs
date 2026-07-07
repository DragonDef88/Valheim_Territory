using System.Collections.Generic;
using UnityEngine;

public class BackgroundLoadingBudgetController
{
	private const ThreadPriority c_defaultBudget = 0;

	private static List<ThreadPriority> m_budgetRequests = new List<ThreadPriority>();

	[RuntimeInitializeOnLoadMethod]
	private static void OnLoad()
	{
		ApplyBudget();
	}

	public static ThreadPriority RequestLoadingBudget(ThreadPriority priority)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		AddRequest(priority);
		ApplyBudget();
		return priority;
	}

	public static ThreadPriority UpdateLoadingBudgetRequest(ThreadPriority oldPriority, ThreadPriority newPriority)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		RemoveRequest(oldPriority);
		AddRequest(newPriority);
		ApplyBudget();
		return newPriority;
	}

	public static void ReleaseLoadingBudgetRequest(ThreadPriority priority)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		RemoveRequest(priority);
		ApplyBudget();
	}

	private static void AddRequest(ThreadPriority priority)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		int num = m_budgetRequests.BinarySearch(priority);
		if (num < 0)
		{
			num = ~num;
		}
		m_budgetRequests.Insert(num, priority);
	}

	private static void RemoveRequest(ThreadPriority priority)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		int num = m_budgetRequests.BinarySearch(priority);
		if (num >= 0)
		{
			m_budgetRequests.RemoveAt(num);
		}
		else
		{
			ZLog.LogError((object)$"Failed to remove loading budget request {priority}");
		}
	}

	private static void ApplyBudget()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Invalid comparison between Unknown and I4
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		ThreadPriority val2 = (Application.backgroundLoadingPriority = (ThreadPriority)((m_budgetRequests.Count > 0 && (int)m_budgetRequests[m_budgetRequests.Count - 1] >= 0) ? ((int)m_budgetRequests[m_budgetRequests.Count - 1]) : 0));
		ZLog.Log((object)$"Set background loading budget to {val2}");
	}
}
