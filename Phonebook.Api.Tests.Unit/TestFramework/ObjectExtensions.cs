using System;
using System.Reflection;

namespace Phonebook.Api.Tests.Unit.TestFramework
{
    public static class ObjectExtensions
    {
        public static void SetPrivateProperty(this object obj, string propertyName, object value)
        {
            obj.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).SetValue(obj, value, null);
        }

        public static T WithPrivatePropertSet<T>(this T obj, string propertyName, object value)
        {
            obj.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).SetValue(obj, value, null);

            return obj;
        }

        public static T WithIdSetToRandomInteger<T>(this T obj)
        {
            var randomInteger = new Random().Next();
            return obj.WithPrivatePropertSet("Id", randomInteger);
        }
    }
}
