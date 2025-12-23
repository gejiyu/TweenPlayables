using System;
using System.Collections;
using UnityEngine;

namespace TweenPlayables
{
    /// <summary>
    /// 数据源类型
    /// </summary>
    [Serializable]
    public enum DataSourceType
    {
        Direct = 0,         // 直接值
        DataManager = 1,    // TimelineDataManager
        Protobuf = 2        // Protobuf 字段
    }

    /// <summary>
    /// 字符串数据源配置 - 统一的字符串值来源配置结构
    /// 参考 ConditionalPlayableAssetBase.CustomConditionData 的设计
    /// 使所有 StringParameter 的数据源配置保持一致
    /// </summary>
    [Serializable]
    public class StringDataSource
    {
        /// <summary>
        /// 数据源类型
        /// </summary>
        public DataSourceType dataType = DataSourceType.Direct;
        
        /// <summary>
        /// 直接字符串值（当 dataType = Direct 时使用）
        /// </summary>
        public string directValue = "";
        
        /// <summary>
        /// DataManager 数据键（当 dataType = DataManager 时使用）
        /// </summary>
        public string dataKey = "";
        
        /// <summary>
        /// Protobuf 字段路径（当 dataType = Protobuf 时使用）
        /// 支持嵌套路径，如 "DrawWheelInfo.WheelId"
        /// 如果指向 List，配合 index 和 elementField 使用
        /// </summary>
        public string protobufField = "";
        
        /// <summary>
        /// List 元素的字段路径（当 protobufField 指向 List 且元素是 Protobuf Message 时使用）
        /// 例如: protobufField="items", elementField="ExcelId" → items[index].ExcelId
        /// </summary>
        public string elementField = "";
        
        /// <summary>
        /// 列表索引数据源类型（用于 List/Array 类型的数据）
        /// </summary>
        public DataSourceType indexType = DataSourceType.Direct;
        
        /// <summary>
        /// 数值索引（当 indexType = Direct 时使用）
        /// </summary>
        public int indexNumericValue = 0;

        /// <summary>
        /// 索引数据键（当 indexType = DataManager 时使用）
        /// </summary>
        public string indexDataKey = "";
        
        /// <summary>
        /// 元素字段的索引数据源类型（当 elementField 也指向 List 时使用）
        /// </summary>
        public DataSourceType elementIndexType = DataSourceType.Direct;
        
        /// <summary>
        /// 元素字段的数值索引（当 elementIndexType = Direct 时使用）
        /// </summary>
        public int elementIndexNumericValue = 0;

        /// <summary>
        /// 元素字段的索引数据键（当 elementIndexType = DataManager 时使用）
        /// </summary>
        public string elementIndexDataKey = "";
        
        /// <summary>
        /// 缓存的 TimelineDataManager 引用（运行时通过 TweenAnimationBehaviour 传入）
        /// Timeline 资产不能直接引用场景对象，所以不序列化
        /// </summary>
        [NonSerialized]
        private UnityEngine.Timeline.TimelineDataManager cachedDataManager;
        
        /// <summary>
        /// 公开访问 cachedDataManager
        /// </summary>
        public UnityEngine.Timeline.TimelineDataManager CachedDataManager
        {
            get { return cachedDataManager; }
            set { cachedDataManager = value; }
        }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public StringDataSource()
        {
        }
        
        /// <summary>
        /// 带初始值的构造函数
        /// </summary>
        public StringDataSource(string defaultValue)
        {
            this.directValue = defaultValue;
        }
        
        /// <summary>
        /// 获取实际的字符串值
        /// </summary>
        /// <returns>解析后的字符串值</returns>
        public string GetValue()
        {
            Debug.Log($"[字符串数据源.获取值] 开始获取值 - 数据类型: {dataType}");
            
            // 如果没有 DataManager，强制使用 Direct 类型
            if (cachedDataManager == null && dataType != DataSourceType.Direct)
            {
                Debug.LogWarning($"[字符串数据源.获取值] DataManager 为空，使用直接值: '{directValue}'");
                return directValue ?? "";
            }
            
            // 记录配置信息
            switch (dataType)
            {
                case DataSourceType.Direct:
                    Debug.Log($"[字符串数据源.获取值] 直接模式 - 直接值: '{directValue}'");
                    break;
                case DataSourceType.DataManager:
                    Debug.Log($"[字符串数据源.获取值] DataManager模式 - 数据键: '{dataKey}', 索引类型: {indexType}, 数值索引: {indexNumericValue}, 索引数据键: '{indexDataKey}'");
                    break;
                case DataSourceType.Protobuf:
                    Debug.Log($"[字符串数据源.获取值] Protobuf模式 - 字段路径: '{protobufField}', 元素字段: '{elementField}', 索引类型: {indexType}, 数值索引: {indexNumericValue}, 索引数据键: '{indexDataKey}'");
                    break;
            }
            
            string result = StringDataSourceAccessor.GetString(this, cachedDataManager);
            Debug.Log($"[字符串数据源.获取值] 最终结果: '{result}'");
            
            return result;
        }
    }

    /// <summary>
    /// 字符串数据源访问器 - 负责从各种数据源获取字符串值
    /// </summary>
    public static class StringDataSourceAccessor
    {
        /// <summary>
        /// 从 StringDataSource 获取字符串值
        /// </summary>
        public static string GetString(StringDataSource source, object dataManagerObject = null)
        {
            Debug.Log("========== [GetString] 开始 ==========");
            
            if (source == null)
            {
                Debug.LogWarning("[GetString] source 为 null，返回空字符串");
                return "";
            }
            
            Debug.Log($"[GetString] 数据源类型: {source.dataType}");
                
            switch (source.dataType)
            {
                case DataSourceType.Direct:
                    Debug.Log($"[GetString] Direct 模式 - directValue: '{source.directValue}'");
                    string directResult = source.directValue ?? "";
                    Debug.Log($"[GetString] Direct 模式返回: '{directResult}'");
                    return directResult;

                case DataSourceType.DataManager:
                    Debug.Log($"[GetString] DataManager 模式:");
                    Debug.Log($"  - dataKey: '{source.dataKey}'");
                    Debug.Log($"  - indexType: {source.indexType}");
                    Debug.Log($"  - indexNumericValue: {source.indexNumericValue}");
                    Debug.Log($"  - indexDataKey: '{source.indexDataKey}'");
                    Debug.Log($"  - dataManagerObject != null: {dataManagerObject != null}");
                    
                    if (dataManagerObject != null && !string.IsNullOrEmpty(source.dataKey))
                    {
                        Debug.Log("[GetString] 开始解析主列表索引...");
                        int resolvedIndex = GetIndexValue(source.indexType, source.indexNumericValue, source.indexDataKey, dataManagerObject);
                        Debug.Log($"[GetString] 主列表索引解析完成: {resolvedIndex}");
                        
                        string dmResult = GetStringFromDataManager(dataManagerObject, source.dataKey, resolvedIndex, source.directValue);
                        Debug.Log($"[GetString] DataManager 模式返回: '{dmResult}'");
                        return dmResult;
                    }
                    Debug.Log($"[GetString] DataManager 条件不满足，返回 directValue: '{source.directValue}'");
                    return source.directValue ?? "";

                case DataSourceType.Protobuf:
                    Debug.Log($"[GetString] Protobuf 模式:");
                    Debug.Log($"  - protobufField: '{source.protobufField}'");
                    Debug.Log($"  - elementField: '{source.elementField}'");
                    Debug.Log($"  - indexType: {source.indexType}");
                    Debug.Log($"  - indexNumericValue: {source.indexNumericValue}");
                    Debug.Log($"  - indexDataKey: '{source.indexDataKey}'");
                    Debug.Log($"  - elementIndexType: {source.elementIndexType}");
                    Debug.Log($"  - elementIndexNumericValue: {source.elementIndexNumericValue}");
                    Debug.Log($"  - elementIndexDataKey: '{source.elementIndexDataKey}'");
                    Debug.Log($"  - dataManagerObject != null: {dataManagerObject != null}");
                    
                    if (dataManagerObject != null && !string.IsNullOrEmpty(source.protobufField))
                    {
                        Debug.Log("[GetString] 开始解析主列表索引...");
                        int resolvedIndex = GetIndexValue(source.indexType, source.indexNumericValue, source.indexDataKey, dataManagerObject);
                        Debug.Log($"[GetString] 主列表索引解析完成: {resolvedIndex}");
                        
                        Debug.Log("[GetString] 开始解析元素列表索引...");
                        int elementResolvedIndex = GetIndexValue(source.elementIndexType, source.elementIndexNumericValue, source.elementIndexDataKey, dataManagerObject);
                        Debug.Log($"[GetString] 元素列表索引解析完成: {elementResolvedIndex}");
                        
                        Debug.Log("[GetString] 开始获取 Protobuf 对象...");
                        var protobufObject = GetProtobufObject(dataManagerObject);
                        Debug.Log($"[GetString] Protobuf 对象获取结果: {(protobufObject != null ? protobufObject.GetType().Name : "null")}");
                        
                        if (protobufObject != null)
                        {
                            Debug.Log($"[GetString] 开始获取字段值: protobufField='{source.protobufField}', listIndex={resolvedIndex}, elementField='{source.elementField}', elementListIndex={elementResolvedIndex}");
                            var value = GetProtobufFieldValue(protobufObject, source.protobufField, resolvedIndex, source.elementField, elementResolvedIndex);
                            Debug.Log($"[GetString] 字段值获取结果: {(value != null ? $"类型={value.GetType().Name}, 值={value}" : "null")}");
                            
                            string pbResult = value != null ? value.ToString() : (source.directValue ?? "");
                            Debug.Log($"[GetString] Protobuf 模式返回: '{pbResult}'");
                            return pbResult;
                        }
                        else
                        {
                            Debug.LogWarning("[GetString] Protobuf 对象为 null，返回 directValue");
                        }
                    }
                    else
                    {
                        Debug.Log($"[GetString] Protobuf 条件不满足 (dataManagerObject={dataManagerObject != null}, protobufField='{source.protobufField}')");
                    }
                    Debug.Log($"[GetString] Protobuf 模式返回 directValue: '{source.directValue}'");
                    return source.directValue ?? "";

                default:
                    Debug.LogWarning($"[GetString] 未知的数据源类型: {source.dataType}");
                    return source.directValue ?? "";
            }
        }

        /// <summary>
        /// 从 DataManager 获取字符串值（支持列表）
        /// </summary>
        private static string GetStringFromDataManager(object dataManagerObject, string dataKey, int index, string defaultValue)
        {
            try
            {
                var type = dataManagerObject.GetType();
                
                if (type.Name == "TimelineDataManager")
                {
                    var getMethod = type.GetMethod("Get");
                    if (getMethod != null)
                    {
                        var genericMethod = getMethod.MakeGenericMethod(typeof(object));
                        var result = genericMethod.Invoke(dataManagerObject, new object[] { dataKey, null });
                        
                        if (result != null)
                        {
                            // 检查是否是列表类型 - 支持任何类型的列表
                            var resultType = result.GetType();
                            if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(System.Collections.Generic.List<>))
                            {
                                // 获取列表的 Count 属性
                                var countProperty = resultType.GetProperty("Count");
                                int listSize = countProperty != null ? (int)countProperty.GetValue(result) : 0;
                                
                                if (index >= 0 && index < listSize)
                                {
                                    // 获取索引器
                                    var indexer = resultType.GetProperty("Item");
                                    if (indexer != null)
                                    {
                                        var element = indexer.GetValue(result, new object[] { index });
                                        return element != null ? element.ToString() : defaultValue;
                                    }
                                }
                                return defaultValue;
                            }
                            else if (resultType.IsArray)
                            {
                                var array = result as Array;
                                if (index >= 0 && index < array.Length)
                                {
                                    var element = array.GetValue(index);
                                    return element != null ? element.ToString() : defaultValue;
                                }
                                return defaultValue;
                            }
                            else
                            {
                                // 直接返回转换为字符串
                                return result.ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to get string from DataManager (key: {dataKey}): {ex.Message}");
            }

            return defaultValue;
        }

        /// <summary>
        /// 获取索引值
        /// </summary>
        private static int GetIndexValue(DataSourceType indexType, int indexNumericValue, string indexDataKey, object dataManagerObject)
        {
            Debug.Log($"[获取索引值] 开始 - 索引类型: {indexType}, 数值索引: {indexNumericValue}, 索引数据键: '{indexDataKey}'");
            
            int result = 0;
            
            switch (indexType)
            {
                case DataSourceType.Direct:
                    result = indexNumericValue;
                    Debug.Log($"[获取索引值] 直接模式，返回索引: {result}");
                    return result;

                case DataSourceType.DataManager:
                    if (dataManagerObject != null && !string.IsNullOrEmpty(indexDataKey))
                    {
                        try
                        {
                            var type = dataManagerObject.GetType();
                            var getMethod = type.GetMethod("Get");
                            if (getMethod != null)
                            {
                                var genericMethod = getMethod.MakeGenericMethod(typeof(int));
                                var methodResult = genericMethod.Invoke(dataManagerObject, new object[] { indexDataKey, 0 });
                                result = methodResult != null ? (int)methodResult : 0;
                                Debug.Log($"[获取索引值] DataManager模式，从键 '{indexDataKey}' 获取到索引: {result}");
                                return result;
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning($"[获取索引值] DataManager 获取索引失败: {ex.Message}");
                            return 0;
                        }
                    }
                    Debug.LogWarning("[获取索引值] DataManager 模式但参数无效，返回 0");
                    return 0;

                default:
                    Debug.LogWarning($"[获取索引值] 未知的索引类型: {indexType}，返回 0");
                    return 0;
            }
        }

        /// <summary>
        /// 从 TimelineDataManager 获取 Protobuf 对象
        /// </summary>
        private static object GetProtobufObject(object dataManagerObject)
        {
            try
            {
                var type = dataManagerObject.GetType();
                
                if (type.Name == "TimelineDataManager")
                {
                    // 先尝试作为 Property
                    var property = type.GetProperty("genericFishDeadSync");
                    if (property != null)
                    {
                        return property.GetValue(dataManagerObject);
                    }
                    
                    // 再尝试作为 Field
                    var field = type.GetField("genericFishDeadSync");
                    if (field != null)
                    {
                        return field.GetValue(dataManagerObject);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to get Protobuf object: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// 从 Protobuf 对象获取字段值（支持嵌套路径和列表索引）
        /// </summary>
        private static object GetProtobufFieldValue(object protobufObject, string fieldPath, int listIndex, string elementFieldPath, int elementListIndex = 0)
        {
            Debug.Log($"[获取Protobuf字段值] 开始 - 字段路径: '{fieldPath}', 列表索引: {listIndex}, 元素字段路径: '{elementFieldPath}', 元素列表索引: {elementListIndex}");
            
            if (protobufObject == null || string.IsNullOrEmpty(fieldPath))
            {
                Debug.LogWarning("[获取Protobuf字段值] Protobuf对象为空 或 字段路径为空");
                return null;
            }

            try
            {
                var pathParts = fieldPath.Split('.');
                object currentObject = protobufObject;
                
                Debug.Log($"[获取Protobuf字段值] 路径分段: [{string.Join(", ", pathParts)}]");

                // 导航到指定路径
                for (int i = 0; i < pathParts.Length; i++)
                {
                    Debug.Log($"[获取Protobuf字段值] 第 {i+1}/{pathParts.Length} 步: 访问字段 '{pathParts[i]}'");
                    
                    if (currentObject == null)
                    {
                        Debug.LogWarning($"[获取Protobuf字段值] 当前对象为空 在第 {i+1} 步");
                        return null;
                    }

                    var type = currentObject.GetType();
                    Debug.Log($"[获取Protobuf字段值] 当前对象类型: {type.Name}");
                    
                    var property = type.GetProperty(pathParts[i]);

                    if (property == null)
                    {
                        Debug.LogWarning($"Protobuf 字段 '{pathParts[i]}' 在 {type.Name} 中未找到");
                        return null;
                    }

                    var value = property.GetValue(currentObject);
                    Debug.Log($"[获取Protobuf字段值] 字段 '{pathParts[i]}' 值类型: {(value != null ? value.GetType().Name : "null")}");

                    // 如果是最后一个部分
                    if (i == pathParts.Length - 1)
                    {
                        Debug.Log($"[获取Protobuf字段值] 到达路径末端");
                        
                        // 检查是否是列表类型
                        if (IsProtobufList(property.PropertyType))
                        {
                            Debug.Log($"[获取Protobuf字段值] 这是一个列表类型，准备使用索引 {listIndex}");
                            
                            // 从列表获取元素
                            var indexer = property.PropertyType.GetProperty("Item");
                            if (indexer != null && value != null)
                            {
                                var count = property.PropertyType.GetProperty("Count");
                                int listSize = count != null ? (int)count.GetValue(value) : 0;
                                Debug.Log($"[获取Protobuf字段值] 列表大小: {listSize}");

                                if (listIndex >= 0 && listIndex < listSize)
                                {
                                    var element = indexer.GetValue(value, new object[] { listIndex });
                                    Debug.Log($"[获取Protobuf字段值] ✅ 成功获取索引 {listIndex} 的元素，类型: {(element != null ? element.GetType().Name : "null")}");
                                    
                                    // 如果有 elementFieldPath，继续访问元素的字段
                                    if (!string.IsNullOrEmpty(elementFieldPath) && element != null)
                                    {
                                        Debug.Log($"[获取Protobuf字段值] 继续访问元素的字段: '{elementFieldPath}', 使用元素索引: {elementListIndex}");
                                        return GetProtobufFieldValue(element, elementFieldPath, elementListIndex, null, 0);
                                    }
                                    
                                    Debug.Log($"[获取Protobuf字段值] 返回元素值: {element}");
                                    return element;
                                }
                                else
                                {
                                    Debug.LogWarning($"Protobuf 列表索引 {listIndex} 超出范围 (0-{listSize-1})，字段路径: '{fieldPath}'");
                                }
                            }
                            return null;
                        }
                        else
                        {
                            Debug.Log($"[获取Protobuf字段值] 不是列表，直接返回值: {value}");
                            return value;
                        }
                    }
                    else
                    {
                        // 继续导航到下一层
                        Debug.Log($"[获取Protobuf字段值] 继续导航到下一层");
                        currentObject = value;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"获取 Protobuf 字段 '{fieldPath}' 失败: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// 检查类型是否为 Protobuf RepeatedField
        /// </summary>
        private static bool IsProtobufList(Type type)
        {
            if (type == null)
                return false;

            if (type.IsGenericType)
            {
                var genericTypeDef = type.GetGenericTypeDefinition();
                return genericTypeDef.Name.Contains("RepeatedField");
            }

            return false;
        }
    }
}
