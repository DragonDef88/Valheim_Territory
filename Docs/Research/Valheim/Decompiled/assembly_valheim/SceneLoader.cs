using System.Collections;
using SoftReferenceableAssets.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
	public SceneReference m_scene;

	private bool _showLogos = true;

	private bool _showHealthWarning;

	private bool _showSaveNotification;

	private bool _skipEnabled;

	private bool _logosSkippable;

	private bool _skipAllAtOnce;

	private bool _skipped;

	private ILoadSceneAsyncOperation _sceneLoadOperation;

	private ThreadPriority _currentLoadingBudgetRequest;

	private float _fakeProgress;

	[SerializeField]
	private LoadingIndicator loadingIndicator;

	[SerializeField]
	private GameObject gameLogo;

	[SerializeField]
	private GameObject coffeeStainLogo;

	[SerializeField]
	private GameObject ironGateLogo;

	[SerializeField]
	private CanvasGroup savingNotification;

	[SerializeField]
	private CanvasGroup healthWarning;

	public AnimationCurve alphaCurve;

	public AnimationCurve scalingCurve;

	private const float LogoDisplayTime = 2f;

	private const float SaveNotificationDisplayTime = 5f;

	private const float HealthWarningDisplayTime = 5f;

	private const float FadeInOutTime = 0.5f;

	private void Awake()
	{
		_showLogos = true;
		_showHealthWarning = false;
		_showSaveNotification = false;
		((Component)healthWarning).gameObject.SetActive(false);
		((Component)savingNotification).gameObject.SetActive(false);
		coffeeStainLogo.SetActive(false);
		ironGateLogo.SetActive(false);
		gameLogo.SetActive(false);
		ZInput.Initialize();
	}

	private void Start()
	{
		StartLoading();
	}

	private void Update()
	{
		ZInput.Update(Time.unscaledDeltaTime);
		if (_skipEnabled && (ZInput.GetButtonDown("JoyButtonA") || ZInput.GetMouseButtonDown(0)))
		{
			_skipped = true;
		}
		if (!loadingIndicator.IsVisible)
		{
			return;
		}
		float num = ((_sceneLoadOperation == null) ? 0f : _sceneLoadOperation.Progress);
		if (num <= 0.25f)
		{
			float num2 = num / 0.25f * 0.05f;
			if (_fakeProgress < num2)
			{
				_fakeProgress = num2;
			}
			else if (num == 0.25f)
			{
				_fakeProgress = Mathf.Min(num, _fakeProgress + Time.deltaTime * 0.01f);
			}
		}
		else
		{
			_fakeProgress = num;
		}
		loadingIndicator.SetProgress(_fakeProgress);
	}

	private void OnDestroy()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if ((int)_currentLoadingBudgetRequest != 0)
		{
			BackgroundLoadingBudgetController.ReleaseLoadingBudgetRequest(_currentLoadingBudgetRequest);
		}
	}

	private void StartLoading()
	{
		((MonoBehaviour)this).StartCoroutine(LoadSceneAsync());
	}

	private IEnumerator LoadSceneAsync()
	{
		SceneReference scene = m_scene;
		ZLog.Log((object)("Starting to load scene:" + ((object)(SceneReference)(ref scene)).ToString()));
		_sceneLoadOperation = SceneManager.LoadSceneAsync(m_scene, (LoadSceneMode)0);
		_currentLoadingBudgetRequest = BackgroundLoadingBudgetController.RequestLoadingBudget((ThreadPriority)2);
		_sceneLoadOperation.AllowSceneActivation = false;
		_ = Localization.instance;
		PlatformInitializer.AllowSaveDataInitialization = false;
		if (PlatformInitializer.StartedSaveDataInitialization)
		{
			while (!PlatformInitializer.SaveDataInitialized)
			{
				yield return null;
			}
		}
		if (_showLogos)
		{
			Image componentInChildren = coffeeStainLogo.GetComponentInChildren<Image>();
			Image igImage = ironGateLogo.GetComponentInChildren<Image>();
			if (!_skipAllAtOnce)
			{
				_skipped = false;
			}
			if (!_logosSkippable || !_skipped)
			{
				yield return FadeLogo(coffeeStainLogo, componentInChildren, 2f, alphaCurve, scalingCurve);
			}
			coffeeStainLogo.SetActive(false);
			if (!_skipAllAtOnce)
			{
				_skipped = false;
			}
			if (!_logosSkippable || !_skipped)
			{
				yield return FadeLogo(ironGateLogo, igImage, 2f, alphaCurve, scalingCurve);
			}
			ironGateLogo.SetActive(false);
		}
		if (_showSaveNotification)
		{
			if (!_skipAllAtOnce)
			{
				_skipped = false;
			}
			if (!_skipped)
			{
				yield return ShowSaveNotification();
			}
		}
		if (_showHealthWarning)
		{
			if (!_skipAllAtOnce)
			{
				_skipped = false;
			}
			if (!_skipped)
			{
				yield return ShowHealthWarning();
			}
		}
		gameLogo.SetActive(true);
		_currentLoadingBudgetRequest = BackgroundLoadingBudgetController.UpdateLoadingBudgetRequest(_currentLoadingBudgetRequest, (ThreadPriority)4);
		loadingIndicator.SetShow(show: true);
		PlatformInitializer.AllowSaveDataInitialization = true;
		while (!PlatformInitializer.SaveDataInitialized)
		{
			yield return null;
		}
		PlatformInitializer.InputDeviceRequired = true;
		if (StartupMessages.Instance != null)
		{
			StartupMessages.Instance.DisplayStartupMessages();
		}
		while (!_sceneLoadOperation.IsLoadedButNotActivated)
		{
			yield return null;
		}
		loadingIndicator.SetShow(show: false);
		while (loadingIndicator.IsVisible)
		{
			yield return null;
		}
		yield return null;
		while (PlatformInitializer.WaitingForInputDevice)
		{
			yield return null;
		}
		if (StartupMessages.Instance != null)
		{
			while (StartupMessages.Instance.StartupMessageDisplayed)
			{
				yield return null;
			}
		}
		_sceneLoadOperation.AllowSceneActivation = true;
	}

	private IEnumerator ShowSaveNotification()
	{
		if (!_skipped)
		{
			savingNotification.alpha = 0f;
			((Component)savingNotification).gameObject.SetActive(true);
			yield return null;
			Transform transform = ((Component)healthWarning).transform;
			LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)(object)((transform is RectTransform) ? transform : null));
			float fadeTimer3 = 0f;
			while (true)
			{
				if (savingNotification.alpha < 1f)
				{
					float num = 1f - (0.5f - fadeTimer3) / 0.5f;
					float alpha = Mathf.SmoothStep(0f, 1f, num);
					savingNotification.alpha = alpha;
					fadeTimer3 += Time.unscaledDeltaTime;
					if (_skipped)
					{
						break;
					}
					yield return null;
					continue;
				}
				fadeTimer3 = 0f;
				while (true)
				{
					if (fadeTimer3 < 5f)
					{
						fadeTimer3 += Time.unscaledDeltaTime;
						if (_skipped)
						{
							break;
						}
						yield return null;
						continue;
					}
					fadeTimer3 = 0f;
					while (savingNotification.alpha > 0f)
					{
						float num = 1f - (0.5f - fadeTimer3) / 0.5f;
						float alpha = Mathf.SmoothStep(savingNotification.alpha, 0f, num);
						savingNotification.alpha = alpha;
						fadeTimer3 += Time.unscaledDeltaTime;
						if (_skipped)
						{
							break;
						}
						yield return null;
					}
					break;
				}
				break;
			}
		}
		((Component)savingNotification).gameObject.SetActive(false);
	}

	private IEnumerator ShowHealthWarning()
	{
		if (!_skipped)
		{
			healthWarning.alpha = 0f;
			((Component)healthWarning).gameObject.SetActive(true);
			yield return null;
			Transform transform = ((Component)healthWarning).transform;
			LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)(object)((transform is RectTransform) ? transform : null));
			float fadeTimer3 = 0f;
			while (true)
			{
				if (healthWarning.alpha < 1f)
				{
					float num = 1f - (0.5f - fadeTimer3) / 0.5f;
					float alpha = Mathf.SmoothStep(0f, 1f, num);
					healthWarning.alpha = alpha;
					fadeTimer3 += Time.unscaledDeltaTime;
					if (_skipped)
					{
						break;
					}
					yield return null;
					continue;
				}
				fadeTimer3 = 0f;
				while (true)
				{
					if (fadeTimer3 < 5f)
					{
						fadeTimer3 += Time.unscaledDeltaTime;
						if (_skipped)
						{
							break;
						}
						yield return null;
						continue;
					}
					fadeTimer3 = 0f;
					while (healthWarning.alpha > 0f)
					{
						float num = 1f - (0.5f - fadeTimer3) / 0.5f;
						float alpha = Mathf.SmoothStep(healthWarning.alpha, 0f, num);
						healthWarning.alpha = alpha;
						fadeTimer3 += Time.unscaledDeltaTime;
						if (_skipped)
						{
							break;
						}
						yield return null;
					}
					break;
				}
				break;
			}
		}
		((Component)healthWarning).gameObject.SetActive(false);
	}

	private IEnumerator FadeLogo(GameObject parentGameObject, Image logo, float duration, AnimationCurve alpha, AnimationCurve scale)
	{
		Color spriteColor = ((Graphic)logo).color;
		float timer = 0f;
		parentGameObject.SetActive(true);
		while (timer < duration && (!_logosSkippable || !_skipped))
		{
			float a = alpha.Evaluate(timer);
			spriteColor.a = a;
			((Graphic)logo).color = spriteColor;
			a = scale.Evaluate(timer);
			((Component)logo).transform.localScale = Vector3.one * a;
			timer += Time.deltaTime;
			yield return null;
		}
	}
}
