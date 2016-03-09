using System;

namespace JoshCodes.Core.Extensions
{
	public static class AttributeExtensions
	{
		public static T GetCustomAttribute<T>(this Type type, bool inherit)
		{
			foreach(var attr in type.GetCustomAttributes(typeof(T), inherit))
			{
				if(typeof(T).IsAssignableFrom(attr.GetType()))
				{
					return (T)attr;
				}
			}
			return default(T);
		}
	}
}

