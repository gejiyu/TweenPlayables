using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace TweenPlayables
{
    /// <summary>
    /// Timeline 计算器行为 - 在 Timeline 播放时执行数学计算并存储结果
    /// </summary>
    [Serializable]
    public class TweenCalculatorBehaviour : PlayableBehaviour
    {
        private bool initialized;
        private TimelineDataManager binding;
        private CalculatorNode calculatorNode;

        /// <summary>
        /// 设置计算器节点配置
        /// </summary>
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
            // 如果未激活则跳过
            if (!calculatorNode.isActive)
                return;

            var dataManager = info.output.GetUserData() as TimelineDataManager;
            
            // 首次播放时初始化
            if (!initialized && dataManager != null)
            {
                Initialize(dataManager);
            }
            
            // Clip 开始播放时执行计算
            if (info.effectivePlayState == PlayState.Playing && binding != null)
            {
                OnCalculatorStarted(binding, playable, info);
            }
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            // 如果未激活则跳过
            if (!calculatorNode.isActive)
                return;

            // Clip 暂停或结束时调用
            var duration = playable.GetDuration();
            var count = playable.GetTime() + info.deltaTime;

            // 检查是否到达 Clip 末尾或 Graph 已完成
            if ((info.effectivePlayState == PlayState.Paused && count > duration) || 
                playable.GetGraph().GetRootPlayable(0).IsDone())
            {
                OnCalculatorFinished(binding, playable, info);
            }
        }

        /// <summary>
        /// 初始化绑定
        /// </summary>
        private void Initialize(TimelineDataManager dataManager)
        {
            if (dataManager == null || initialized) 
                return;

            binding = dataManager;
            initialized = true;
        }

        /// <summary>
        /// 计算器 Clip 开始播放时调用
        /// </summary>
        protected virtual void OnCalculatorStarted(TimelineDataManager dataManager, Playable playable, FrameData info)
        {
            PerformCalculation(dataManager);
        }

        /// <summary>
        /// 计算器 Clip 结束时调用
        /// </summary>
        protected virtual void OnCalculatorFinished(TimelineDataManager dataManager, Playable playable, FrameData info)
        {
            // 可选：清理结果或执行其他逻辑
        }

        /// <summary>
        /// 根据配置执行计算
        /// </summary>
        private void PerformCalculation(TimelineDataManager dataManager)
        {
            if (dataManager == null) return;

            // 获取左操作数
            float leftValue = GetOperandValue(
                dataManager,
                calculatorNode.leftDataType,
                calculatorNode.leftNumericValue,
                calculatorNode.leftDataType == CalculatorDataSource.Protobuf ? calculatorNode.leftProtobufField : calculatorNode.leftDataKey,
                calculatorNode.leftIndexType,
                calculatorNode.leftIndexNumericValue,
                calculatorNode.leftIndexDataKey,
                calculatorNode.leftElementField,
                calculatorNode.leftElementIndexType,
                calculatorNode.leftElementIndexNumericValue,
                calculatorNode.leftElementIndexDataKey
            );

            // 获取右操作数
            float rightValue = GetOperandValue(
                dataManager,
                calculatorNode.rightDataType,
                calculatorNode.rightNumericValue,
                calculatorNode.rightDataType == CalculatorDataSource.Protobuf ? calculatorNode.rightProtobufField : calculatorNode.rightDataKey,
                calculatorNode.rightIndexType,
                calculatorNode.rightIndexNumericValue,
                calculatorNode.rightIndexDataKey,
                calculatorNode.rightElementField,
                calculatorNode.rightElementIndexType,
                calculatorNode.rightElementIndexNumericValue,
                calculatorNode.rightElementIndexDataKey
            );

            // 执行运算
            float result = PerformOperation(leftValue, calculatorNode.calculationOperator, rightValue);

            // 存储结果到 TimelineDataManager
            if (!string.IsNullOrEmpty(calculatorNode.resultDataKey))
            {
                StoreResult(dataManager, calculatorNode.resultDataKey, result,
                    calculatorNode.resultIndexType,
                    calculatorNode.resultIndexNumericValue,
                    calculatorNode.resultIndexDataKey);
            }
        }

        /// <summary>
        /// 根据数据源类型获取操作数值
        /// </summary>
        private float GetOperandValue(TimelineDataManager dataManager, 
            CalculatorDataSource dataType, float numericValue, string dataKey,
            CalculatorDataSource indexType, int indexNumericValue, string indexDataKey,
            string elementField, CalculatorDataSource elementIndexType, int elementIndexNumericValue, string elementIndexDataKey)
        {
            Debug.Log("========== [GetOperandValue] 开始 ==========");
            Debug.Log($"[GetOperandValue] 数据源类型: {dataType}");
            Debug.Log($"[GetOperandValue] numericValue: {numericValue}");
            Debug.Log($"[GetOperandValue] dataKey: '{dataKey}'");
            Debug.Log($"[GetOperandValue] indexType: {indexType}, indexNumericValue: {indexNumericValue}, indexDataKey: '{indexDataKey}'");
            Debug.Log($"[GetOperandValue] elementField: '{elementField}'");
            Debug.Log($"[GetOperandValue] elementIndexType: {elementIndexType}, elementIndexNumericValue: {elementIndexNumericValue}, elementIndexDataKey: '{elementIndexDataKey}'");
            
            float result = 0f;
            
            switch (dataType)
            {
                case CalculatorDataSource.Numeric:
                    result = numericValue;
                    Debug.Log($"[GetOperandValue] Numeric 模式返回: {result}");
                    return result;

                case CalculatorDataSource.DataManager:
                    result = GetDataManagerValue(dataManager, dataKey, indexType, indexNumericValue, indexDataKey);
                    Debug.Log($"[GetOperandValue] DataManager 模式返回: {result}");
                    return result;

                case CalculatorDataSource.Protobuf:
                    result = GetProtobufValue(dataManager, dataKey, indexType, indexNumericValue, indexDataKey,
                        elementField, elementIndexType, elementIndexNumericValue, elementIndexDataKey);
                    Debug.Log($"[GetOperandValue] Protobuf 模式返回: {result}");
                    return result;

                default:
                    Debug.LogWarning($"[GetOperandValue] 未知的数据源类型: {dataType}，返回 0");
                    return 0f;
            }
        }

        /// <summary>
        /// 从 DataManager 获取值
        /// </summary>
        private float GetDataManagerValue(TimelineDataManager dataManager, string dataKey,
            CalculatorDataSource indexType, int indexNumericValue, string indexDataKey)
        {
            if (dataManager == null || string.IsNullOrEmpty(dataKey))
                return 0f;

            int resolvedIndex = GetIndexValue(dataManager, indexType, indexNumericValue, indexDataKey);
            var rawData = dataManager.Get<object>(dataKey, null);

            return rawData != null ? ConvertToFloat(rawData, resolvedIndex) : 0f;
        }

        /// <summary>
        /// 从 Protobuf 获取值
        /// </summary>
        private float GetProtobufValue(TimelineDataManager dataManager, string fieldPath,
            CalculatorDataSource indexType, int indexNumericValue, string indexDataKey,
            string elementField, CalculatorDataSource elementIndexType, int elementIndexNumericValue, string elementIndexDataKey)
        {
            if (dataManager?.genericFishDeadSync == null || string.IsNullOrEmpty(fieldPath))
                return 0f;

            return GetProtobufFieldValue(dataManager.genericFishDeadSync, fieldPath, 
                indexType, indexNumericValue, indexDataKey, dataManager,
                elementField, elementIndexType, elementIndexNumericValue, elementIndexDataKey);
        }

        /// <summary>
        /// 将对象转换为 float 值（支持列表和数组）
        /// </summary>
        private float ConvertToFloat(object data, int index)
        {
            if (data == null) return 0f;

            // 处理列表类型
            switch (data)
            {
                case System.Collections.Generic.List<float> floatList:
                    return GetListValue(floatList, index, "float list");
                    
                case System.Collections.Generic.List<int> intList:
                    return GetListValue(intList, index, "int list");
                    
                case float[] floatArray:
                    return GetArrayValue(floatArray, index, "float array");
                    
                case int[] intArray:
                    return GetArrayValue(intArray, index, "int array");
                    
                case float f:
                    return f;
                    
                case int i:
                    return i;
                    
                case uint ui:
                    return ui;
                    
                case double d:
                    return (float)d;
                    
                case long l:
                    return l;
                    
                case ulong ul:
                    return ul;
                    
                default:
                    return 0f;
            }
        }

        /// <summary>
        /// 从列表获取值（带边界检查）
        /// </summary>
        private T GetListValue<T>(System.Collections.Generic.List<T> list, int index, string typeName) where T : struct
        {
            if (index < 0 || index >= list.Count)
            {
                if (index != 0) // 只在非默认索引时警告
                    Debug.LogWarning($"Index {index} out of range for {typeName} (size: {list.Count})");
                return default;
            }
            return list[index];
        }

        /// <summary>
        /// 从数组获取值（带边界检查）
        /// </summary>
        private T GetArrayValue<T>(T[] array, int index, string typeName) where T : struct
        {
            if (index < 0 || index >= array.Length)
            {
                if (index != 0)
                    Debug.LogWarning($"Index {index} out of range for {typeName} (size: {array.Length})");
                return default;
            }
            return array[index];
        }

        /// <summary>
        /// 使用反射从 Protobuf 字段获取值（支持嵌套路径和元素字段）
        /// </summary>
        private float GetProtobufFieldValue(object protobufObject, string fieldPath, 
            CalculatorDataSource indexType, int indexNumericValue, string indexDataKey, TimelineDataManager dataManager,
            string elementFieldPath, CalculatorDataSource elementIndexType, int elementIndexNumericValue, string elementIndexDataKey)
        {
            if (protobufObject == null || string.IsNullOrEmpty(fieldPath))
                return 0f;

            try
            {
                // 支持嵌套字段路径 (例如: "DrawWheelInfo.WheelId")
                var pathParts = fieldPath.Split('.');
                object currentObject = protobufObject;
                
                // 逐层导航嵌套字段
                for (int i = 0; i < pathParts.Length; i++)
                {
                    if (currentObject == null)
                        return 0f;
                    
                    var type = currentObject.GetType();
                    var property = type.GetProperty(pathParts[i]);
                    
                    if (property == null)
                    {
                        Debug.LogWarning($"Protobuf field '{pathParts[i]}' not found in {type.Name}");
                        return 0f;
                    }
                    
                    var value = property.GetValue(currentObject);
                    
                    // 检查是否是 list 类型 - 如果是 list，不管是不是最后一层都要停止导航
                    var valueType = property.PropertyType;
                    if (valueType.IsGenericType && valueType.GetGenericTypeDefinition().Name.Contains("RepeatedField"))
                    {
                        Debug.Log($"[GetProtobufFieldValue] 在第 {i+1} 步遇到列表类型，停止路径导航");
                        return ConvertProtobufValue(value, indexType, indexNumericValue, indexDataKey, dataManager,
                            elementFieldPath, elementIndexType, elementIndexNumericValue, elementIndexDataKey);
                    }
                    
                    // 最后一层路径 - 提取值
                    if (i == pathParts.Length - 1)
                    {
                        return ConvertProtobufValue(value, indexType, indexNumericValue, indexDataKey, dataManager,
                            elementFieldPath, elementIndexType, elementIndexNumericValue, elementIndexDataKey);
                    }
                    
                    // 继续导航到下一层（只有非 list 的嵌套对象才会继续）
                    currentObject = value;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to get Protobuf field '{fieldPath}': {ex.Message}");
            }

            return 0f;
        }

        /// <summary>
        /// 转换 Protobuf 值为 float（支持 RepeatedField、基础类型和元素字段）
        /// </summary>
        private float ConvertProtobufValue(object value, CalculatorDataSource indexType, 
            int indexNumericValue, string indexDataKey, TimelineDataManager dataManager,
            string elementFieldPath, CalculatorDataSource elementIndexType, int elementIndexNumericValue, string elementIndexDataKey)
        {
            if (value == null) return 0f;

            int index = GetIndexValue(dataManager, indexType, indexNumericValue, indexDataKey);

            // 处理 RepeatedField 类型 - 基本类型
            switch (value)
            {
                case Google.Protobuf.Collections.RepeatedField<uint> uintList:
                    return GetProtobufListValue(uintList, index, "uint");
                    
                case Google.Protobuf.Collections.RepeatedField<int> intList:
                    return GetProtobufListValue(intList, index, "int");
                    
                case Google.Protobuf.Collections.RepeatedField<float> floatList:
                    return GetProtobufListValue(floatList, index, "float");
                    
                case Google.Protobuf.Collections.RepeatedField<double> doubleList:
                    return (float)GetProtobufListValue(doubleList, index, "double");
                    
                case Google.Protobuf.Collections.RepeatedField<long> longList:
                    return GetProtobufListValue(longList, index, "long");
                    
                case Google.Protobuf.Collections.RepeatedField<ulong> ulongList:
                    return GetProtobufListValue(ulongList, index, "ulong");
            }

            // 检查是否是 Protobuf Message 的 RepeatedField
            var valueType = value.GetType();
            if (valueType.IsGenericType && valueType.GetGenericTypeDefinition().Name.Contains("RepeatedField"))
            {
                // 这是一个 RepeatedField<TMessage>，元素是 Protobuf Message
                var indexer = valueType.GetProperty("Item");
                var count = valueType.GetProperty("Count");
                
                if (indexer != null && count != null)
                {
                    int listSize = (int)count.GetValue(value);
                    if (index >= 0 && index < listSize)
                    {
                        var element = indexer.GetValue(value, new object[] { index });
                        
                        // 如果有 elementFieldPath，继续访问元素的字段
                        if (!string.IsNullOrEmpty(elementFieldPath) && element != null)
                        {
                            return GetProtobufFieldValue(element, elementFieldPath,
                                elementIndexType, elementIndexNumericValue, elementIndexDataKey, dataManager,
                                null, CalculatorDataSource.Numeric, 0, null);
                        }
                        
                        // 直接转换元素值
                        return ConvertToFloat(element, 0);
                    }
                    else if (index != 0)
                    {
                        Debug.LogWarning($"Protobuf RepeatedField index {index} out of range (size: {listSize})");
                    }
                }
                return 0f;
            }

            // 处理基础数值类型
            return ConvertToFloat(value, 0);
        }

        /// <summary>
        /// 从 Protobuf RepeatedField 获取值（带边界检查）
        /// </summary>
        private T GetProtobufListValue<T>(Google.Protobuf.Collections.RepeatedField<T> list, int index, string typeName)
        {
            if (index < 0 || index >= list.Count)
            {
                if (index != 0)
                    Debug.LogWarning($"Protobuf RepeatedField<{typeName}> index {index} out of range (size: {list.Count})");
                return default;
            }
            return list[index];
        }

        /// <summary>
        /// 根据索引数据源类型获取索引值
        /// </summary>
        private int GetIndexValue(TimelineDataManager dataManager, CalculatorDataSource indexType, int indexNumericValue, string indexDataKey)
        {
            Debug.Log($"[GetIndexValue] 开始 - indexType: {indexType}, indexNumericValue: {indexNumericValue}, indexDataKey: '{indexDataKey}'");
            
            int result = 0;
            
            switch (indexType)
            {
                case CalculatorDataSource.Numeric:
                    result = indexNumericValue;
                    Debug.Log($"[GetIndexValue] Numeric 模式返回: {result}");
                    return result;

                case CalculatorDataSource.DataManager:
                    if (dataManager != null && !string.IsNullOrEmpty(indexDataKey))
                    {
                        // 尝试获取各种数值类型
                        var value = dataManager.Get<object>(indexDataKey, null);
                        if (value != null)
                        {
                            result = ConvertToInt(value);
                            Debug.Log($"[GetIndexValue] DataManager 模式从键 '{indexDataKey}' 获取到值: {value} (类型: {value.GetType().Name}), 转换为索引: {result}");
                        }
                        else
                        {
                            Debug.LogWarning($"[GetIndexValue] DataManager 键 '{indexDataKey}' 未找到，返回 0");
                        }
                        return result;
                    }
                    Debug.LogWarning($"[GetIndexValue] DataManager 模式但参数无效，返回 0");
                    return 0;

                default:
                    Debug.LogWarning($"[GetIndexValue] 未知的索引类型: {indexType}，返回 0");
                    return 0;
            }
        }
        
        /// <summary>
        /// 将各种数值类型转换为 int
        /// </summary>
        private int ConvertToInt(object value)
        {
            if (value == null) return 0;
            
            switch (value)
            {
                case int i: return i;
                case uint ui: return (int)ui;
                case float f: return (int)f;
                case double d: return (int)d;
                case long l: return (int)l;
                case ulong ul: return (int)ul;
                case short s: return s;
                case ushort us: return us;
                case byte b: return b;
                case sbyte sb: return sb;
                default:
                    Debug.LogWarning($"无法将类型 {value.GetType().Name} 转换为 int，返回 0");
                    return 0;
            }
        }

        /// <summary>
        /// 执行数学运算
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
                    if (rightValue == 0f)
                    {
                        Debug.LogWarning("Division by zero in calculator");
                        return 0f;
                    }
                    return leftValue / rightValue;

                case CalculationOperator.Modulo:
                    if (rightValue == 0f)
                    {
                        Debug.LogWarning("Modulo by zero in calculator");
                        return 0f;
                    }
                    return leftValue % rightValue;

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
        /// 将计算结果存储到 TimelineDataManager
        /// </summary>
        private void StoreResult(TimelineDataManager dataManager, string dataKey, float value,
            CalculatorDataSource indexType, int indexNumericValue, string indexDataKey)
        {
            if (dataManager == null || string.IsNullOrEmpty(dataKey))
                return;

            var existingData = dataManager.Get<object>(dataKey, null);

            // 如果没有现有数据，直接存储为 float
            if (existingData == null)
            {
                dataManager.Set(dataKey, value);
                return;
            }

            int resolvedIndex = GetIndexValue(dataManager, indexType, indexNumericValue, indexDataKey);

            // 根据现有数据类型更新值
            switch (existingData)
            {
                case System.Collections.Generic.List<float> floatList:
                    UpdateList(floatList, resolvedIndex, value, dataKey);
                    break;
                    
                case System.Collections.Generic.List<int> intList:
                    UpdateList(intList, resolvedIndex, (int)value, dataKey);
                    break;
                    
                case float[] floatArray:
                    UpdateArray(floatArray, resolvedIndex, value, dataKey);
                    break;
                    
                case int[] intArray:
                    UpdateArray(intArray, resolvedIndex, (int)value, dataKey);
                    break;
                    
                default:
                    // 非列表类型直接覆盖
                    dataManager.Set(dataKey, value);
                    break;
            }
        }

        /// <summary>
        /// 更新列表中的值（支持追加）
        /// </summary>
        private void UpdateList<T>(System.Collections.Generic.List<T> list, int index, T value, string dataKey)
        {
            if (index >= 0 && index < list.Count)
            {
                list[index] = value;
            }
            else if (index == list.Count)
            {
                list.Add(value);
            }
            else
            {
                Debug.LogWarning($"Cannot update {dataKey}: index {index} out of range (size: {list.Count})");
            }
        }

        /// <summary>
        /// 更新数组中的值
        /// </summary>
        private void UpdateArray<T>(T[] array, int index, T value, string dataKey)
        {
            if (index >= 0 && index < array.Length)
            {
                array[index] = value;
            }
            else
            {
                Debug.LogWarning($"Cannot update {dataKey}: index {index} out of range (size: {array.Length})");
            }
        }
    }
}
