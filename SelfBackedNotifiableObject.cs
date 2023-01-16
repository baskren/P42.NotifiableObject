using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Dynamic;
using Newtonsoft.Json;

namespace P42.NotifiableObject
{
    public abstract class SelfBackedNotifiablePropertyObject : BaseNotifiablePropertyObject
    {
        [JsonIgnore]
        private Dictionary<string, object> ObjectStore = new Dictionary<string, object>();

        protected T GetValue<T>([CallerMemberName] string propertyName = null)
        {
            if (ObjectStore.TryGetValue(propertyName, out object value))
                return (T)value;
            return default;
        }

        protected T GetValue<T>(T defaultValue = default, [CallerMemberName] string propertyName = null)
        {
            if (ObjectStore.TryGetValue(propertyName, out object value))
                return (T)value;
            return defaultValue;
        }

        protected bool SetValue<T>(T value, [CallerMemberName] string propertyName = null)
        {
            var current = GetValue<T>(propertyName);
            if (EqualityComparer<T>.Default.Equals(current, value))
                return false;

            ObjectStore[propertyName] = value;

            HasChanged = true;
            OnPropertyChanged(propertyName);
            return true;
        }

    }

}