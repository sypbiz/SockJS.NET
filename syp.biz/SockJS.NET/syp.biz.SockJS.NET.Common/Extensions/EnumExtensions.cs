using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace syp.biz.SockJS.NET.Common.Extensions
{
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static class EnumExtensions
    {
        public static string? GetDescription<T>(this T @enum) where T : Enum
        {
            var attributes = @enum.GetType()
                .GetField(@enum.ToString())
                .GetCustomAttributes<DescriptionAttribute>(false);
            return attributes.FirstOrDefault()?.Description;
        }

        public static IDictionary<T, string?> GetMemberDescriptions<T>() where T : Enum
        {
            return typeof(T).GetFields()
                .ToDictionary(f => (T)f.GetValue(null), f => f.GetCustomAttributes<DescriptionAttribute>(false).FirstOrDefault()?.Description);
        }

        public static IDictionary<string, T> GetDescriptionMembers<T>() where T : Enum
        {
            return typeof(T).GetFields()
                .Select(field => (field, desc: field.GetCustomAttributes<DescriptionAttribute>(false).FirstOrDefault()?.Description))
                .Where(f => !(f.desc is null))
                .ToDictionary(f => f.desc!, f => (T)f.field.GetValue(null));
        }
    }
}
