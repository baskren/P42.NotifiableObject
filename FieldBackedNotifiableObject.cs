using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Dynamic;
using Newtonsoft.Json;

namespace P42.NotifiableObject
{
    public abstract class FieldBackedNotifiablePropertyObject : BaseNotifiablePropertyObject
    {

        protected virtual bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null, [CallerFilePath] string callerPath = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            if (propertyName == null)
                throw new InvalidDataContractException("null propertyName in SetField");

            field = value;
            HasChanged = true;
            OnPropertyChanged(propertyName);
            return true;
        }

    }
}