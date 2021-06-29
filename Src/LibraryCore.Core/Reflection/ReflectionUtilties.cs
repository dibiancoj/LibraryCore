using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace LibraryCore.Core.Reflection
{
    [ExcludeFromCodeCoverage]
    public static class ReflectionUtilties
    {

        /// <summary>
        /// Scans assemblies to find all instances of TInterface ie: register all classes that implement IRepository
        /// </summary>
        /// <typeparam name="TInterface">Interface type for the types that you want to register</typeparam>
        /// <returns>All the types that implement that interface</returns>
        public static IEnumerable<TypeInfo> ScanForAllInstancesOfType<TInterface>()
        {
            //NOT TESTABLE - No App Domain In Unit Test Project
            return (Assembly.GetEntryAssembly() ?? throw new Exception("No Entry Assembly"))
                   .GetReferencedAssemblies()
                   .Select(Assembly.Load)
                   .SelectMany(x => x.DefinedTypes)
                   .Where(x => x.ImplementedInterfaces.Contains(typeof(TInterface)));
        }

    }
}
