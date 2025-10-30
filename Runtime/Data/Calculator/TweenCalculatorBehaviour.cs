using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace TweenPlayables
{
    [Serializable]
    public class TweenCalculatorBehaviour : PlayableBehaviour
    {
        private bool initialized;
        private TimelineDataManager binding;
        private CalculatorNode calculatorNode;

        public void SetCalculatorNode(CalculatorNode calculatorNode)
        {
            this.calculatorNode = calculatorNode;
        }

        public override void OnGraphStop(Playable playable)
        {
            initialized = false;
            binding = null;
        }

        public override void OnPlayableCreate(Playable playable)
        {
            initialized = false;
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            // Skip if not active
            if (!calculatorNode.isActive)
                return;

            var dataManager = info.output.GetUserData() as TimelineDataManager;
            
            // Initialize on first play if not already initialized
            if (!initialized && dataManager != null)
            {
                Initialize(dataManager);
            }
            
            // Called when the clip starts playing
            if (info.effectivePlayState == PlayState.Playing && binding != null)
            {
                OnCalculatorStarted(binding, playable, info);
            }
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            // Skip if not active
            if (!calculatorNode.isActive)
                return;

            // Called when the clip pauses or ends
            var duration = playable.GetDuration();
            var count = playable.GetTime() + info.deltaTime;

            // Check if reached end of clip or graph is done
            if ((info.effectivePlayState == PlayState.Paused && count > duration) || 
                playable.GetGraph().GetRootPlayable(0).IsDone())
            {
                OnCalculatorFinished(binding, playable, info);
            }
        }

        private void Initialize(TimelineDataManager dataManager)
        {
            if (dataManager == null) return;
            if (initialized) return;

            binding = dataManager;
            initialized = true;
        }

        /// <summary>
        /// Called when calculator clip starts playing
        /// </summary>
        protected virtual void OnCalculatorStarted(TimelineDataManager dataManager, Playable playable, FrameData info)
        {
            // Perform calculation and store result
            PerformCalculation(dataManager);
        }

        /// <summary>
        /// Called when calculator clip finishes
        /// </summary>
        protected virtual void OnCalculatorFinished(TimelineDataManager dataManager, Playable playable, FrameData info)
        {
            // Calculator finish logic - could optionally clear results or perform cleanup
        }

        /// <summary>
        /// Performs the calculation based on calculator node configuration
        /// </summary>
        private void PerformCalculation(TimelineDataManager dataManager)
        {
            if (dataManager == null) return;

            // Get left operand value
            float leftValue = GetOperandValue(
                dataManager,
                calculatorNode.leftDataType,
                calculatorNode.leftNumericValue,
                calculatorNode.leftDataType == CalculatorDataSource.Protobuf ? calculatorNode.leftProtobufField : calculatorNode.leftDataKey,
                calculatorNode.leftIndexType,
                calculatorNode.leftIndexNumericValue,
                calculatorNode.leftIndexDataKey
            );

            // Get right operand value
            float rightValue = GetOperandValue(
                dataManager,
                calculatorNode.rightDataType,
                calculatorNode.rightNumericValue,
                calculatorNode.rightDataType == CalculatorDataSource.Protobuf ? calculatorNode.rightProtobufField : calculatorNode.rightDataKey,
                calculatorNode.rightIndexType,
                calculatorNode.rightIndexNumericValue,
                calculatorNode.rightIndexDataKey
            );

            // Perform calculation
            float result = PerformOperation(leftValue, calculatorNode.calculationOperator, rightValue);

            // Store result in TimelineDataManager
            if (!string.IsNullOrEmpty(calculatorNode.resultDataKey))
            {
                StoreResult(dataManager, calculatorNode.resultDataKey, result,
                    calculatorNode.resultIndexType,
                    calculatorNode.resultIndexNumericValue,
                    calculatorNode.resultIndexDataKey);
            }
        }

        /// <summary>
        /// Gets operand value based on data source type
        /// </summary>
        private float GetOperandValue(TimelineDataManager dataManager, 
            CalculatorDataSource dataType, float numericValue, string dataKey,
            CalculatorDataSource indexType, int indexNumericValue, string indexDataKey)
        {
            switch (dataType)
            {
                case CalculatorDataSource.Numeric:
                    return numericValue;

                case CalculatorDataSource.DataManager:
                    if (dataManager != null && !string.IsNullOrEmpty(dataKey))
                    {
                        // Resolve index value
                        int resolvedIndex = GetIndexValue(dataManager, indexType, indexNumericValue, indexDataKey);

                        // Get raw data
                        var rawData = dataManager.Get<object>(dataKey, null);

                        if (rawData != null)
                        {
                            return GetValueFromData(rawData, resolvedIndex);
                        }
                    }
                    return 0f;

                case CalculatorDataSource.Protobuf:
                    if (dataManager != null && dataManager.genericFishDeadSync != null && !string.IsNullOrEmpty(dataKey))
                    {
                        return GetProtobufFieldValue(dataManager.genericFishDeadSync, dataKey, indexType, indexNumericValue, indexDataKey, dataManager);
                    }
                    return 0f;

                default:
                    return 0f;
            }
        }

        /// <summary>
        /// Gets value from data object (handles lists and arrays)
        /// </summary>
        private float GetValueFromData(object rawData, int resolvedIndex)
        {
            // Check for List<float>
            if (rawData is System.Collections.Generic.List<float> floatList)
            {
                if (resolvedIndex >= 0 && resolvedIndex < floatList.Count)
                    return floatList[resolvedIndex];
                return 0f;
            }
            // Check for List<int>
            else if (rawData is System.Collections.Generic.List<int> intList)
            {
                if (resolvedIndex >= 0 && resolvedIndex < intList.Count)
                    return intList[resolvedIndex];
                return 0f;
            }
            // Check for float[]
            else if (rawData is float[] floatArray)
            {
                if (resolvedIndex >= 0 && resolvedIndex < floatArray.Length)
                    return floatArray[resolvedIndex];
                return 0f;
            }
            // Check for int[]
            else if (rawData is int[] intArray)
            {
                if (resolvedIndex >= 0 && resolvedIndex < intArray.Length)
                    return intArray[resolvedIndex];
                return 0f;
            }
            // Try direct conversion
            else if (rawData is float f)
            {
                return f;
            }
            else if (rawData is int i)
            {
                return i;
            }
            else if (rawData is uint ui)
            {
                return ui;
            }
            
            return 0f;
        }

        /// <summary>
        /// Gets value from Protobuf field using reflection
        /// </summary>
        private float GetProtobufFieldValue(object protobufObject, string fieldName, 
            CalculatorDataSource indexType, int indexNumericValue, string indexDataKey, TimelineDataManager dataManager)
        {
            if (protobufObject == null || string.IsNullOrEmpty(fieldName))
                return 0f;

            try
            {
                // Support nested field paths (e.g., "DrawWheelInfo.WheelId")
                var fieldPath = fieldName.Split('.');
                object currentObject = protobufObject;
                
                // Navigate through nested fields
                for (int i = 0; i < fieldPath.Length; i++)
                {
                    if (currentObject == null)
                        return 0f;
                    
                    var type = currentObject.GetType();
                    var property = type.GetProperty(fieldPath[i]);
                    
                    if (property == null)
                        return 0f;
                    
                    var value = property.GetValue(currentObject);
                    
                    // If this is the last part of the path, extract the value
                    if (i == fieldPath.Length - 1)
                    {
                        if (value != null)
                        {
                            // If it's a repeated field (list), use index
                            if (value is Google.Protobuf.Collections.RepeatedField<uint> uintList)
                            {
                                int index = GetIndexValue(dataManager, indexType, indexNumericValue, indexDataKey);
                                if (index >= 0 && index < uintList.Count)
                                    return uintList[index];
                                return 0f;
                            }
                            else if (value is Google.Protobuf.Collections.RepeatedField<int> intList)
                            {
                                int index = GetIndexValue(dataManager, indexType, indexNumericValue, indexDataKey);
                                if (index >= 0 && index < intList.Count)
                                    return intList[index];
                                return 0f;
                            }
                            else if (value is Google.Protobuf.Collections.RepeatedField<float> floatList)
                            {
                                int index = GetIndexValue(dataManager, indexType, indexNumericValue, indexDataKey);
                                if (index >= 0 && index < floatList.Count)
                                    return floatList[index];
                                return 0f;
                            }
                            // Handle primitive types
                            else if (value is uint ui)
                            {
                                return ui;
                            }
                            else if (value is int intVal)
                            {
                                return intVal;
                            }
                            else if (value is float f)
                            {
                                return f;
                            }
                            else if (value is double d)
                            {
                                return (float)d;
                            }
                            else if (value is long l)
                            {
                                return l;
                            }
                            else if (value is ulong ul)
                            {
                                return ul;
                            }
                        }
                        return 0f;
                    }
                    else
                    {
                        // Not the last part - continue navigating
                        currentObject = value;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to get Protobuf field '{fieldName}': {ex.Message}");
            }

            return 0f;
        }

        /// <summary>
        /// Gets index value based on data source type
        /// </summary>
        private int GetIndexValue(TimelineDataManager dataManager, CalculatorDataSource indexType, int indexNumericValue, string indexDataKey)
        {
            switch (indexType)
            {
                case CalculatorDataSource.Numeric:
                    return indexNumericValue;

                case CalculatorDataSource.DataManager:
                    if (dataManager != null && !string.IsNullOrEmpty(indexDataKey))
                    {
                        return dataManager.Get<int>(indexDataKey, 0);
                    }
                    return 0;

                default:
                    return 0;
            }
        }

        /// <summary>
        /// Performs the mathematical operation
        /// </summary>
        private float PerformOperation(float leftValue, CalculationOperator op, float rightValue)
        {
            switch (op)
            {
                case CalculationOperator.Add:
                    return leftValue + rightValue;

                case CalculationOperator.Subtract:
                    return leftValue - rightValue;

                case CalculationOperator.Multiply:
                    return leftValue * rightValue;

                case CalculationOperator.Divide:
                    return rightValue != 0 ? leftValue / rightValue : 0f;

                case CalculationOperator.Modulo:
                    return rightValue != 0 ? leftValue % rightValue : 0f;

                case CalculationOperator.Power:
                    return Mathf.Pow(leftValue, rightValue);

                case CalculationOperator.Min:
                    return Mathf.Min(leftValue, rightValue);

                case CalculationOperator.Max:
                    return Mathf.Max(leftValue, rightValue);

                default:
                    return 0f;
            }
        }

        /// <summary>
        /// Stores the result in TimelineDataManager
        /// </summary>
        private void StoreResult(TimelineDataManager dataManager, string dataKey, float value,
            CalculatorDataSource indexType, int indexNumericValue, string indexDataKey)
        {
            if (dataManager == null || string.IsNullOrEmpty(dataKey))
                return;

            // Get the existing data
            var existingData = dataManager.Get<object>(dataKey, null);

            // If no existing data, store as simple float
            if (existingData == null)
            {
                dataManager.Set(dataKey, value);
                return;
            }

            // Resolve index value
            int resolvedIndex = GetIndexValue(dataManager, indexType, indexNumericValue, indexDataKey);

            // Check if existing data is a list or array and update at index
            if (existingData is System.Collections.Generic.List<float> floatList)
            {
                if (resolvedIndex >= 0 && resolvedIndex < floatList.Count)
                {
                    floatList[resolvedIndex] = value;
                }
                else if (resolvedIndex == floatList.Count)
                {
                    // Allow adding to the end
                    floatList.Add(value);
                }
            }
            else if (existingData is System.Collections.Generic.List<int> intList)
            {
                if (resolvedIndex >= 0 && resolvedIndex < intList.Count)
                {
                    intList[resolvedIndex] = (int)value;
                }
                else if (resolvedIndex == intList.Count)
                {
                    intList.Add((int)value);
                }
            }
            else if (existingData is float[] floatArray)
            {
                if (resolvedIndex >= 0 && resolvedIndex < floatArray.Length)
                {
                    floatArray[resolvedIndex] = value;
                }
            }
            else if (existingData is int[] intArray)
            {
                if (resolvedIndex >= 0 && resolvedIndex < intArray.Length)
                {
                    intArray[resolvedIndex] = (int)value;
                }
            }
            else
            {
                // For non-list types, just overwrite with new value
                dataManager.Set(dataKey, value);
            }
        }
    }
}
