using System;
using System.Collections.Generic;
using System.Reflection;

namespace JesseJohnston
{
	/// <summary>
	/// Simple service locator.  Use this class to create instances of concrete types for interfaces or abstract classes.
	/// </summary>
	public class ServiceLocator
	{
		#region Types
		private class ConstructionInfo
		{
			private Type typeToCreate;
			private TypeParameter[] parms;

			public TypeParameter[] Parameters
			{
				get { return this.parms; }
			}
			public Type[] ParameterTypes
			{
				get
				{
					Type[] types = new Type[parms.Length];
					for (int i = 0; i < parms.Length; i++)
						types[i] = parms[i].Type;
					return types;
				}
			}
			public object[] ParameterValues
			{
				get
				{
					object[] values = new object[parms.Length];
					for (int i = 0; i < parms.Length; i++)
						values[i] = parms[i].Value;
					return values;
				}
			}
			public Type Type
			{
				get { return typeToCreate; }
			}

			public ConstructionInfo(Type instantiatedType, params TypeParameter[] parameters)
			{
				if (instantiatedType == null)
					throw new ArgumentNullException("instantiatedType");
				this.typeToCreate = instantiatedType;
				this.parms = parameters;
			}
			public ConstructionInfo(Type instantiatedType, params Type[] parameterTypes)
			{
				if (instantiatedType == null)
					throw new ArgumentNullException("instantiatedType");
				this.typeToCreate = instantiatedType;
				this.parms = new TypeParameter[parameterTypes.Length];
				for (int i = 0; i < parameterTypes.Length; i++)
					this.parms[i] = new TypeParameter(parameterTypes[i]);
			}
		}
		#endregion

		#region Fields
		private Dictionary<Type, object> instances = new Dictionary<Type, object>();
		private Dictionary<Type, ConstructionInfo> ctorParmTypes = new Dictionary<Type, ConstructionInfo>();
		private Dictionary<Type, Creator> creators = new Dictionary<Type, Creator>();
		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="ServiceLocator"/> class.
		/// </summary>
		public ServiceLocator()
		{
		}

		/// <summary>
		/// Registers a type that can be used to manufacture a specific type to be retrieved later with a call to <see cref="Get"/>.
		/// </summary>
		/// <remarks>
		/// When an object of the specified type is retrieved later, it will be created using the default constructor.
		/// </remarks>
		/// <typeparam name="T">Type to register.</typeparam>
		/// <param name="instantiatedType">The concrete type of the objects that will be created.</param>
		public void Register<T>(Type instantiatedType) where T : class
		{
			Type t = typeof(T);
			RegisterTypeChecks(t, instantiatedType);
			this.ctorParmTypes.Add(t, new ConstructionInfo(instantiatedType, new TypeParameter[0]));
		}

		/// <summary>
		/// Registers a type and a set of parameter types to specify a constructor for that type that can be used to manufacture a specific type
		/// to be retrieved later with a call to <see cref="Get"/>.
		/// </summary>
		/// <remarks>
		/// When an object of the specified type is retrieved later, it will be created using the type and constructor type parameters provided.
		/// </remarks>
		/// <typeparam name="T">Type to register.</typeparam>
		/// <param name="instantiatedType">The concrete type of the objects that will be created.</param>
		/// <param name="constructorParameters">The constructor parameter types.</param>
		public void Register<T>(Type instantiatedType, params Type[] constructorParameters) where T : class
		{
			Type t = typeof(T);
			RegisterTypeChecks(t, instantiatedType);
			this.ctorParmTypes.Add(t, new ConstructionInfo(instantiatedType, constructorParameters));
		}

		/// <summary>
		/// Registers a type and a set of parameter types to specify a constructor for that type that can be used to manufacture a specific type
		/// to be retrieved later with a call to <see cref="Get"/>.
		/// </summary>
		/// <remarks>
		/// When an object of the specified type is retrieved later, it will be created using the type and constructor type parameters provided.
		/// Note that each <see cref="TypeParameter"/> may have a preset value.  This is useful when specifying a value type parameter or other
		/// parameter not registered with the container.
		/// </remarks>
		/// <typeparam name="T">Type to register.</typeparam>
		/// <param name="instantiatedType">The concrete type of the objects that will be created.</param>
		/// <param name="constructorParameters">The constructor parameter types.</param>
		public void Register<T>(Type instantiatedType, params TypeParameter[] constructorParameters) where T : class
		{
			Type t = typeof(T);
			RegisterTypeChecks(t, instantiatedType);
			this.ctorParmTypes.Add(t, new ConstructionInfo(instantiatedType, constructorParameters));
		}

		/// <summary>
		/// Registers a creator method to use to manufacture an object of a specified type that can be retrieved later with a call to <see cref="Get"/>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="method">The method that will be called to create objects.</param>
		public void Register<T>(Creator method) where T : class
		{
			if (method == null)
				throw new ArgumentNullException("method");
			Type t = typeof(T);
			if (IsRegistered(t))
				throw new ArgumentException("Type '" + t.Name + "' is already registered.");
			this.creators.Add(t, method);
		}

		/// <summary>
		/// Registers an object of a specified type that can be retrieved later with a call to <see cref="Get"/>.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="singleton">The singleton object to return.</param>
		public void Register<T>(T singleton) where T : class
		{
			if (singleton == null)
				throw new ArgumentNullException("singleton");
			Type t = typeof(T);
			if (IsRegistered(t))
				throw new ArgumentException("Type '" + t.Name + "' is already registered.");
			this.instances.Add(t, singleton);
		}

		/// <summary>
		/// Determines whether a type is registered.
		/// </summary>
		/// <typeparam name="T">The type.</typeparam>
		/// <returns>
		/// 	<c>true</c> if the type is registered; otherwise, <c>false</c>.
		/// </returns>
		public bool IsRegistered<T>() where T : class
		{
			return IsRegistered(typeof(T));
		}

		/// <summary>
		/// Unregisters the type.
		/// </summary>
		/// <typeparam name="T">The type to unregister.</typeparam>
		public void Unregister<T>() where T : class
		{
			Type t = typeof(T);
			this.instances.Remove(t);
			this.ctorParmTypes.Remove(t);
			this.creators.Remove(t);
		}

		/// <summary>
		/// Unregisters all types.
		/// </summary>
		public void UnregisterAll()
		{
			this.instances.Clear();
			this.ctorParmTypes.Clear();
			this.creators.Clear();
		}

		/// <summary>
		/// Gets or creates an instance of the specified type.
		/// </summary>
		/// <typeparam name="T">The type of object to create.</typeparam>
		/// <returns>The created object</returns>
		public T Get<T>() where T : class
		{
			return (T)Get(typeof(T));
		}

		private object Get(Type t)
		{
			if (this.instances.ContainsKey(t))
				return this.instances[t];
			else if (this.creators.ContainsKey(t))
				return this.creators[t]();
			else if (this.ctorParmTypes.ContainsKey(t))
				return Create(t, this.ctorParmTypes[t]);
			else
				throw new InvalidOperationException("Type '" + t.Name + "' has not been registered.");
		}
		private object Create(Type typeToCreate, ConstructionInfo info)
		{
			Type instantiatedType = info.Type;
			object[] parms = info.ParameterValues;

			// For each constructor parameter that was not provided in the call to Register,
			// retrive an object of the parameter type.
			for (int i = 0; i < info.Parameters.Length; i++)
			{
				if (!info.Parameters[i].IsValueSet)
					parms[i] = Get(info.Parameters[i].Type);
			}

			ConstructorInfo ctor = instantiatedType.GetConstructor(info.ParameterTypes);
			if (ctor == null)
				throw new InvalidOperationException("Constructor for type '" + instantiatedType.Name + "' was not found.");

			return ctor.Invoke(parms);
		}
		private void RegisterTypeChecks(Type typeToCreate, Type instantiatedType)
		{
			if (!typeToCreate.IsAssignableFrom(instantiatedType))
				throw new ArgumentException("Type '" + instantiatedType.Name + "' is not derived from '" + typeToCreate.Name + "'.");
			if (IsRegistered(typeToCreate))
				throw new ArgumentException("Type '" + typeToCreate.Name + "' is already registered.");
		}
		private bool IsRegistered(Type t)
		{
			return this.instances.ContainsKey(t) || this.ctorParmTypes.ContainsKey(t) || this.creators.ContainsKey(t);
		}
	}

	/// <summary>
	/// A method that creates an instance of some type.
	/// </summary>
	public delegate object Creator();

	/// <summary>
	/// Represents the type of a constructor parameter, and optionally a value for the parameter.
	/// </summary>
	public class TypeParameter
	{
		private Type paramType;
		private object paramValue;
		private bool valueSet;

		/// <summary>
		/// Gets a value indicating whether the parameter has a provided value.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if a value has been provided; otherwise, <c>false</c>.
		/// </value>
		public bool IsValueSet
		{
			get { return this.valueSet; }
		}

		/// <summary>
		/// Gets the type of the parameter.
		/// </summary>
		/// <value>The type.</value>
		public Type Type
		{
			get { return this.paramType; }
		}

		/// <summary>
		/// Gets the value (if provided), or null.
		/// </summary>
		/// <value>The value.</value>
		public object Value
		{
			get { return this.paramValue; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TypeParameter"/> class.
		/// </summary>
		/// <param name="parameterType">Type of the parameter.</param>
		public TypeParameter(Type parameterType)
		{
			if (parameterType == null)
				throw new ArgumentNullException("parameterType");
			this.paramType = parameterType;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TypeParameter"/> class.
		/// </summary>
		/// <param name="parameterType">Type of the parameter.</param>
		/// <param name="value">The parameter value.</param>
		public TypeParameter(Type parameterType, object value)
		{
			if (parameterType == null)
				throw new ArgumentNullException("parameterType");
			this.paramType = parameterType;

			if (value != null && !parameterType.IsAssignableFrom(value.GetType()))
				throw new ArgumentException("value is not a '" + parameterType.Name + "'.", "value");
			this.paramValue = value;
			this.valueSet = true;
		}
	}
}
