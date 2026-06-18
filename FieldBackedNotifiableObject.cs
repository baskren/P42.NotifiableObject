using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace P42.NotifiableObject
{
    public abstract class FieldBackedNotifiablePropertyObject : BaseNotifiablePropertyObject
    {

        protected virtual bool SetField<T>(ref T field, T value, 
            [CallerMemberName] string propertyName = "", 
            [CallerFilePath] string callerPath = "")
        {
            
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new InvalidDataContractException("null propertyName in SetField");
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            HasChanged = true;
            OnPropertyChanged(propertyName);
            return true;
        }

    }
}