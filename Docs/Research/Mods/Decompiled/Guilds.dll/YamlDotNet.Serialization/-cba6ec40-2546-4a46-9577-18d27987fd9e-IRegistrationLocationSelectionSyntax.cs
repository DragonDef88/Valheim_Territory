namespace YamlDotNet.Serialization;

internal interface _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIRegistrationLocationSelectionSyntax<TBaseRegistrationType>
{
	void InsteadOf<TRegistrationType>() where TRegistrationType : TBaseRegistrationType;

	void Before<TRegistrationType>() where TRegistrationType : TBaseRegistrationType;

	void After<TRegistrationType>() where TRegistrationType : TBaseRegistrationType;

	void OnTop();

	void OnBottom();
}
