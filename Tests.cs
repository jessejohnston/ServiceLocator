using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JesseJohnston;

namespace JesseJohnston.Tests
{
	[TestClass]
	public class ServiceLocatorTests
	{
		#region Types
		private interface ITest
		{
			bool Property { get; }
			void Method();
		}
		private interface IOwner
		{
			string Name { get; }
		}
		private interface ICat
		{
			string Name { get; }
			IOwner Owner { get; }
		}
		private class Test : ITest
		{
			private bool prop;

			public Test()
			{
			}
			public Test(bool propValue)
			{
				this.prop = propValue;
			}
			public bool Property { get { return prop; } }
			public void Method() { }
		}
		private class SystemTest : Test
		{
			public SystemTest(bool propValue) : base(propValue)
			{
			}
		}
		private class Owner : IOwner
		{
			private string name;

			public string Name
			{
				get { return this.name; }
				set { this.name = value; }
			}

			public Owner()
			{
			}
		}
		private class Pet
		{
			private string name;
			private IOwner owner;

			public string Name
			{
				get { return this.name; }
				set { this.name = value; }
			}
			public IOwner Owner
			{
				get { return this.owner; }
			}

			protected Pet(IOwner owner)
			{
				this.owner = owner;
			}
		}
		private class Cat : Pet, ICat
		{
			public Cat(IOwner owner) : base(owner)
			{
			}
			public Cat(IOwner owner, string name) : base(owner)
			{
				this.Name = name;
			}
			private Cat() : base(null)
			{
			}
		}
		private class Dog : Pet
		{
			public Dog() : base(null)
			{
			}
		}
		#endregion

		private ServiceLocator locator;

		[TestInitialize]
		public void Setup()
		{
			this.locator = new ServiceLocator();
		}

		[TestMethod]
		public void RegisterInterfaceDefaultCtor()
		{
			locator.Register<ITest>(typeof(Test));
		}

		[TestMethod]
		public void RegisterInterfaceParameterizedCtor()
		{
			locator.Register<ITest>(typeof(Test), new TypeParameter(typeof(bool), true));
		}

		[TestMethod]
		public void RegisterInterfaceDependentParameterizedCtor()
		{
			locator.Register<ICat>(typeof(Cat), typeof(IOwner));
		}

		[TestMethod]
		public void RegisterInterfaceMixedParameterizedCtor()
		{
			locator.Register<ICat>(typeof(Cat), new TypeParameter(typeof(IOwner)), new TypeParameter(typeof(string), "Kitty"));
		}

		[TestMethod]
		public void RegisterInterfaceCreator()
		{
			locator.Register<ICat>(delegate() { return new Cat(new Owner(), "Ashley"); });
		}

		[TestMethod]
		public void RegisterInterfaceSingleton()
		{
			locator.Register<ICat>(new Cat(new Owner(), "Ashley"));
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void RegisterInterfaceInvalidType()
		{
			locator.Register<ICat>(typeof(Owner));
		}

		[TestMethod]
		public void RegisterClassDefaultCtor()
		{
			locator.Register<Pet>(typeof(Dog));
		}

		[TestMethod]
		public void RegisterClassParameterizedCtor()
		{
			locator.Register<Test>(typeof(SystemTest), new TypeParameter(typeof(bool), true));
		}

		[TestMethod]
		public void RegisterClassDependentParameterizedCtor()
		{
			locator.Register<Pet>(typeof(Cat), typeof(IOwner));
		}

		[TestMethod]
		public void RegisterClassMixedParameterizedCtor()
		{
			locator.Register<Pet>(typeof(Cat), new TypeParameter(typeof(IOwner)), new TypeParameter(typeof(string), "Kitty"));
		}

		[TestMethod]
		public void RegisterClassCreator()
		{
			locator.Register<Test>(delegate() { return new SystemTest(true); });
		}

		[TestMethod]
		public void RegisterClassSingleton()
		{
			locator.Register<Test>(new SystemTest(true));
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void RegisterClassInvalidType()
		{
			locator.Register<Pet>(typeof(Test));
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void RegisterDuplicate()
		{
			locator.Register<ITest>(typeof(Test));
			locator.Register<ITest>(typeof(Test));
		}

		[TestMethod]
		public void IsRegisteredType()
		{
			locator.Register<ITest>(typeof(Test));
			Assert.IsTrue(locator.IsRegistered<ITest>());
		}

		[TestMethod]
		public void IsRegisteredCreator()
		{
			locator.Register<Test>(delegate() { return new SystemTest(true); });
			Assert.IsTrue(locator.IsRegistered<Test>());
		}

		[TestMethod]
		public void IsRegisteredSingleton()
		{
			locator.Register<Test>(new SystemTest(true));
			Assert.IsTrue(locator.IsRegistered<Test>());
		}

		[TestMethod]
		public void IsRegisteredNotRegistered()
		{
			Assert.IsFalse(locator.IsRegistered<Test>());
		}

		[TestMethod]
		public void UnregisterType()
		{
			locator.Register<Pet>(typeof(Dog));
			locator.Unregister<Pet>();
			Assert.IsFalse(locator.IsRegistered<Pet>());
		}

		[TestMethod]
		public void UnregisterCreator()
		{
			locator.Register<Test>(delegate() { return new SystemTest(true); });
			locator.Unregister<Test>();
			Assert.IsFalse(locator.IsRegistered<Test>());
		}

		[TestMethod]
		public void UnregisterSingleton()
		{
			locator.Register<Test>(new SystemTest(true));
			locator.Unregister<Test>();
			Assert.IsFalse(locator.IsRegistered<Test>());
		}

		[TestMethod]
		public void UnregisterNotRegistered()
		{
			locator.Unregister<Test>();
			Assert.IsFalse(locator.IsRegistered<Test>());
		}

		[TestMethod]
		public void UnregisterAll()
		{
			locator.Register<Pet>(typeof(Dog));
			locator.Register<Test>(delegate() { return new SystemTest(true); });
			locator.Register<ICat>(new Cat(new Owner(), "Ashley"));
			locator.UnregisterAll();
			Assert.IsFalse(locator.IsRegistered<Pet>());
			Assert.IsFalse(locator.IsRegistered<Test>());
			Assert.IsFalse(locator.IsRegistered<ICat>());
		}

		[TestMethod]
		public void UnregisterAllNotRegistered()
		{
			locator.UnregisterAll();
		}

		[TestMethod]
		public void GetInterfaceDefaultCtor()
		{
			ServiceLocator locator = new ServiceLocator();
			locator.Register<ITest>(typeof(Test));
			ITest instance = locator.Get<ITest>();
			Assert.IsNotNull(instance);
			Assert.IsTrue(instance is Test);
		}

		[TestMethod]
		public void GetInterfaceParameterizedCtor()
		{
			locator.Register<ITest>(typeof(Test), new TypeParameter(typeof(bool), true));
			ITest instance = locator.Get<ITest>();
			Assert.IsNotNull(instance);
			Assert.IsTrue(instance is Test);
			Assert.IsTrue(instance.Property);
		}

		[TestMethod]
		public void GetInterfaceDependentParameterizedCtor()
		{
			locator.Register<ICat>(typeof(Cat), typeof(IOwner));
			locator.Register<IOwner>(typeof(Owner));

			ICat instance = locator.Get<ICat>();
			Assert.IsNotNull(instance);
			Assert.IsTrue(instance is Cat);
			Assert.IsNotNull(instance.Owner);
			Assert.IsTrue(instance.Owner is Owner);
		}

		[TestMethod]
		public void GetInterfaceMixedParameterizedCtor()
		{
			locator.Register<IOwner>(typeof(Owner));
			locator.Register<ICat>(typeof(Cat), new TypeParameter(typeof(IOwner)), new TypeParameter(typeof(string), "Kitty"));
			ICat cat = locator.Get<ICat>();
			Assert.IsNotNull(cat);
			Assert.IsTrue(cat is Cat);
			Assert.IsNotNull(cat.Owner);
			Assert.IsTrue(cat.Owner is Owner);
			Assert.AreEqual(cat.Name, "Kitty");
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void GetInterfaceDependentParameterizedCtor_DependencyNotRegistered()
		{
			locator.Register<ICat>(typeof(Cat), typeof(IOwner));

			ICat instance = locator.Get<ICat>();
		}

		[TestMethod]
		public void GetInterfaceCreator()
		{
			locator.Register<ICat>(delegate() { return new Cat(new Owner(), "Ashley"); });
			ICat cat = locator.Get<ICat>();
			Assert.IsNotNull(cat);
			Assert.IsTrue(cat is Cat);
			Assert.AreEqual("Ashley", cat.Name);
		}

		[TestMethod]
		public void GetInterfaceSingleton()
		{
			Cat ashley = new Cat(new Owner(), "Ashley");
			locator.Register<ICat>(ashley);
			ICat cat = locator.Get<ICat>();
			Assert.AreSame(ashley, cat);
		}

		[TestMethod]
		public void GetClassDefaultCtor()
		{
			locator.Register<Pet>(typeof(Dog));
			Pet pet = locator.Get<Pet>();
			Assert.IsNotNull(pet);
			Assert.IsTrue(pet is Dog);
		}

		[TestMethod]
		public void GetClassParameterizedCtor()
		{
			locator.Register<Test>(typeof(SystemTest), new TypeParameter(typeof(bool), true));
			Test test = locator.Get<Test>();
			Assert.IsNotNull(test);
			Assert.IsTrue(test is SystemTest);
			Assert.IsTrue(test.Property);
		}

		[TestMethod]
		public void GetClassDependentParameterizedCtor()
		{
			locator.Register<IOwner>(typeof(Owner));
			locator.Register<Pet>(typeof(Cat), typeof(IOwner));
			Pet pet = locator.Get<Pet>();
			Assert.IsNotNull(pet);
			Assert.IsTrue(pet is Cat);
			Assert.IsNotNull(pet.Owner);
			Assert.IsTrue(pet.Owner is Owner);
		}

		[TestMethod]
		public void GetClassMixedParameterizedCtor()
		{
			locator.Register<Pet>(typeof(Cat), new TypeParameter(typeof(IOwner)), new TypeParameter(typeof(string), "Kitty"));
			locator.Register<IOwner>(typeof(Owner));
			Pet pet = locator.Get<Pet>();
			Assert.IsNotNull(pet);
			Assert.IsTrue(pet is Cat);
			Assert.IsNotNull(pet.Owner);
			Assert.AreEqual(pet.Name, "Kitty");
		}

		[TestMethod]
		public void GetClassCreator()
		{
			locator.Register<Test>(delegate() { return new SystemTest(true); });
			Test test = locator.Get<Test>();
			Assert.IsNotNull(test);
			Assert.IsTrue(test is SystemTest);
			Assert.IsTrue(test.Property);
		}

		[TestMethod]
		public void GetClassSingleton()
		{
			SystemTest theTest = new SystemTest(true);
			locator.Register<Test>(theTest);
			Test test = locator.Get<Test>();
			Assert.AreSame(theTest, test);
		}
	}
}
