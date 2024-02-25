namespace LibraryCore.Shared;

internal static class ErrorMessages
{
    internal const string AotDynamicAccess = "DynamicBehavior is incompatible with trimming.";
    internal const string AotDynamicAccessUseOverload = "DynamicBehavior is incompatible with trimming. Use the JsonTypeInfo overload for aot support.";

    internal const string AotUnitTestTraitName = "CompileMode";
    internal const string AotUnitTestTraitValue = "Aot";
}