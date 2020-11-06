using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Assertions;
using UnityEngine.Playables;
using GZ.Tools.UnityUtility;

namespace GZ.AnimationGraph
{
    [Serializable]
    public class StateMachineNode : BaseNode, IUpdatableNode
    {
        [SerializeReference] public Parameters Parameters = new Parameters();
        [SerializeReference] public States States = new States();
        [SerializeReference] public List<AnyState> AnyStates = new List<AnyState>();
        [SerializeReference] public List<Transition> Transitions = new List<Transition>();

        [SerializeReference] public State EntryState;

        public bool InTransition;
        public Transition CurrentTransition;
        public int TransitionDestinationIndex;

        public float TransitionProgress => InTransition ? InputPorts[TransitionDestinationIndex].Weight : 0f;

        [SerializeReference] public State CurrentState;
        [SerializeReference] public State NextState;

        private List<TriggerProvider> _triggersToReset = new List<TriggerProvider>();

        public void Update(float deltaTime)
        {
            if (CurrentState == null) { return; }

            if (!InTransition || CurrentTransition.InterruptableByAnyState)
            {
                int index = InTransition && CurrentTransition.SourceState is AnyState ? AnyStates.IndexOf((AnyState)CurrentTransition.SourceState) : AnyStates.Count - 1;

                for (int i = 0; i <= index; i++)
                {
                    AnyState anyState = AnyStates[i];
                    StateFilterItem currentStateFilter = anyState.StateFilter.Find(f => f.State == CurrentState);
                    StateFilterItem nextStateFilter = anyState.StateFilter.Find(f => f.State == NextState);

                    if ((currentStateFilter == null || currentStateFilter.Mode == AnyStateFilterMode.NextState)
                        && (NextState == null || nextStateFilter == null || nextStateFilter.Mode == AnyStateFilterMode.CurrentState))
                    {
                        continue;
                    }


                    Transition evaluatedTransition = null;

                    if (InTransition && CurrentTransition.SourceState == anyState)
                    {
                        evaluatedTransition = EvaluateTransitions(anyState, anyState.ExitTransitions.IndexOf(CurrentTransition));
                    }
                    else { evaluatedTransition = EvaluateTransitions(anyState); }

                    if (evaluatedTransition != null)
                    {
                        StartTransition(evaluatedTransition);
                        break;
                    }
                }
            }

            Transition transition = null;

            for (int i = 0; i < States.Count; i++)
            {
                if (Playable.GetInputWeight(i) > 0f && InputPorts[i].Link != null)
                {
                    State state = States.At(i);
                    state.PreviousTime.Value = state.Time.Value;
                    state.PreviousNormalizedTime.Value = state.NormalizedTime.Value;
                    state.Time.Value += deltaTime;
                    state.NormalizedTime.Value += deltaTime / InputPorts[i].Link.OutputPort.Node.Duration;
                }
            }

            if (InTransition)
            {
                if (CurrentTransition.InterruptionSource != TransitionInterruptionSource.None && !(CurrentTransition.SourceState is AnyState))
                {
                    switch (CurrentTransition.InterruptionSource)
                    {
                        case TransitionInterruptionSource.CurrentState:
                            {
                                int currentTransitionIndex = CurrentTransition.OrderedInterruption ? CurrentState.ExitTransitions.IndexOf(CurrentTransition) : -1;

                                if (currentTransitionIndex != 0) { EvaluateTransitions(CurrentState, currentTransitionIndex); }

                                break;
                            }
                        case TransitionInterruptionSource.NextState:
                            EvaluateTransitions(NextState);

                            break;
                        case TransitionInterruptionSource.CurrentThenNextState:
                            {
                                int currentTransitionIndex = CurrentTransition.OrderedInterruption ? CurrentState.ExitTransitions.IndexOf(CurrentTransition) : -1;

                                if (currentTransitionIndex == 0 || ((transition = EvaluateTransitions(CurrentState, currentTransitionIndex)) != null))
                                {
                                    transition = EvaluateTransitions(NextState);
                                }

                                break;
                            }
                        case TransitionInterruptionSource.NextThenCurrentState:
                            if ((transition = EvaluateTransitions(NextState)) != null)
                            {
                                int currentTransitionIndex = CurrentTransition.OrderedInterruption ? CurrentState.ExitTransitions.IndexOf(CurrentTransition) : -1;

                                if (currentTransitionIndex != 0) { EvaluateTransitions(CurrentState, currentTransitionIndex); }
                            }

                            break;
                    }
                }

                UpdateTransition(deltaTime);
            }
            else
            {
                transition = EvaluateTransitions(CurrentState);
            }

            _triggersToReset.ForEach(t => t.Value = false);
            _triggersToReset.Clear();

            if (transition != null) { StartTransition(transition); }
        }

        private Transition EvaluateTransitions(State state, int end = -1)
        {
            if (end < 0)
            {
                end = state.ExitTransitions.Count;
            }

            for (int i = 0; i < end; i++)
            {
                var transition = state.ExitTransitions[i];

                if (transition == CurrentTransition)
                {
                    continue;
                }

                bool canTransitionate = true;
                HashSet<TriggerProvider> possibleTriggersToReset = new HashSet<TriggerProvider>();

                foreach (var condition in transition.Conditions)
                {
                    if (!condition.IsTrue())
                    {
                        canTransitionate = false;
                        break;
                    }
                    else if (condition.Evaluator is TriggerConditionEvaluator)
                    {
                        possibleTriggersToReset.Add((TriggerProvider)condition.ValueProvider);
                    }
                }

                //if (canTransitionate)
                //{
                //    foreach (var stateCondition in transition.StateConditions)
                //    {
                //        if (!stateCondition.IsTrue(state))
                //        {
                //            canTransitionate = false;
                //            break;
                //        }
                //    }

                //    if (canTransitionate)
                //    {
                //        foreach (var conditionBehaviour in transition.ConditionBehaviours)
                //        {
                //            if (!conditionBehaviour.IsTrue(this, state, machineLayer.NextState))
                //            {
                //                canTransitionate = false;
                //                break;
                //            }
                //        }

                if (canTransitionate)
                {
                    foreach (var triggerToReset in possibleTriggersToReset)
                    {
                        _triggersToReset.Add(triggerToReset);
                    }

                    possibleTriggersToReset.Clear();

                    return transition;
                }
                    //}
                //}

                possibleTriggersToReset.Clear();
            }

            return null;
        }

        private void StartTransition(Transition transition)
        {
            InTransition = true;
            CurrentTransition = transition;
            TransitionDestinationIndex = States.Values.IndexOf(transition.DestinationState);
            var statePlayable = Playable.GetInput(TransitionDestinationIndex);
            statePlayable.Play();
            
            if (InputPorts[TransitionDestinationIndex].Link.OutputPort.Node.Speed < 0f)
            {
                Playable.GetInput(TransitionDestinationIndex).SetTime(InputPorts[TransitionDestinationIndex].Link.OutputPort.Node.RawDuration);
            }

            if (!(transition.SourceState is AnyState))
            {
                CurrentState = transition.SourceState;
            }

            NextState = transition.DestinationState;

            if (transition.InterruptionSource != TransitionInterruptionSource.None && transition.InterruptionSource != TransitionInterruptionSource.CurrentState)
            {
                Transition evaluatedTransition = EvaluateTransitions(NextState);

                if (evaluatedTransition != null) { StartTransition(evaluatedTransition); }
            }
        }

        private void UpdateTransition(float deltaTime)
        {
            int inputsCount = States.Count;
            float transitionDuration = CurrentTransition.Duration;

            float progressDelta = transitionDuration != 0f ? deltaTime / transitionDuration : 1f;
            float transitionProgress = TransitionProgress;
            float newTransitionProgress = transitionProgress + progressDelta;

            float inverseTransitionProgress = 1f - transitionProgress;

            void ResetState(State state)
            {
                state.PreviousTime.Value = 0f;
                state.Time.Value = 0f;
                state.PreviousNormalizedTime.Value = 0f;
                state.NormalizedTime.Value = 0f;
            }

            for (int i = 0; i < inputsCount; i++)
            {
                if (i == TransitionDestinationIndex) continue;

                float inputWeight = InputPorts[i].Weight;

                if (inputWeight <= 0f) { continue; }

                float newInputWeight = inputWeight - progressDelta * (inputWeight / inverseTransitionProgress);

                if (newInputWeight <= 0f)
                {
                    InputPorts[i].Weight = 0f;
                    Playable.GetInput(i).Pause();
                    Playable.GetInput(i).SetTime(0f);
                    ResetState(States.At(i));

                    continue;
                }

                InputPorts[i].Weight = newInputWeight;
            }

            if (newTransitionProgress >= 1f)
            {
                for (int i = 0; i < States.Count; i++)
                {
                    if (i == TransitionDestinationIndex) { continue; }

                    InputPorts[i].Weight = 0f;
                    Playable.GetInput(i).Pause();
                    Playable.GetInput(i).SetTime(0f);
                    ResetState(States.At(i));
                }

                InputPorts[TransitionDestinationIndex].Weight = 1f;

                if (CurrentTransition.PlayAfterTransition)
                    Playable.GetInput(TransitionDestinationIndex).Play();

                CurrentState = CurrentTransition.DestinationState;
                NextState = null;

                InTransition = false;
                CurrentTransition = null;
            }
            else
            {
                InputPorts[TransitionDestinationIndex].Weight = newTransitionProgress;
            }
        }

        #region Parameter
        public Parameter AddParameter(string name)
        {
            var parameter = new Parameter { Name = NameValidation.ValidateName(name, validationName => !Parameters.ContainsKey(validationName)) };
            Parameters.Add(parameter.Name, parameter);

            return parameter;
        }
        public Parameter AddParameter(Parameter parameter)
        {
            parameter.Name = NameValidation.ValidateName(parameter.Name, validationName => !Parameters.ContainsKey(validationName));
            Parameters.Add(parameter.Name, parameter);

            return parameter;
        }

        public bool ContainsParameter(string name, ValueProviderType type) => Parameters.ContainsKey(name) && Parameters[name].Type == type;

        public bool GetBool(string name) => ContainsParameter(name, ValueProviderType.Bool) ? ((BoolProvider)Parameters[name].ValueProvider).Value : false;
        public bool SetBool(string name, bool value)
        {
            if (!ContainsParameter(name, ValueProviderType.Bool)) { return false; }

            ((BoolProvider)Parameters[name].ValueProvider).Value = value;

            return true;
        }

        public int GetInt(string name) => ContainsParameter(name, ValueProviderType.Int) ? ((IntProvider)Parameters[name].ValueProvider).Value : 0;
        public bool SetInt(string name, int value)
        {
            if (!ContainsParameter(name, ValueProviderType.Int)) { return false; }

            ((IntProvider)Parameters[name].ValueProvider).Value = value;

            return true;
        }

        public float GetFloat(string name) => ContainsParameter(name, ValueProviderType.Float) ? ((FloatProvider)Parameters[name].ValueProvider).Value : 0f;
        public bool SetFloat(string name, float value)
        {
            if (!ContainsParameter(name, ValueProviderType.Float)) { return false; }

            ((FloatProvider)Parameters[name].ValueProvider).Value = value;

            return true;
        }

        public bool SetTrigger(string name)
        {
            if (!ContainsParameter(name, ValueProviderType.Trigger)) { return false; }

            ((TriggerProvider)Parameters[name].ValueProvider).Value = true;

            return true;
        }
        public bool ResetTrigger(string name)
        {
            if (!ContainsParameter(name, ValueProviderType.Trigger)) { return false; }

            ((TriggerProvider)Parameters[name].ValueProvider).Value = false;

            return true;
        }

        #endregion Parameter

        public State AddState(string name)
        {
            var state = new State() { Name = NameValidation.ValidateName(name, validationName => !States.ContainsKey(validationName) && !validationName.Equals(AnyState.AnyStateName)) };

            if (States.Count == 0)
            {
                CurrentState = state;

                state.InputPort = CreateBaseInputPort(1f);
            }
            else
            {
                state.InputPort = CreateBaseInputPort(0f);
            }

            States.Add(state.Name, state);

            return state;
        }
        public State AddState(State state)
        {
            state.Name = NameValidation.ValidateName(state.Name, validationName => !States.ContainsKey(validationName) && !validationName.Equals(AnyState.AnyStateName));

            if (States.Count == 0)
            {
                CurrentState = state;

                state.InputPort = CreateBaseInputPort(1f);
            }
            else
            {
                state.InputPort = CreateBaseInputPort(0f);
            }

            States.Add(state.Name, state);

            return state;
        }

        public State RemoveState(string stateName)
        {
            if (!States.ContainsKey(stateName)) { return null; }

            State state = States[stateName];
            RemovePlayableInput(Playable, States.Values.IndexOf(state));
            States.Remove(stateName);

            return state;
        }

        private void RemovePlayableInput(Playable playable, int index)
        {
            int FindPlayableOutput(Playable outputPlayable, Playable inputPlayable)
            {
                int outputCount = outputPlayable.GetOutputCount();

                for (int i = 0; i < outputCount; i++)
                {
                    if (outputPlayable.GetOutput(i).Equals(inputPlayable))
                    {
                        return i;
                    }    
                }

                return -1;
            }

            playable.DisconnectInput(index);
            int newInputCount = playable.GetInputCount() - 1;

            for (int i = index; i < newInputCount; i++)
            {
                Playable outputPlayable = playable.GetInput(i + 1);
                playable.ConnectInput(i, outputPlayable, FindPlayableOutput(outputPlayable, playable), playable.GetInputWeight(i + 1));
                playable.DisconnectInput(i + 1);
            }

            playable.SetInputCount(newInputCount);
        }

        public void SetDefaultState(string stateName)
        {
            Assert.IsTrue(States.ContainsKey(stateName), $"Failed to set default state because the state machine {Name} doesn't contain an state called {stateName}");

            int index = States.Values.IndexOf(CurrentState);
            CurrentState = States[stateName];
            InputPorts[index].Weight = 1f;
            Playable.GetInput(index).Play();
        }

        public override NodeLink Connect(NodeInputPort inputPort, NodeOutputPort outputPort)
        {
            outputPort.Node.Playable.SetTime(0f);

            if (States.At(inputPort.Index) == CurrentState)
            {
                outputPort.Node.Playable.Play();
            }
            else if (States.At(inputPort.Index) == NextState)
            {
                outputPort.Node.Playable.Play();
            }
            else
            {
                outputPort.Node.Playable.Pause();
            }

            inputPort.Weight = inputPort.Weight;

            return base.Connect(inputPort, outputPort);
        }

        public override BaseNode Copy()
        {
            StateMachineNode copy = new StateMachineNode() { Name = Name, Speed = Speed };

            var valueProviderCopyMap = new Dictionary<IValueProvider, IValueProvider>();

            Parameters.Values.ForEach(p =>
            {
                var parameterCopy = copy.AddParameter(p.Copy());
                valueProviderCopyMap.Add(p.ValueProvider, parameterCopy.ValueProvider);
            });

            var copiedStates = new Dictionary<State, State>();
            var copiedTransitions = new Dictionary<Transition, Transition>();
            var transitionsToSetSource = new List<(Transition copy, State originalSource)>();
            var transitionsToSetDestination = new List<(Transition copy, State originalDestination)>();
            var conditionsToSetValueProvider = new List<(TransitionCondition copy, IValueProvider originalValueProvider)>();

            Transition CopyTransition(Transition original)
            {
                if (copiedTransitions.ContainsKey(original)) { return copiedTransitions[original]; }

                Transition transitionCopy = original.Copy(copiedStates, valueProviderCopyMap);

                copy.Transitions.Add(transitionCopy);

                copiedTransitions.Add(original, transitionCopy);

                if (transitionCopy.SourceState == null) { transitionsToSetSource.Add((transitionCopy, original.SourceState)); }

                if (transitionCopy.DestinationState == null) { transitionsToSetDestination.Add((transitionCopy, original.DestinationState)); }

                return transitionCopy;
            }

            States.Values.ForEach(s =>
            {
                State stateCopy = s.Copy(CopyTransition, valueProviderCopyMap);
                copy.AddState(stateCopy);
                copiedStates.Add(s, stateCopy);
            });
            AnyStates.ForEach(s =>
            {
                AnyState anyStateCopy = (AnyState)s.Copy(CopyTransition);
                s.StateFilter.ForEach(f => anyStateCopy.StateFilter.Add(new StateFilterItem() { State = copiedStates[f.State], Mode = f.Mode }));
                copy.AnyStates.Add(anyStateCopy);
                copiedStates.Add(s, anyStateCopy);
            });

            copy.EntryState = copiedStates[EntryState];

            transitionsToSetSource.ForEach(t => t.copy.SourceState = copiedStates[t.originalSource]);
            transitionsToSetDestination.ForEach(t => t.copy.DestinationState = copiedStates[t.originalDestination]);

            return copy;
        }

        protected override Playable OnCreatePlayable(PlayableGraph playableGraph)
        {
            AnimationMixerPlayable playable = AnimationMixerPlayable.Create(playableGraph);

            playable.SetInputCount(InputPorts.Count);

            InputPorts.ForEach(inputPort =>
            {
                inputPort.Weight = inputPort.Weight;
            });

            return playable;
        }

        public override (float rawDuration, float duration) CalculateDuration()
        {
            return (float.PositiveInfinity, float.PositiveInfinity);
        }
    }

    [Serializable]
    public class Parameters : IndexedDictionary<string, Parameter> { }

    [Serializable]
    public class States : IndexedDictionary<string, State> { }
}
