namespace STUWard;

internal readonly struct WardConfigurationRequestSubmission
{
	internal bool IsPending { get; }

	internal long RequestId { get; }

	internal WardConfigurationRequestResultCode ResultCode { get; }

	internal WardConfiguration Configuration { get; }

	internal bool ShowOverlapMessage { get; }

	internal WardConfigurationRequestSubmission(bool isPending, long requestId, WardConfigurationRequestResultCode resultCode, WardConfiguration configuration, bool showOverlapMessage)
	{
		IsPending = isPending;
		RequestId = requestId;
		ResultCode = resultCode;
		Configuration = configuration;
		ShowOverlapMessage = showOverlapMessage;
	}
}
