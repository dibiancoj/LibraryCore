using LibraryCore.AspNet.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Xunit;
using static LibraryCore.Tests.AspNet.Validation.RequiredIfContainsTest.RequiredIfModel;

namespace LibraryCore.Tests.AspNet.Validation;

public class RequiredIfContainsTest
{

    #region Framework

    public class RequiredIfModel
    {
        public enum TestEnum
        {
            One,
            Two,
            Three
        }

        public IList<TestEnum> Value { get; set; }

        [RequiredIfContains(nameof(Value), TestEnum.Two, ErrorMessage = "My_Error_Resource")]
        public string ValueIfYes { get; set; }
    }

    public class RequiredIfModelMissingProperty
    {
        [RequiredIfContains("MissingProperty", TestEnum.Two, ErrorMessage = "My_Error_Resource")]
        public string RequiredIfMissingPropertyCheck { get; set; }
    }

    public class RequiredNotArrayTestModel
    {
        public string Value { get; set; }

        [RequiredIfContains(nameof(Value), TestEnum.Two, ErrorMessage = "My_Error_Resource")]
        public string ValueIfYes { get; set; }
    }

    private static IList<TestEnum> MockList(bool includeTwo)
    {
        var data = new List<TestEnum> { TestEnum.One, TestEnum.Three };

        if (includeTwo)
        {
            data.Add(TestEnum.Two);
        }

        return data;
    }

    #endregion

    #region Unit Tests

    [Fact]
    public void RequiredIfTestNotEnumerableShouldThrow()
    {
        var target = new RequiredNotArrayTestModel { Value = "Test" };
        var context = new ValidationContext(target);
        var results = new List<ValidationResult>();

        Assert.Throws<Exception>(() => Validator.TryValidateObject(target, context, results, true));
    }

    [InlineData(null)]
    [InlineData("Test 123")]
    [InlineData("")]
    [Theory]
    public void RequiredIfTestWhenEmptyList(string valueIfYes)
    {
        var target = new RequiredIfModel { Value = Array.Empty<TestEnum>(), ValueIfYes = valueIfYes };
        var context = new ValidationContext(target);
        var results = new List<ValidationResult>();

        Assert.True(Validator.TryValidateObject(target, context, results, true));
    }

    [InlineData(false, null, true)]
    [InlineData(false, "Test 123", true)]
    [InlineData(true, null, false)]
    [InlineData(true, "", false)]
    [InlineData(true, "Test 123", true)]
    [Theory]
    public void RequiredIfTestWhenNotFoundInLocalization(bool includeTwoInList, string valueIfYes, bool shouldBeValidModel)
    {
        var target = new RequiredIfModel { Value = MockList(includeTwoInList), ValueIfYes = valueIfYes };
        var context = new ValidationContext(target);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(target, context, results, true);

        Assert.Equal(shouldBeValidModel, isValid);

        if (!shouldBeValidModel)
        {
            Assert.Single(results);
            Assert.Single(results, x => x.ErrorMessage == "My_Error_Resource");
        }
    }

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
