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
            // 如果没有 DataManager，强制使用 Direct 类型
            if (cachedDataManager == null && dataType != DataSourceType.Direct)
            {
                return directValue ?? "";
            }
            
            return StringDataSourceAccessor.GetString(this, cachedDataManager);
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
            if (source == null)
                return "";
                
            switch (source.dataType)
            {
                case DataSourceType.Direct:
                    return source.directValue ?? "";

                case DataSourceType.DataManager:
                    if (dataManagerObject != null && !string.IsNullOrEmpty(source.dataKey))
                    {
                        int resolvedIndex = GetIndexValue(source.indexType, source.indexNumericValue, source.indexDataKey, dataManagerObject);
                        return GetStringFromDataManager(dataManagerObject, source.dataKey, resolvedIndex, source.directValue);
                    }
                    return source.directValue ?? "";

                case DataSourceType.Protobuf:
                    if (dataManagerObject != null && !string.IsNullOrEmpty(source.protobufField))
                    {
                        int resolvedIndex = GetIndexValue(source.indexType, source.indexNumericValue, source.indexDataKey, dataManagerObject);
                        var protobufObject = GetProtobufObject(dataManagerObject);
                        
                        if (protobufObject != null)
                        {
                            var value = GetProtobufFieldValue(protobufObject, source.protobufField, resolvedIndex, source.elementField);
                            return value != null ? value.ToString() : (source.directValue ?? "");
                        }
                    }
                    return source.directValue ?? "";

                default:
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
            switch (indexType)
            {
                case DataSourceType.Direct:
                    return indexNumericValue;

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
                                var result = genericMethod.Invoke(dataManagerObject, new object[] { indexDataKey, 0 });
                                return result != null ? (int)result : 0;
                            }
                        }
                        catch
                        {
                            return 0;
                        }
                    }
                    return 0;

                default:
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
        private static object GetProtobufFieldValue(object protobufObject, string fieldPath, int listIndex, string elementFieldPath)
        {
            if (protobufObject == null || string.IsNullOrEmpty(fieldPath))
                return null;

            try
            {
                var pathParts = fieldPath.Split('.');
                object currentObject = protobufObject;

                // 导航到指定路径
                for (int i = 0; i < pathParts.Length; i++)
                {
                    if (currentObject == null)
                        return null;

                    var type = currentObject.GetType();
                    var property = type.GetProperty(pathParts[i]);

                    if (property == null)
                    {
                        Debug.LogWarning($"Protobuf field '{pathParts[i]}' not found in {type.Name}");
                        return null;
                    }

                    var value = property.GetValue(currentObject);

                    // 如果是最后一个部分
                    if (i == pathParts.Length - 1)
                    {
                        // 检查是否是列表类型
                        if (IsProtobufList(property.PropertyType))
                        {
                            // 从列表获取元素
                            var indexer = property.PropertyType.GetProperty("Item");
                            if (indexer != null && value != null)
                            {
                                var count = property.PropertyType.GetProperty("Count");
                                int listSize = count != null ? (int)count.GetValue(value) : 0;

                                if (listIndex >= 0 && listIndex < listSize)
                                {
                                    var element = indexer.GetValue(value, new object[] { listIndex });
                                    
                                    // 如果有 elementFieldPath，继续访问元素的字段
                                    if (!string.IsNullOrEmpty(elementFieldPath) && element != null)
                                    {
                                        return GetProtobufFieldValue(element, elementFieldPath, 0, null);
                                    }
                                    
                                    return element;
                                }
                                else
                                {
                                    Debug.LogWarning($"Protobuf list index {listIndex} out of range (0-{listSize-1}) for field '{fieldPath}'");
                                }
                            }
                            return null;
                        }
                        else
                        {
                            return value;
                        }
                    }
                    else
                    {
                        // 继续导航到下一层
                        currentObject = value;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to get Protobuf field '{fieldPath}': {ex.Message}");
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
