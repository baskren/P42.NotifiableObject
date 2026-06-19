using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using Newtonsoft.Json;

namespace P42.NotifiableObject
{
    public abstract class BaseNotifiablePropertyObject : INotifyPropertyChanged
    {
        [JsonIgnore]
        private static readonly Lock ClassLock = new ();

        [JsonIgnore]
        public static SynchronizationContext? SyncContext
        {
            get
            {
                lock (ClassLock)
                    return field;
            }
            set 
            {
                lock (ClassLock)
                    field = value;
            }
        } 
        
        [JsonIgnore]
        // ReSharper disable once MemberCanBePrivate.Global
        public static long Instances { get; private set; }

        [JsonIgnore]
        private readonly Lock _lock = new();
        
        [JsonIgnore]
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public long InstanceId { get; private set; }

        [JsonIgnore]
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public virtual bool Logging { get; }

        // ReSharper disable once MemberCanBePrivate.Global
        protected ConcurrentQueue<string> BatchedPropertyChanges { get; } = new ConcurrentQueue<string>();

        private int _batchChanges;
        [JsonIgnore]
        public bool BatchChanges
        {
            get
            {
                lock (_lock)
                    return _batchChanges > 0;
            }
            set
            {
                lock (_lock)
                {
                    if (value)
                        _batchChanges++;
                    else if (_batchChanges > 0)
                        _batchChanges--;
                    if (_batchChanges != 0) 
                        return;
                }

                while (BatchedPropertyChanges.TryDequeue(out var name))
                    OnPropertyChanged(name);
                
            }
        }

        [JsonIgnore]
        public bool HasChanged
        {
            get
            {
                lock (_lock)
                    return field;
            }
            protected set
            {
                if (_deserializing)
                    return;
                
                lock (_lock)
                    field = value;
            }
        }

        private bool _deserializing;
        
        [OnDeserializing]
        internal void OnDeserializing(StreamingContext context)
            => _deserializing = true;
        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
            => _deserializing = false;


        internal BaseNotifiablePropertyObject()
        {
            lock (ClassLock)
            {
                InstanceId = Instances++;
            }
        }

        #region Property Change Handler
        //public event PropertyChangedEventHandler PropertyChanged;
        private readonly AsyncAwaitBestPractices.WeakEventManager _propertyChangedEventManager = new();
        public event PropertyChangedEventHandler? PropertyChanged
        {
            add => _propertyChangedEventManager.AddEventHandler(value);
            remove => _propertyChangedEventManager.RemoveEventHandler(value);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (_deserializing)
                return;
            
            if (BatchChanges)
            {
                BatchedPropertyChanges.Enqueue(propertyName);
                return;
            }
            
            var context = SyncContext;
            if (context == null)
                _propertyChangedEventManager.RaiseEvent(propertyName);
            else
                context.Post(_ => _propertyChangedEventManager.RaiseEvent(propertyName), null);
            
        }


        #endregion


    }
}
