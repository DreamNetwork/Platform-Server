﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DreamNetwork.PlatformServer.Logic
{
    static class Typing
    {
        public static Type FindEqualType(this IEnumerable<Type> types)
        {
            var typeArray = types.ToArray();

            if (!typeArray.Any())
                return null;

            var commonType = typeArray.Aggregate(typeArray.First(), (current, type) => current.FindEqualTypeWith(type));
            Debug.WriteLine(">>> {0} <<<", commonType);

            return commonType;
        }

        // provide common base class or implemented interface
        public static Type FindEqualTypeWith(this Type typeLeft, Type typeRight)
        {
            if (typeLeft == null || typeRight == null) return null;

            var commonBaseClass = typeLeft.FindBaseClassWith(typeRight) ?? typeof(object);

            return commonBaseClass == typeof(object)
                    ? typeLeft.FindInterfaceWith(typeRight)
                    : commonBaseClass;
        }

        // searching for common base class (either concrete or abstract)
        public static Type FindBaseClassWith(this Type typeLeft, Type typeRight)
        {
            if (typeLeft == null || typeRight == null) return null;

            return typeLeft
                    .GetClassHierarchy()
                    .Intersect(typeRight.GetClassHierarchy())
                    .FirstOrDefault(type => !type.IsInterface);
        }

        // searching for common implemented interface
        // it's possible for one class to implement multiple interfaces, 
        // in this case return first common based interface
        public static Type FindInterfaceWith(this Type typeLeft, Type typeRight)
        {
            if (typeLeft == null || typeRight == null) return null;

            return typeLeft
                    .GetInterfaceHierarchy()
                    .Intersect(typeRight.GetInterfaceHierarchy())
                    .FirstOrDefault();
        }

        // iterate on interface hierarhy
        public static IEnumerable<Type> GetInterfaceHierarchy(this Type type)
        {
            if (type.IsInterface) return new[] { type }.AsEnumerable();

            return type
                    .GetInterfaces()
                    .OrderByDescending(current => current.GetInterfaces().Count())
                    .AsEnumerable();
        }

        // interate on class hierarhy
        public static IEnumerable<Type> GetClassHierarchy(this Type type)
        {
            if (type == null) yield break;

            Type typeInHierarchy = type;

            do
            {
                yield return typeInHierarchy;
                typeInHierarchy = typeInHierarchy.BaseType;
            }
            while (typeInHierarchy != null && !typeInHierarchy.IsInterface);
        }
    }
}
