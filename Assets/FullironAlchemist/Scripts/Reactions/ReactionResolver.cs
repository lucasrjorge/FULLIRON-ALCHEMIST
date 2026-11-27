using System.Collections.Generic;

namespace Fulliron.Alchemy
{
    public static class ReactionResolver
    {
        public static ReactionResult Resolve(AlchemyDatabase database, ReactionRequest request)
        {
            if (database == null)
                return ReactionResult.Fail(null, "Banco de dados de alquimia ausente.", null);

            if (request == null)
                return ReactionResult.Fail(null, "Requisição de reação vazia.", null);

            if (database.reactionRecipes == null)
            {
                var emptyFailure = new FailureProfile
                {
                    failureMessage = "O banco não tem receitas de reação."
                };
                return ReactionResult.Fail(null, emptyFailure.failureMessage, emptyFailure);
            }

            ReactionRecipe best = null;
            for (int i = 0; i < database.reactionRecipes.Count; i++)
            {
                var recipe = database.reactionRecipes[i];
                if (recipe == null)
                    continue;
                if (!string.IsNullOrEmpty(recipe.symbolId) && recipe.symbolId != request.symbolId)
                    continue;
                if (!Matches(recipe, request))
                    continue;
                if (best == null || recipe.priority > best.priority)
                    best = recipe;
            }

            if (best == null)
            {
                var failure = new FailureProfile
                {
                    failureMessage = "Os componentes não formaram a reação esperada."
                };
                return ReactionResult.Fail(null, failure.failureMessage, failure);
            }

            return ReactionResult.Success(best, "A reação se completou.");
        }

        static bool Matches(ReactionRecipe recipe, ReactionRequest request)
        {
            if (recipe.requirements == null)
                return false;

            for (int i = 0; i < recipe.requirements.Count; i++)
            {
                var requirement = recipe.requirements[i];
                if (requirement == null)
                    return false;

                Placement placement = FindPlacement(request, requirement.anchorElement);
                if (placement == null || !requirement.Matches(placement.item))
                    return false;
            }

            return true;
        }

        static Placement FindPlacement(ReactionRequest request, AlchemicalElement element)
        {
            if (request.placements == null)
                return null;

            for (int i = 0; i < request.placements.Count; i++)
            {
                var placement = request.placements[i];
                if (placement != null && placement.anchorElement == element)
                    return placement;
            }

            return null;
        }
    }
}
