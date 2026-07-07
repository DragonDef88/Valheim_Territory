namespace STUWard;

internal readonly struct WardConfigurationUpdateResult
{
	internal WardConfigurationRequestResultCode ResultCode { get; }

	internal WardConfiguration Configuration { get; }

	internal bool ShowOverlapMessage { get; }

	internal WardConfigurationUpdateResult(WardConfigurationRequestResultCode resultCode, WardConfiguration configuration, bool showOverlapMessage)
	{
		ResultCode = resultCode;
		Configuration = configuration;
		ShowOverlapMessage = showOverlapMessage;
	}
}
