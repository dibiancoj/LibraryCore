﻿using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using LibraryCore.Shared;

namespace LibraryCore.AspNet.Validation;

[RequiresUnreferencedCode(ErrorMessages.AotDynamicAccess)]
public class DateOfBirthRangeAttribute : RangeAttribute
{
    // We are working around a Data Annotation / DateTime limitation so we can establish a reasonable DateTime range.
    public DateOfBirthRangeAttribute()
        : base(typeof(DateTime), DateTime.Now.AddYears(-125).ToShortDateString(), DateTime.Today.AddDays(1).ToShortDateString())
    {
    }
}
