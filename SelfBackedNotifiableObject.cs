using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace P42.NotifiableObject
{
    public abstract class SelfBackedNotifiablePropertyObject : BaseNotifiablePropertyObject
    {
        [JsonIgnore]
        private readonly System.Threading.Lock _valueLock = new ();
        
        [JsonIgnore]
        private readonly ConcurrentDictionary<string, object> _objectStore = new();

        protected T? GetValue<T>(T? defaultValue = default, [CallerMemberName] string propertyName = "")
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                return defaultValue;    
            
            if (_objectStore.TryGetValue(propertyName, out var value))
                return (T)value;
            return defaultValue;
        }

        protected bool SetValue<T>(T value, [CallerMemberName] string propertyName = "")
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                return false;    
            
            lock (_valueLock)
            {
                if (EqualityComparer<T>.Default.Equals(GetValue<T>(default, propertyName), value))
                    return false;

                _objectStore[propertyName] = value!;

                HasChanged = true;
                OnPropertyChanged(propertyName);
                return true;
            }        
        }

    }

}