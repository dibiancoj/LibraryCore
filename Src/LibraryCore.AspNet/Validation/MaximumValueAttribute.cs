using System;
using System.ComponentModel.DataAnnotations;

namespace LibraryCore.AspNet.Validation
{
    public class MaximumValueAttribute : ValidationAttribute
    {
        public MaximumValueAttribute(double maximumValueAccepted)
            : this(maximumValueAccepted, false)
        {
        }

        public MaximumValueAttribute(double maximumValueAccepted, bool allowNulls)
        {
            MaximumValueAccepted = maximumValueAccepted;
            AllowNullValues = allowNulls;
        }

        private double MaximumValueAccepted { get; }
        private bool AllowNullValues { get; }

        public override bool IsValid(object? value)
        {
            if (value == null)
            {
                return AllowNullValues;
            }

            return Convert.ToDouble(value) <= MaximumValueAccepted;
        }
    }
}
