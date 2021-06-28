using LibraryCore.Core.ExtensionMethods;
using System;
using System.Linq;
using Xunit;

namespace LibraryCore.Tests.Core.ExtensionMethods
{
    public class ObjectExtensionMethodTest
    {

        #region Framework

        private record DummyObject(int Id = 1, string Text = "1");

        public abstract class MyObject
        {
            public const int MyValue = 1;

            public int MyValueGetter
            {
                get { return MyValue; }
            }
        }

        public class MyDerivedObject : MyObject
        {
        }

        #endregion

        #region Cast Unit Tests

        /// <summary>
        /// Try to convert a class to something else
        /// </summary>
        [Fact]
        public void ObjectCastTest1()
        {
            var objectToTest = new MyDerivedObject();

            Assert.Equal(MyObject.MyValue, objectToTest.Cast<MyObject>().MyValueGetter);
        }

        /// <summary>
        /// Try to convert a class to something that isn't castable
        /// </summary>
        [Fact]
        public void ObjectCastToNullTest1()
        {
            var objectToTest = new DummyObject();

            Assert.Throws<InvalidCastException>(() => objectToTest.Cast<MyObject>());
        }

        #endregion

        #region As Unit Tests

        /// <summary>
        /// Try to convert a class to something else
        /// </summary>
        [Fact]
        public void ObjectAsTest1()
        {
            var objectToTest = new MyDerivedObject();

            Assert.Equal(MyObject.MyValue, objectToTest.As<MyObject>().MyValueGetter);
        }

        /// <summary>
        /// Try to convert a class to something that isn't castable
        /// </summary>
        [Fact]
        public void ObjectAsToNullTest1()
        {
            var objectToTest = new DummyObject();

            Assert.Null(objectToTest.As<MyObject>()?.MyValueGetter);
        }

        #endregion

        #region Is Unit Tests

        /// <summary>
        /// Try to convert a class that is derived from another. Should return true
        /// </summary>
        [Fact]
        public void ObjectIsTest1()
        {
            Assert.True(new MyDerivedObject().Is<MyObject>());
        }

        /// <summary>
        /// Try to check a class that isn't castable. Should return false since it's not the same or derived type
        /// </summary>
        [Fact]
        public void ObjectIsToNullTest1()
        {
            Assert.False(new DummyObject().Is<MyObject>());
        }

        #endregion

        #region Single Object To Array Types

        /// <summary>
        /// Unit test to create an IEnumerable from a single object
        /// </summary>
        [Fact]
        public void SingleObjectToIEnumerableTest1()
        {
            //make sure we only have 1 record. This should prove it's in a form of ienumerable
            Assert.Single(new DummyObject().ToIEnumerableLazy());
        }

        /// <summary>
        /// Unit test to create an IList from a single object
        /// </summary>
        [Fact]
        public void SingleObjectToListTest1()
        {
            //grab a single record and push to an ienumerable
            var iListBuiltFromSingleObject = new DummyObject().ToIList();

            //make sure we only have 1 record. This should prove it's in a form of ienumerable
            Assert.Equal(1, iListBuiltFromSingleObject.Count);

            //add another record so we can make sure it increments
            iListBuiltFromSingleObject.Add(new DummyObject());

            //check the count
            Assert.Equal(2, iListBuiltFromSingleObject.Count);
        }

        #endregion

    }
}
