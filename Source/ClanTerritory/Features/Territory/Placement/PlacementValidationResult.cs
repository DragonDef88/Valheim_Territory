namespace ClanTerritory.Features.Territory.Placement
{
    internal sealed class PlacementValidationResult
    {
        public static readonly PlacementValidationResult Success =
            new PlacementValidationResult(
                true,
                PlacementResult.Success,
                string.Empty);

        public bool IsSuccess { get; }

        public PlacementResult Result { get; }

        public string Message { get; }

        public PlacementValidationResult(
            bool isSuccess,
            PlacementResult result,
            string message)
        {
            IsSuccess = isSuccess;
            Result = result;
            Message = message;
        }

        public static PlacementValidationResult Failure(
            PlacementResult result,
            string message)
        {
            return new PlacementValidationResult(
                false,
                result,
                message);
        }
    }
}