using LibraryCore.AspNet.Validation;
using System.ComponentModel.DataAnnotations;

namespace LibraryCore.Tests.AspNet.Validation;

public class RequiredIfValidationTest
{
    #region Framework

    public class RequiredIfModel
    {
        public string? Value { get; set; }

        [RequiredIf(nameof(Value), "Yes", ErrorMessage = "My Error Message")]
        public string? ValueIfYes { get; set; }

        [RequiredIf(nameof(Value), "Yes1", "Yes2", ErrorMessage = "My Error Message")]
        public string? ValueIfYes1OrYes2 { get; set; }
    }

    public class RequiredIfModelMissingProperty
    {
        [RequiredIf("MissingPropertyName", "SomeValue", ErrorMessage = "My Error Message")]
        public string? RequiredIfMissingPropertyCheck { get; set; }
    }

    #endregion

    #region Unit Tests

    #region Required If With 1 Value To Look For

    [InlineData("Yes", null, false)]
    [InlineData("Yes", "", false)]
    [InlineData("Yes", "Test 123", true)]
    [InlineData("No", null, true)]
    [InlineData("No", "Test 123", true)]
    [Theory]
    public void RequiredIfTestWhenNotFoundInLocalization(string value, string? valueIfYes, bool shouldBeValidModel)
    {
        var target = new RequiredIfModel { Value = value, ValueIfYes = valueIfYes };
        //don't use default <-- for automated builds
        var context = new ValidationContext(target);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(target, context, results, true);

        Assert.Equal(shouldBeValidModel, isValid);

        if (!shouldBeValidModel)
        {
            Assert.Single(results);
            Assert.Single(results, x => x.ErrorMessage == "My Error Message");
        }
    }

    [Fact]
    public void TriggerPropertyIsNull()
    {
        var target = new RequiredIfModel { Value = null };
        //don't use default <-- for automated builds
        var context = new ValidationContext(target);
        var results = new List<ValidationResult>();

        Assert.True(Validator.TryValidateObject(target, context, results, true));
    }

    #endregion

    #region Required If When Looking For Multiple Values - ie contains | or statement

    [InlineData("NotRequired", null, true)]
    [InlineData("Yes1", "", false)]
    [InlineData("Yes1", null, false)]
    [InlineData("Yes1", "ValueIsSet", true)]
    [InlineData("Yes2", "", false)]
    [InlineData("Yes2", null, false)]
    [InlineData("Yes2", "ValueIsSet", true)]
    [Theory]
    public void RequiredIfTestWhenLookingForMultipleValues(string value, string? valueIfYes1OrYes2, bool shouldBeValidModel)
    {
        var target = new RequiredIfModel { Value = value, ValueIfYes1OrYes2 = valueIfYes1OrYes2 };
        var context = new ValidationContext(target);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(target, context, results, true);

        Assert.Equal(shouldBeValidModel, isValid);

        if (!shouldBeValidModel)
        {
            Assert.Single(results);
            Assert.Single(results, x => x.ErrorMessage == "My Error Message");
        }
    }

    #endregion

    #endregion

    #region Required If Property Is Missing

    [Fact]
    public void PropertyNotFound()
    {
        var target = new RequiredIfModelMissingProperty();
        //don't use default <-- for automated builds
        var context = new ValidationContext(target);
        var results = new List<ValidationResult>();

        Assert.Throws<MissingFieldException>(() => Validator.TryValidateObject(target, context, results, true));
    }

    #endregion

}