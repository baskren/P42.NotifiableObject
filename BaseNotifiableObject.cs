using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Dynamic;
using Newtonsoft.Json;

namespace P42.NotifiableObject
{
    public abstract class BaseNotifiablePropertyObject : INotifyPropertyChanged
    {
        [JsonIgnore]
        public static long Instances { get; private set; }

        [JsonIgnore]
        public long InstanceId { get; private set; }

        [JsonIgnore]
        public virtual bool Logging { get; }

        protected List<string> BatchedPropertyChanges = new List<string>();

        int _batchChanges;
        [JsonIgnore]
        public bool BatchChanges
        {
            get => _batchChanges > 0;
            set
            {
                if (value)
                    _batchChanges++;
                else
                    _batchChanges--;
                _batchChanges = Math.Max(0, _batchChanges);
                if (_batchChanges == 0)
                {
                    var propertyNames = new List<string>(BatchedPropertyChanges);
                    BatchedPropertyChanges.Clear();
                    foreach (var propertyName in propertyNames)
                        OnPropertyChanged(propertyName);
                }
            }
        }

        [JsonIgnore]
        public bool HasChanged { get; set; }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
            => HasChanged = false;


        internal BaseNotifiablePropertyObject()
        {
            InstanceId = Instances++;
        }

        #region Property Change Handler
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (BatchChanges)
                BatchedPropertyChanges.Add(propertyName);
            else
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        #endregion


    }
}
