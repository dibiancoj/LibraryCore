using System;
using System.ComponentModel.DataAnnotations;

namespace LibraryCore.AspNet.Validation;

public class DateOfBirthRangeAttribute : RangeAttribute
{
    // We are working around a Data Annotation / DateTime limitation so we can establish a reasonable DateTime range.
    public DateOfBirthRangeAttribute()
        : base(typeof(DateTime), DateTime.Now.AddYears(-200).ToShortDateString(), DateTime.Now.AddYears(1).ToShortDateString())
    {
    }
}
