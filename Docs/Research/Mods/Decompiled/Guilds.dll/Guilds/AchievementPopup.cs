using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Guilds;

[PublicAPI]
public class AchievementPopup : MonoBehaviour
{
	private struct QueuedPopup
	{
		public PlayerReference Player;

		public AchievementConfig Config;

		public int Level;
	}

	[CompilerGenerated]
	private sealed class _003C_003CQueue_003Eg__Dequeue_007C41_0_003Ed : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public AchievementPopup _003C_003E4__this;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003C_003CQueue_003Eg__Dequeue_007C41_0_003Ed(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Expected O, but got Unknown
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Expected O, but got Unknown
			int num = _003C_003E1__state;
			AchievementPopup achievementPopup = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				break;
			case 1:
				_003C_003E1__state = -1;
				achievementPopup.Hide();
				_003C_003E2__current = (object)new WaitForSeconds(2f);
				_003C_003E1__state = 2;
				return true;
			case 2:
				_003C_003E1__state = -1;
				break;
			}
			if (achievementPopup.queue.Count > 0)
			{
				QueuedPopup queuedPopup = achievementPopup.queue.Dequeue();
				achievementPopup.achievementIconImg.sprite = queuedPopup.Config.GetIcon() ?? achievementPopup.defaultIcon;
				((TMP_Text)achievementPopup.aiDescription).text = queuedPopup.Config.name;
				((TMP_Text)achievementPopup.guildLevelText).text = queuedPopup.Config.GetLevel(queuedPopup.Level).ToString();
				achievementPopup.Show();
				_003C_003E2__current = (object)new WaitForSeconds(5f);
				_003C_003E1__state = 1;
				return true;
			}
			((Component)achievementPopup).gameObject.SetActive(false);
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[Header("Row Root")]
	public RectTransform rowRootTransform;

	public AchievementPopup rowInstance;

	public GameObject rowRootGameObject;

	public RectTransform back;

	public Image backImage;

	public Image borderImage;

	public GameObject rowNotCompleted;

	[Header("Content Area")]
	public RectTransform contentAreaRectTransform;

	[Header("Left Column")]
	public RectTransform leftColumnRect;

	public HorizontalLayoutGroup leftColumnHLayoutGroup;

	[Header("Left Column - Icon Container")]
	public RectTransform achievementIconContainerRect;

	public Image achievementIconContainerImg;

	public RectTransform achievementIconImgRect;

	public RectTransform achievementIconBorderRect;

	public Image achievementIconBorderImg;

	public Image achievementIconImg;

	[Header("Right Column - Rank Area")]
	public RectTransform rightCol;

	public Image rightColBg;

	public RectTransform rightColIconContainer;

	public Image rightColIconContainerSelectedIcon;

	public Image rightColIconcontainerBorder;

	public RectTransform guildLevelRect;

	public TextMeshProUGUI guildLevelText;

	[Header("Right Column - AchievementInformation")]
	public RectTransform aiRect;

	public RectTransform aiHeader;

	public HorizontalLayoutGroup aiHeaderHLG;

	public TextMeshProUGUI aiHeaderTitle;

	public TextMeshProUGUI aiHeaderDate;

	public TextMeshProUGUI aiDescription;

	public RectTransform progressBar;

	public Image progressBarBg;

	public Image progressBarBgGreen;

	public TextMeshProUGUI progressText;

	[Header("Animator")]
	public Animator animatorComp;

	public Sprite defaultIcon;

	private Queue<QueuedPopup> queue = new Queue<QueuedPopup>();

	public void Awake()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		RectTransform component = ((Component)this).GetComponent<RectTransform>();
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector(0.5f, 0.25f);
		component.anchorMax = val;
		component.anchorMin = val;
		defaultIcon = achievementIconImg.sprite;
	}

	public void Show()
	{
		animatorComp.SetBool("visible", true);
	}

	public void Hide()
	{
		if (animatorComp.GetBool("visible"))
		{
			animatorComp.SetBool("visible", false);
		}
	}

	public static void Queue(PlayerReference player, AchievementConfig config, int level)
	{
		if (Object.op_Implicit((Object)(object)Interface.AchievementPopup))
		{
			Interface.AchievementPopup.GetComponent<AchievementPopup>().Queue(new QueuedPopup
			{
				Player = player,
				Config = config,
				Level = level
			});
		}
	}

	private void Queue(QueuedPopup popup)
	{
		queue.Enqueue(popup);
		if (!((Component)this).gameObject.activeSelf)
		{
			((Component)this).gameObject.SetActive(true);
			((MonoBehaviour)this).StartCoroutine(Dequeue());
		}
		[IteratorStateMachine(typeof(_003C_003CQueue_003Eg__Dequeue_007C41_0_003Ed))]
		IEnumerator Dequeue()
		{
			//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
			return new _003C_003CQueue_003Eg__Dequeue_007C41_0_003Ed(0)
			{
				_003C_003E4__this = this
			};
		}
	}
}
