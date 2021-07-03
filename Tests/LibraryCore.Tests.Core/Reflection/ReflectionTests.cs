using LibraryCore.Core.Reflection;
using System.Collections.Generic;
using Xunit;

namespace LibraryCore.Tests.Core.Reflection
{
    public class ReflectionTests
    {

        #region Framework

        private record DummyObject(int Id, string Description);

        private class BaseDeriveReflectionClass
        {
            public int? NullIdProperty { get; set; }
            public List<string> IEnumerablePropertyTest { get; set; }
        }

        #endregion

        #region Property Info Is Nullable Of T

        /// <summary>
        /// Test that a nullable property is noted at run time in a dynamic - reflection manner
        /// </summary>
        [Fact]
        public void PropertyNullableOfTTest1()
        {
            //make sure it picks it up
            Assert.True(ReflectionUtilties.IsNullableOfT(typeof(BaseDeriveReflectionClass).GetProperty(nameof(BaseDeriveReflectionClass.NullIdProperty))));

            //test to make sure it doesn't pick these guys up
            Assert.False(ReflectionUtilties.IsNullableOfT(typeof(DummyObject).GetProperty(nameof(DummyObject.Id))));
            Assert.False(ReflectionUtilties.IsNullableOfT(typeof(DummyObject).GetProperty(nameof(DummyObject.Description))));
        }

        #endregion

        #region Property Info Is Collection

        /// <summary>
        /// Test that a property is a collection in a dynamic - reflection manner 
        /// </summary>
        [Fact]
        public void PropertyIsCollectionTest1()
        {
            //make sure it picks it up
            Assert.True(ReflectionUtilties.PropertyInfoIsIEnumerable(typeof(BaseDeriveReflectionClass).GetProperty(nameof(BaseDeriveReflectionClass.IEnumerablePropertyTest))));

            //test to make sure it doesn't pick these guys up
            Assert.False(ReflectionUtilties.PropertyInfoIsIEnumerable(typeof(DummyObject).GetProperty(nameof(DummyObject.Id))));
            Assert.False(ReflectionUtilties.PropertyInfoIsIEnumerable(typeof(DummyObject).GetProperty(nameof(DummyObject.Description))));
        }

        #endregion

    }
}
