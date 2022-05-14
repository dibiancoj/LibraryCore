using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq.Expressions;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

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
public class ScoreCriteriaToken<TScore> : IToken
{
    public ScoreCriteriaToken(TScore scoreValue, IImmutableList<IToken> scoreCriteriaTokens)
    {
        ScoreValue = scoreValue;
        ScoreCriteriaTokens = scoreCriteriaTokens;
    }

    public TScore ScoreValue { get; }
    public IImmutableList<IToken> ScoreCriteriaTokens { get; }

    public Expression CreateExpression(IImmutableList<ParameterExpression> parameters) => throw new NotImplementedException();
}
