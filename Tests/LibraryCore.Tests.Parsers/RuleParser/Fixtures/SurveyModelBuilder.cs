﻿using static LibraryCore.Tests.Parsers.RuleParser.Fixtures.Survey;

namespace LibraryCore.Tests.Parsers.RuleParser.Fixtures;

public record Survey(string Name, int SurgeryCount, double PriceOfSurgery, DateTime DateOfBirth, DateTime? LastLogin, bool CanDrive, bool? HasAccount, int? NumberOfMotorcyles, double? NumberOfBoats, IDictionary<int, string> Answers, Survey? InnerSurvey, string? NullableNameTest)
{
    public string InstanceMethodName(int howManyCharacters) => Name[..howManyCharacters];

    public SmokingStatusEnum SmokingStatus { get; set; }
    public SmokingStatusEnum? NullableSmokingStatus { get; set; }

    public enum SmokingStatusEnum
    {
        DoNotSmoke,
        Moderate,
        SmokeAlot
    }
}

public class SurveyModelBuilder
{
    public SurveyModelBuilder()
    {
        Value = new Survey("Jacob DeGrom", 10, 9.99, new DateTime(2020, 2, 1), null, true, null, null, null, new Dictionary<int, string>
        {
            { 1, "Yes" },
            { 2, "No" },
            { 3, "Maybe" }
        }, null, null);
    }

    public Survey Value { get; set; }

    public SurveyModelBuilder WithName(string name) => SetAndReturn(Value with { Name = name });
    public SurveyModelBuilder WithSurgeryCount(int surgeryCount) => SetAndReturn(Value with { SurgeryCount = surgeryCount });
    public SurveyModelBuilder WithSurgeryPrice(double price) => SetAndReturn(Value with { PriceOfSurgery = price });
    public SurveyModelBuilder WithAnswers(IDictionary<int, string> answers) => SetAndReturn(Value with { Answers = answers });
    public SurveyModelBuilder WithBirthDate(DateTime date) => SetAndReturn(Value with { DateOfBirth = date });
    public SurveyModelBuilder WithLastLogIn(DateTime? date) => SetAndReturn(Value with { LastLogin = date });
    public SurveyModelBuilder WithCanDrive(bool value) => SetAndReturn(Value with { CanDrive = value });
    public SurveyModelBuilder WithHasAccount(bool? value) => SetAndReturn(Value with { HasAccount = value });
    public SurveyModelBuilder WithNumberOfMotorcycles(int? value) => SetAndReturn(Value with { NumberOfMotorcyles = value });
    public SurveyModelBuilder WithNumberOfBoats(double? value) => SetAndReturn(Value with { NumberOfBoats = value });
    public SurveyModelBuilder WithInnerSurveyObject(Survey innerSurvey) => SetAndReturn(Value with { InnerSurvey = innerSurvey });
    public SurveyModelBuilder WithNullableNameTest(string? valueToSet) => SetAndReturn(Value with { NullableNameTest = valueToSet });
    public SurveyModelBuilder WithSmokingStatus(SmokingStatusEnum valueToSet) => SetAndReturn(Value with { SmokingStatus = valueToSet });
    public SurveyModelBuilder WithNullableSmokingStatus(SmokingStatusEnum valueToSet) => SetAndReturn(Value with { NullableSmokingStatus = valueToSet });

    public static IEnumerable<Survey> CreateArrayOfRecords(params SurveyModelBuilder[] surveyModelBuilders)
    {
        foreach (var surveyModelBuilder in surveyModelBuilders)
        {
            yield return surveyModelBuilder.Value;
        }
    }

    private SurveyModelBuilder SetAndReturn(Survey survey)
    {
        Value = survey;
        return this;
    }

}
