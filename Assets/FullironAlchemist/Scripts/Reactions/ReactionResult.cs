namespace Fulliron.Alchemy
{
    public class ReactionResult
    {
        public bool success;
        public ReactionRecipe recipe;
        public string message;
        public FailureProfile failure;

        public static ReactionResult Success(ReactionRecipe recipe, string message)
        {
            return new ReactionResult
            {
                success = true,
                recipe = recipe,
                message = message
            };
        }

        public static ReactionResult Fail(ReactionRecipe recipe, string message, FailureProfile failure)
        {
            return new ReactionResult
            {
                success = false,
                recipe = recipe,
                message = message,
                failure = failure
            };
        }
    }
}
