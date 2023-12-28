using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq.Expressions;

namespace LibraryCore.Parsers.RuleParser.TokenFactories.Implementation;

public class ScoreToken
{
    public enum ScoringMode
    {
        AccumulatedScore,
        ShortCircuitOnFirstTrueEval
    }
}

public record ScoringCriteriaParameter<TScoreType>(TScoreType ScoreValueIfTrue, string ScoreTruthCriteria);

[DebuggerDisplay("Score Value = {ScoreValue}")]
public class ScoreCriteriaToken<TScore>(TScore scoreValue, IReadOnlyList<IToken> scoreCriteriaTokens) : IToken
{
    public TScore ScoreValue { get; } = scoreValue;
    public IReadOnlyList<IToken> ScoreCriteriaTokens { get; } = scoreCriteriaTokens;

    public Expression CreateExpression(IReadOnlyList<ParameterExpression> parameters) => throw new NotImplementedException();
}
