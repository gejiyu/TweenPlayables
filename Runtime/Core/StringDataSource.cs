using System;
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
        /// <param name="dataManagerObject">TimelineDataManager 对象（可选）</param>
        /// <returns>解析后的字符串值</returns>
        public string GetValue(object dataManagerObject = null)
        {
            return StringDataSourceAccessor.GetString(this, dataManagerObject);
        }
    }

    /// <summary>
    /// 字符串数据源访问器 - 负责从各种数据源获取字符串值
    /// </summary>
    public static class StringDataSourceAccessor
    {
        // 缓存 TimelineDataManager 避免重复查找
        private static UnityEngine.Timeline.TimelineDataManager s_CachedDataManager;
        
        /// <summary>
        /// 从 StringDataSource 获取字符串值
        /// </summary>
        public static string GetString(StringDataSource source, object dataManagerObject = null)
        {
            if (source == null)
                return "";
            
            // 如果需要 DataManager 但没有提供，尝试自动查找
            if (dataManagerObject == null && (source.dataType == DataSourceType.DataManager || source.dataType == DataSourceType.Protobuf))
            {
                dataManagerObject = GetOrFindDataManager(null);
            }
                
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
        /// 获取或查找 TimelineDataManager
        /// </summary>
        private static UnityEngine.Timeline.TimelineDataManager GetOrFindDataManager(object dataManagerObject)
        {
            // 如果直接传递了 TimelineDataManager，使用它并缓存
            if (dataManagerObject is UnityEngine.Timeline.TimelineDataManager manager)
            {
                s_CachedDataManager = manager;
                return manager;
            }
            
            // 否则尝试使用缓存的实例
            if (s_CachedDataManager != null)
            {
                return s_CachedDataManager;
            }
            
            // 最后尝试在场景中查找
            s_CachedDataManager = UnityEngine.Object.FindObjectOfType<UnityEngine.Timeline.TimelineDataManager>();
            return s_CachedDataManager;
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
                            // 检查是否是列表类型
                            if (result is System.Collections.Generic.List<string> stringList)
                            {
                                if (index >= 0 && index < stringList.Count)
                                    return stringList[index];
                                return defaultValue;
                            }
                            else if (result is string[] stringArray)
                            {
                                if (index >= 0 && index < stringArray.Length)
                                    return stringArray[index];
                                return defaultValue;
                            }
                            else
                            {
                                // 直接返回字符串
                                return result.ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to get string from DataManager: {ex.Message}");
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
                    var property = type.GetProperty("genericFishDeadSync");
                    if (property != null)
                    {
                        return property.GetValue(dataManagerObject);
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

        /// <summary>
        /// 在场景中查找 TimelineDataManager
        /// </summary>
        public static object FindTimelineDataManager()
        {
            try
            {
                var allObjects = UnityEngine.Object.FindObjectsOfType<UnityEngine.Component>();
                foreach (var obj in allObjects)
                {
                    if (obj.GetType().Name == "TimelineDataManager")
                    {
                        return obj;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to find TimelineDataManager: {ex.Message}");
            }

            return null;
        }
    }
}
