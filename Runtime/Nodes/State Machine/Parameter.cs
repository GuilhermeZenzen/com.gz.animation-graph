using System;
using UnityEngine;

namespace GZ.AnimationGraph
{
    [Serializable]
    public class Parameter
    {
        public string Id;

        public string Name;

        [SerializeField] private ValueProviderType _type = ValueProviderType.Bool;
        public ValueProviderType Type
        {
            get => _type;
            set
            {
                if (_type == value) { return; }

                _type = value;

                switch (_type)
                {
                    case ValueProviderType.Bool:
                        ValueProvider = new BoolProvider();
                        break;
                    case ValueProviderType.Int:
                        ValueProvider = new IntProvider();
                        break;
                    case ValueProviderType.Float:
                        ValueProvider = new FloatProvider();
                        break;
                    case ValueProviderType.Trigger:
                        ValueProvider = new TriggerProvider();
                        break;
                    default:
                        break;
                }

                OnTypeChanged?.Invoke();
            }
        }

        [SerializeReference] public IValueProvider ValueProvider = new BoolProvider();

        public event Action OnTypeChanged;

        public Parameter()
        {
            Id = Guid.NewGuid().ToString();
        }

        public Parameter Copy() => new Parameter() { Name = Name, _type = _type, ValueProvider = ValueProvider.Copy() };
    }

    public enum ParameterType
    {
        Bool,
        Int,
        Float,
        Trigger
    }
}
