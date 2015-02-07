using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using code2plan.DomainModel;

namespace code2plan.Tests
{
	[TestFixture]
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

		[SetUp]
		public void Setup()
		{
			this.locator = new ServiceLocator();
		}

		[Test]
		public void RegisterInterfaceDefaultCtor()
		{
			locator.Register<ITest>(typeof(Test));
		}

		[Test]
		public void RegisterInterfaceParameterizedCtor()
		{
			locator.Register<ITest>(typeof(Test), new TypeParameter(typeof(bool), true));
		}

		[Test]
		public void RegisterInterfaceDependentParameterizedCtor()
		{
			locator.Register<ICat>(typeof(Cat), typeof(IOwner));
		}

		[Test]
		public void RegisterInterfaceMixedParameterizedCtor()
		{
			locator.Register<ICat>(typeof(Cat), new TypeParameter(typeof(IOwner)), new TypeParameter(typeof(string), "Kitty"));
		}

		[Test]
		public void RegisterInterfaceCreator()
		{
			locator.Register<ICat>(delegate() { return new Cat(new Owner(), "Ashley"); });
		}

		[Test]
		public void RegisterInterfaceSingleton()
		{
			locator.Register<ICat>(new Cat(new Owner(), "Ashley"));
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void RegisterInterfaceInvalidType()
		{
			locator.Register<ICat>(typeof(Owner));
		}

		[Test]
		public void RegisterClassDefaultCtor()
		{
			locator.Register<Pet>(typeof(Dog));
		}

		[Test]
		public void RegisterClassParameterizedCtor()
		{
			locator.Register<Test>(typeof(SystemTest), new TypeParameter(typeof(bool), true));
		}

		[Test]
		public void RegisterClassDependentParameterizedCtor()
		{
			locator.Register<Pet>(typeof(Cat), typeof(IOwner));
		}

		[Test]
		public void RegisterClassMixedParameterizedCtor()
		{
			locator.Register<Pet>(typeof(Cat), new TypeParameter(typeof(IOwner)), new TypeParameter(typeof(string), "Kitty"));
		}

		[Test]
		public void RegisterClassCreator()
		{
			locator.Register<Test>(delegate() { return new SystemTest(true); });
		}

		[Test]
		public void RegisterClassSingleton()
		{
			locator.Register<Test>(new SystemTest(true));
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void RegisterClassInvalidType()
		{
			locator.Register<Pet>(typeof(Test));
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void RegisterDuplicate()
		{
			locator.Register<ITest>(typeof(Test));
			locator.Register<ITest>(typeof(Test));
		}

		[Test]
		public void IsRegisteredType()
		{
			locator.Register<ITest>(typeof(Test));
			Assert.IsTrue(locator.IsRegistered<ITest>());
		}

		[Test]
		public void IsRegisteredCreator()
		{
			locator.Register<Test>(delegate() { return new SystemTest(true); });
			Assert.IsTrue(locator.IsRegistered<Test>());
		}

		[Test]
		public void IsRegisteredSingleton()
		{
			locator.Register<Test>(new SystemTest(true));
			Assert.IsTrue(locator.IsRegistered<Test>());
		}

		[Test]
		public void IsRegisteredNotRegistered()
		{
			Assert.IsFalse(locator.IsRegistered<Test>());
		}

		[Test]
		public void UnregisterType()
		{
			locator.Register<Pet>(typeof(Dog));
			locator.Unregister<Pet>();
			Assert.IsFalse(locator.IsRegistered<Pet>());
		}

		[Test]
		public void UnregisterCreator()
		{
			locator.Register<Test>(delegate() { return new SystemTest(true); });
			locator.Unregister<Test>();
			Assert.IsFalse(locator.IsRegistered<Test>());
		}

		[Test]
		public void UnregisterSingleton()
		{
			locator.Register<Test>(new SystemTest(true));
			locator.Unregister<Test>();
			Assert.IsFalse(locator.IsRegistered<Test>());
		}

		[Test]
		public void UnregisterNotRegistered()
		{
			locator.Unregister<Test>();
			Assert.IsFalse(locator.IsRegistered<Test>());
		}

		[Test]
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

		[Test]
		public void UnregisterAllNotRegistered()
		{
			locator.UnregisterAll();
		}

		[Test]
		public void GetInterfaceDefaultCtor()
		{
			locator.Register<ITest>(typeof(Test));
			ITest instance = locator.Get<ITest>();
			Assert.IsNotNull(instance);
			Assert.IsTrue(instance is Test);
		}

		[Test]
		public void GetInterfaceParameterizedCtor()
		{
			locator.Register<ITest>(typeof(Test), new TypeParameter(typeof(bool), true));
			ITest instance = locator.Get<ITest>();
			Assert.IsNotNull(instance);
			Assert.IsTrue(instance is Test);
			Assert.IsTrue(instance.Property);
		}

		[Test]
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

		[Test]
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

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void GetInterfaceDependentParameterizedCtor_DependencyNotRegistered()
		{
			locator.Register<ICat>(typeof(Cat), typeof(IOwner));

			ICat instance = locator.Get<ICat>();
		}

		[Test]
		public void GetInterfaceCreator()
		{
			locator.Register<ICat>(delegate() { return new Cat(new Owner(), "Ashley"); });
			ICat cat = locator.Get<ICat>();
			Assert.IsNotNull(cat);
			Assert.IsTrue(cat is Cat);
			Assert.AreEqual("Ashley", cat.Name);
		}

		[Test]
		public void GetInterfaceSingleton()
		{
			Cat ashley = new Cat(new Owner(), "Ashley");
			locator.Register<ICat>(ashley);
			ICat cat = locator.Get<ICat>();
			Assert.AreSame(ashley, cat);
		}

		[Test]
		public void GetClassDefaultCtor()
		{
			locator.Register<Pet>(typeof(Dog));
			Pet pet = locator.Get<Pet>();
			Assert.IsNotNull(pet);
			Assert.IsTrue(pet is Dog);
		}

		[Test]
		public void GetClassParameterizedCtor()
		{
			locator.Register<Test>(typeof(SystemTest), new TypeParameter(typeof(bool), true));
			Test test = locator.Get<Test>();
			Assert.IsNotNull(test);
			Assert.IsTrue(test is SystemTest);
			Assert.IsTrue(test.Property);
		}

		[Test]
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

		[Test]
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

		[Test]
		public void GetClassCreator()
		{
			locator.Register<Test>(delegate() { return new SystemTest(true); });
			Test test = locator.Get<Test>();
			Assert.IsNotNull(test);
			Assert.IsTrue(test is SystemTest);
			Assert.IsTrue(test.Property);
		}

		[Test]
		public void GetClassSingleton()
		{
			SystemTest theTest = new SystemTest(true);
			locator.Register<Test>(theTest);
			Test test = locator.Get<Test>();
			Assert.AreSame(theTest, test);
		}

		[Test]
		public void Configuration()
		{
			Configuration config1 = Container.Configuration;
			Assert.IsNotNull(config1);
			Configuration config2 = Container.Configuration;
			Assert.AreSame(config1, config2);
		}
	}
}
