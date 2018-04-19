using Moq;
using NUnit.Framework;
using PolyPaint.Models;
using PolyPaint.Services.Auth;
using PolyPaint.Tests.Util;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PolyPaint.Tests.Services.Database
{
    class MockDatabaseTests
    {
        private MockDatabaseService Database { get; set; }

        [SetUp]
        public void SetUp()
        {
            Database = new MockDatabaseService();
        }

        [Test]
        public async Task Push_ShouldCreateKey()
        {
            // Act
            var pushQuery = await Database.Ref("A").Child("B").Child("C").Push();

            // Assert
            Assert.False(String.IsNullOrWhiteSpace(pushQuery.Key));
        }

        [Test]
        public async Task PushAndSet_ShouldSetInDatabase()
        {
            // Arrange
            var expectedValue = "MyValue";

            // Act
            var pushQuery = await Database.Ref("A").Child("B").Child("C").Push();
            var key = pushQuery.Key;
            await pushQuery.Set("MyValue");

            // Assert
            var actualValue = await Database.Ref("A").Child("B").Child("C").Child(key).Once<string>();
            Assert.AreEqual(actualValue, expectedValue);
        }

        [Test]
        public async Task Set_ShouldSetDataInDb()
        {
            // Arrange
            var expected = "MyValue";

            // Act
            await Database.Ref("A").Child("B").Child("C").Set(expected);
            var actual = await Database.Ref("A").Child("B").Child("C").Once<string>();

            // Assert
            Assert.AreEqual(actual, expected);
        }

        [Test]
        [Timeout(5000)]
        public async Task ChildAdded_ShouldCallCallback()
        {
            // Arrange
            var wasCalled = false;
            var expected = "MyValue";
            var semaphore = new Semaphore(0, 1);

            // Act
            Database.Ref("A").Child("B").OnChildAdded<string>((actual) =>
            {
                Assert.AreEqual(actual, expected);
                semaphore.Release();
                wasCalled = true;
            });

            await Database.Ref("A").Child("B").Child("C").Set(expected);

            // Assert
            semaphore.WaitOne();
            Assert.IsTrue(wasCalled);
        }

        [Test]
        [Timeout(5000)]
        public async Task ChildChanged_ShouldCallCallback()
        {
            // Arrange
            var wasCalled = false;
            var semaphore = new Semaphore(0, 1);

            // Act
            Database.Ref("A").Child("People").OnChildChanged<Person>((actual) =>
            {
                Constants.Person1.Age = 5;
                Assert.AreEqual(actual, Constants.Person1);
                semaphore.Release();
                wasCalled = true;
            });

            await Database.Ref("A").Child("People").Child("person1").Set(Constants.Person1);
            await Database.Ref("A").Child("People").Child("person1").Child("Age").Set(5);

            // Assert
            semaphore.WaitOne();
            Assert.IsTrue(wasCalled);
        }

        [Test]
        [Timeout(5000)]
        public async Task ValueChanged_ShouldCallCallback()
        {
            // Arrange
            var wasCalled = false;
            var semaphore = new Semaphore(0, 1);

            // Act
            await Database.Ref("A").Child("People").Child("person1").Set(Constants.Person1);

            Database.Ref("A").Child("People").Child("person1").Child("Age").OnValue<int>((actual) =>
            {
                Assert.AreEqual(actual, 5);
                semaphore.Release();
                wasCalled = true;
            });

            await Database.Ref("A").Child("People").Child("person1").Child("Age").Set(5);

            // Assert
            semaphore.WaitOne();
            Assert.IsTrue(wasCalled);
        }

        [Test]
        [Timeout(5000)]
        public async Task ChildRemoved_ShouldCallCallback()
        {
            // Arrange
            var wasCalled = false;
            var semaphore = new Semaphore(0, 1);

            // Act

            Database.Ref("A").Child("People").OnChildRemoved<Person>((actual) =>
            {
                Assert.AreEqual(actual, Constants.Person1);
                semaphore.Release();
                wasCalled = true;
            });

            await Database.Ref("A").Child("People").Child("person1").Set(Constants.Person1);
            await Database.Ref("A").Child("People").Child("person1").Set<Person>(null);

            // Assert
            semaphore.WaitOne();
            Assert.IsTrue(wasCalled);
        }

        [Test]
        public async Task Once_ChildQueryExistsButDataEmpty_ShouldReturnNull()
        {
            // Act
            var refa = Database.Ref("A").Child("B").Child("C");
            var value = await Database.Ref("A").Child("B").Child("C").Once<object>();

            // Assert
            Assert.IsNull(value);
        }

        private static class Constants
        {
            public static readonly Animal Animal1 = new Animal(Specie.Sloth, "Slothy Slotherson");
            public static readonly Person Person1 = new Person("Testy Testerson", 34, Animal1);
        }
    }
}
