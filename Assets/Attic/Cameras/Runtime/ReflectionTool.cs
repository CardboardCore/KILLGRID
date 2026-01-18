using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Attic.Utils
{
	public static class ReflectionTools
	{
		public static List<Type> GetDerivedTypes<T>()
		{
			return GetDerivedTypes<T>(AppDomain.CurrentDomain.GetAssemblies());
		}

		public static List<Type> GetDerivedTypes<T>(Assembly[] assemblies)
		{
			List<Type> derivedTypes = new List<Type>();
			foreach(Assembly assembly in assemblies)
			{
				try
				{
					derivedTypes.AddRange(assembly.GetTypes().Where(type => typeof(T).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract));
				}
				catch(Exception exc)
				{
					Debug.LogError(exc);
				}
			}

			return derivedTypes;
		}

		public static List<T> GetDerivedInstances<T>(List<Type> derivedTypes)
		{
			return derivedTypes.Select(type => (T) Activator.CreateInstance(type)).ToList();
		}

		public static List<T> GetDerivedInstances<T>(Assembly[] assemblies)
		{
			return GetDerivedInstances<T>(GetDerivedTypes<T>(assemblies));
		}

		public static List<T> GetDerivedInstances<T>()
		{
			return GetDerivedInstances<T>(GetDerivedTypes<T>(AppDomain.CurrentDomain.GetAssemblies()));
		}

		public static List<Assembly> GetAssemblies(List<string> assemblyNames)
		{
			List<Assembly> assemblies = new List<Assembly>();
			foreach(string assemblyName in assemblyNames)
			{
				if(TryGetAssembly(assemblyName, out Assembly assembly))
				{
					assemblies.Add(assembly);
				}
			}

			return assemblies;
		}

		public static bool TryGetAssembly(string name, out Assembly assembly)
		{
			foreach(Assembly ass in AppDomain.CurrentDomain.GetAssemblies())
			{
				if(ass.FullName.Substring(0, ass.FullName.IndexOf(",", StringComparison.Ordinal)).Equals(name))
				{
					assembly = ass;
					return true;
				}
			}

			assembly = null;
			return false;
		}

		public static FieldInfo[] GetFieldsWithAttribute<T>(Type type) where T : Attribute
		{
			FieldInfo[] fields = GetFields(type);

			List<FieldInfo> fieldInfoList = new List<FieldInfo>();

			foreach(FieldInfo fieldInfo in fields)
			{
				T attribute = fieldInfo.GetCustomAttribute<T>();

				if(attribute == null)
				{
					continue;
				}

				fieldInfoList.Add(fieldInfo);
			}

			if(type.BaseType != null)
			{
				fieldInfoList.AddRange(GetFieldsWithAttribute<T>(type.BaseType));
			}

			return fieldInfoList.ToArray();
		}

		public static FieldInfo GetFieldWithName(object obj, string name)
		{
			Type type = obj.GetType();

			FieldInfo[] fields = GetFields(type);

			foreach(FieldInfo fieldInfo in fields)
			{
				if(fieldInfo.Name.Equals(name))
				{
					return fieldInfo;
				}
			}

			return null;
		}

		public static FieldInfo[] GetFields(Type type)
		{
			return type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
		}

		public static Type[] GetTypesFilterAbstract<T>(Assembly assemblyFilter = null)
		{
			List<Type> types = GetTypes<T>(assemblyFilter).ToList();

			for(int i = types.Count - 1; i >= 0; i--)
			{
				if(types[i].IsAbstract)
				{
					types.RemoveAt(i);
				}
			}

			return types.ToArray();
		}

		public static Type[] GetTypes<T>(Assembly assemblyFilter = null)
		{
			Type type = typeof(T);

			List<Type> typeList = new List<Type>();

			// If no assembly was given as filter, search through all assemblies
			if(assemblyFilter == null)
			{
				Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

				foreach(Assembly assembly in assemblies)
				{
					try
					{
						Type[] types = assembly.GetTypes().Where(t => type.IsAssignableFrom(t)).ToArray();
						typeList.AddRange(types);
					}
					catch(ReflectionTypeLoadException ex)
					{
						throw new Exception($"Error while loading types for domain {assembly.FullName}: {ex.Message}");
					}
				}
			}
			else
			{
				Type[] types = assemblyFilter.GetTypes().Where(t => type.IsAssignableFrom(t)).ToArray();
				typeList.AddRange(types);
			}

			return typeList.ToArray();
		}
	}
}
