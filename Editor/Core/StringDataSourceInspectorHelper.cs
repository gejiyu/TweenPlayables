using UnityEngine;
using TweenPlayables;

namespace UnityEditor.Timeline
{
    /// <summary>
    /// StringDataSource Inspector 辅助类
    /// 参考 ConditionalPlayableAssetInspectorHelper 的设计
    /// 用于在 Inspector 中绘制 StringDataSource 的 UI
    /// </summary>
    public static class StringDataSourceInspectorHelper
    {
        /// <summary>
        /// UI 样式定义
        /// </summary>
        public static class Styles
        {
            public static readonly GUIContent DataType = EditorGUIUtility.TrTextContent("Type", "Data source type");
            public static readonly GUIContent DirectValue = EditorGUIUtility.TrTextContent("Value", "Direct string value");
            public static readonly GUIContent DataKey = EditorGUIUtility.TrTextContent("Data Key", "TimelineDataManager key");
            public static readonly GUIContent ProtobufField = EditorGUIUtility.TrTextContent("Protobuf Field", "Protobuf field path");
            public static readonly GUIContent IndexType = EditorGUIUtility.TrTextContent("Index Source", "Index data source type");
            public static readonly GUIContent IndexNumericValue = EditorGUIUtility.TrTextContent("Index", "Numeric index value");
            public static readonly GUIContent IndexDataKey = EditorGUIUtility.TrTextContent("Index Key", "Index from DataManager");
            
            public static readonly string[] DataSourceTypeNames = { "Direct", "Data Manager", "Protobuf" };
            public static readonly string[] IndexSourceTypeNames = { "Numeric", "Data Manager" };
        }

        /// <summary>
        /// 绘制 StringDataSource 的完整 UI
        /// </summary>
        /// <param name="property">StringDataSource 属性</param>
        /// <param name="label">标签</param>
        public static void DrawStringDataSourceGUI(SerializedProperty property, GUIContent label)
        {
            if (property == null)
                return;

            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            var dataTypeProperty = property.FindPropertyRelative("dataType");
            var directValueProperty = property.FindPropertyRelative("directValue");
            var dataKeyProperty = property.FindPropertyRelative("dataKey");
            var protobufFieldProperty = property.FindPropertyRelative("protobufField");
            var elementFieldProperty = property.FindPropertyRelative("elementField");
            var indexTypeProperty = property.FindPropertyRelative("indexType");
            var indexNumericValueProperty = property.FindPropertyRelative("indexNumericValue");
            var indexDataKeyProperty = property.FindPropertyRelative("indexDataKey");

            // 数据源类型选择
            dataTypeProperty.intValue = EditorGUILayout.Popup(Styles.DataType, dataTypeProperty.intValue, Styles.DataSourceTypeNames);

            EditorGUI.indentLevel++;

            // 根据数据源类型显示不同的字段
            switch (dataTypeProperty.intValue)
            {
                case 0: // Direct
                    EditorGUILayout.PropertyField(directValueProperty, Styles.DirectValue);
                    break;

                case 1: // DataManager
                    DrawDataManagerKeyDropdownLayout(dataKeyProperty, indexTypeProperty, indexNumericValueProperty, indexDataKeyProperty);
                    break;

                case 2: // Protobuf
                    DrawProtobufFieldDropdownLayout(protobufFieldProperty);
                    // 只有当 Protobuf 字段是列表时才显示索引配置
                    if (IsProtobufFieldList(protobufFieldProperty.stringValue))
                    {
                        DrawIndexGUILayout(indexTypeProperty, indexNumericValueProperty, indexDataKeyProperty);
                        
                        // 如果 List 元素是 Protobuf Message，显示元素字段选择
                        if (IsProtobufListElementMessage(protobufFieldProperty.stringValue))
                        {
                            DrawElementFieldDropdownLayout(protobufFieldProperty.stringValue, elementFieldProperty);
                        }
                        
                        // 显示索引后的最终值
                        DrawProtobufFinalValue(protobufFieldProperty.stringValue, elementFieldProperty.stringValue,
                                              indexTypeProperty.intValue, indexNumericValueProperty.intValue, indexDataKeyProperty.stringValue);
                    }
                    break;
            }

            EditorGUI.indentLevel--;
            EditorGUI.indentLevel--;
        }



        /// <summary>
        /// 绘制紧凑版 StringDataSource UI（用于 PropertyDrawer）
        /// </summary>
        /// <param name="position">绘制区域</param>
        /// <param name="property">StringDataSource 属性</param>
        /// <param name="label">标签</param>
        /// <returns>下一个控件的起始位置</returns>
        public static Rect DrawStringDataSourceGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property == null)
                return position;

            var dataTypeProperty = property.FindPropertyRelative("dataType");
            var directValueProperty = property.FindPropertyRelative("directValue");
            var dataKeyProperty = property.FindPropertyRelative("dataKey");
            var protobufFieldProperty = property.FindPropertyRelative("protobufField");
            var elementFieldProperty = property.FindPropertyRelative("elementField");
            var indexTypeProperty = property.FindPropertyRelative("indexType");
            var indexNumericValueProperty = property.FindPropertyRelative("indexNumericValue");
            var indexDataKeyProperty = property.FindPropertyRelative("indexDataKey");

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            Rect currentRect = new Rect(position.x, position.y, position.width, lineHeight);

            // 使用 PropertyField 样式：标签在左，控件在右
            var labelWidth = EditorGUIUtility.labelWidth;
            
            // 第一行：主标签 + 数据源类型下拉框
            var labelRect = new Rect(currentRect.x, currentRect.y, labelWidth, lineHeight);
            var controlRect = new Rect(currentRect.x + labelWidth, currentRect.y, currentRect.width - labelWidth, lineHeight);
            
            EditorGUI.LabelField(labelRect, label);
            dataTypeProperty.intValue = EditorGUI.Popup(controlRect, dataTypeProperty.intValue, Styles.DataSourceTypeNames);
            currentRect.y += lineHeight + spacing;

            // 后续字段需要缩进
            float indentOffset = 15f;
            currentRect.x += indentOffset;
            currentRect.width -= indentOffset;
            
            // 根据类型显示字段
            switch (dataTypeProperty.intValue)
            {
                case 0: // Direct
                    EditorGUI.PropertyField(currentRect, directValueProperty, Styles.DirectValue);
                    currentRect.y += lineHeight + spacing;
                    break;

                case 1: // DataManager
                    currentRect = DrawDataManagerKeyDropdown(currentRect, dataKeyProperty, indexTypeProperty, indexNumericValueProperty, indexDataKeyProperty, lineHeight, spacing);
                    break;

                case 2: // Protobuf
                    currentRect = DrawProtobufFieldDropdown(currentRect, protobufFieldProperty, lineHeight, spacing);
                    // 只有当 Protobuf 字段是列表时才显示索引配置
                    if (IsProtobufFieldList(protobufFieldProperty.stringValue))
                    {
                        currentRect = DrawIndexGUI(currentRect, indexTypeProperty, indexNumericValueProperty, indexDataKeyProperty, lineHeight, spacing);
                        
                        // 如果 List 元素是 Protobuf Message，显示元素字段选择
                        if (IsProtobufListElementMessage(protobufFieldProperty.stringValue))
                        {
                            currentRect = DrawElementFieldDropdown(currentRect, protobufFieldProperty.stringValue, elementFieldProperty, lineHeight, spacing);
                        }
                        
                        // 显示索引后的最终值
                        currentRect = DrawProtobufFinalValue(currentRect, protobufFieldProperty.stringValue, elementFieldProperty.stringValue,
                                                            indexTypeProperty.intValue, indexNumericValueProperty.intValue, indexDataKeyProperty.stringValue, lineHeight, spacing);
                    }
                    break;
            }
            
            // 恢复原始位置
            currentRect.x -= indentOffset;
            currentRect.width += indentOffset;
            
            return currentRect;
        }

        /// <summary>
        /// 绘制索引配置 UI（EditorGUILayout 版本，用于 Protobuf）
        /// </summary>
        private static void DrawIndexGUILayout(SerializedProperty indexTypeProperty, SerializedProperty indexNumericValueProperty, SerializedProperty indexDataKeyProperty)
        {
            indexTypeProperty.intValue = EditorGUILayout.Popup("Index Source", indexTypeProperty.intValue, Styles.IndexSourceTypeNames);

            EditorGUI.indentLevel++;
            if (indexTypeProperty.intValue == 0) // Numeric
            {
                EditorGUILayout.PropertyField(indexNumericValueProperty, Styles.IndexNumericValue);
            }
            else if (indexTypeProperty.intValue == 1) // DataManager
            {
                DrawSimpleDataKeyDropdown(indexDataKeyProperty);
            }
            EditorGUI.indentLevel--;
        }

        /// <summary>
        /// 绘制索引配置 UI（Rect 版本，用于 Protobuf）
        /// </summary>
        private static Rect DrawIndexGUI(Rect currentRect, SerializedProperty indexTypeProperty, SerializedProperty indexNumericValueProperty, SerializedProperty indexDataKeyProperty, float lineHeight, float spacing)
        {
            var labelWidth = EditorGUIUtility.labelWidth;
            var labelRect = new Rect(currentRect.x, currentRect.y, labelWidth, lineHeight);
            var popupRect = new Rect(currentRect.x + labelWidth, currentRect.y, currentRect.width - labelWidth, lineHeight);
            
            EditorGUI.LabelField(labelRect, Styles.IndexType);
            indexTypeProperty.intValue = EditorGUI.Popup(popupRect, indexTypeProperty.intValue, Styles.IndexSourceTypeNames);
            currentRect.y += lineHeight + spacing;

            if (indexTypeProperty.intValue == 0) // Numeric
            {
                EditorGUI.PropertyField(currentRect, indexNumericValueProperty, Styles.IndexNumericValue);
                currentRect.y += lineHeight + spacing;
            }
            else if (indexTypeProperty.intValue == 1) // DataManager
            {
                currentRect = DrawSimpleDataKeyDropdown(currentRect, indexDataKeyProperty, lineHeight, spacing);
            }
            
            return currentRect;
        }

        /// <summary>
        /// 计算 StringDataSource UI 的高度
        /// </summary>
        public static float GetStringDataSourceHeight(SerializedProperty property)
        {
            if (property == null)
                return 0;

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            float totalHeight = 0;

            var dataTypeProperty = property.FindPropertyRelative("dataType");
            var indexTypeProperty = property.FindPropertyRelative("indexType");
            var indexDataKeyProperty = property.FindPropertyRelative("indexDataKey");
            
            // 主标签+数据类型（合并为一行）
            totalHeight += (lineHeight + spacing);

            // 根据数据类型计算额外高度
            if (dataTypeProperty.intValue == 0) // Direct
            {
                // Direct Value field
                totalHeight += (lineHeight + spacing);
            }
            // DataManager: 检查是否为列表
            else if (dataTypeProperty.intValue == 1) // DataManager
            {
                var dataKeyProperty = property.FindPropertyRelative("dataKey");
                
                // Data Key dropdown
                totalHeight += (lineHeight + spacing);
                
                if (!string.IsNullOrEmpty(dataKeyProperty.stringValue))
                {
                    var dataManager = UnityEngine.Object.FindObjectOfType<UnityEngine.Timeline.TimelineDataManager>();
                    if (dataManager != null)
                    {
                        var keyValue = GetDataManagerValue(dataManager, dataKeyProperty.stringValue);
                        if (keyValue != null)
                        {
                            // Type: xxx
                            totalHeight += (lineHeight + spacing);
                            
                            var valueInfo = GetValueTypeInfo(keyValue);
                            if (valueInfo.isList)
                            {
                                // List Size: xxx
                                totalHeight += (lineHeight + spacing);
                                
                                // Index Source dropdown
                                totalHeight += (lineHeight + spacing);
                                
                                if (indexTypeProperty.intValue == 0) // Numeric
                                {
                                    // Index Value field
                                    totalHeight += (lineHeight + spacing);
                                    
                                    // Value at [x]: xxx
                                    totalHeight += (lineHeight + spacing);
                                }
                                else if (indexTypeProperty.intValue == 1) // DataManager
                                {
                                    // Index Key dropdown (来自DrawSimpleDataKeyDropdown)
                                    totalHeight += (lineHeight + spacing);
                                    
                                    if (!string.IsNullOrEmpty(indexDataKeyProperty.stringValue))
                                    {
                                        var indexKeyValue = GetDataManagerValue(dataManager, indexDataKeyProperty.stringValue);
                                        if (indexKeyValue != null)
                                        {
                                            // Index Key Value display
                                            totalHeight += (lineHeight + spacing);
                                        }
                                        
                                        if (indexKeyValue is int)
                                        {
                                            // Index from DataManager: xxx
                                            totalHeight += (lineHeight + spacing);
                                            
                                            // Value at [x]: xxx 或 Index out of range
                                            totalHeight += (lineHeight + spacing);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // Current Value: xxx
                                totalHeight += (lineHeight + spacing);
                            }
                        }
                    }
                }
            }
            // Protobuf: 需要计算多层级导航的高度
            else if (dataTypeProperty.intValue == 2) // Protobuf
            {
                var protobufFieldProperty = property.FindPropertyRelative("protobufField");
                var elementFieldProperty = property.FindPropertyRelative("elementField");
                
                // 计算 Protobuf 字段导航的实际高度
                int protobufLines = CalculateProtobufFieldHeight(protobufFieldProperty.stringValue);
                totalHeight += (lineHeight + spacing) * protobufLines;
                
                // 如果是列表类型，还需要索引配置的高度
                if (IsProtobufFieldList(protobufFieldProperty.stringValue))
                {
                    // Index Source dropdown
                    totalHeight += (lineHeight + spacing);
                    
                    if (indexTypeProperty.intValue == 0) // Numeric
                    {
                        // Index value field
                        totalHeight += (lineHeight + spacing);
                    }
                    else if (indexTypeProperty.intValue == 1) // DataManager
                    {
                        // Index Key dropdown
                        totalHeight += (lineHeight + spacing);
                        
                        // 如果选择了 Index Key，显示 Index Key Value
                        if (!string.IsNullOrEmpty(indexDataKeyProperty.stringValue))
                        {
                            var dataManager = UnityEngine.Object.FindObjectOfType<UnityEngine.Timeline.TimelineDataManager>();
                            if (dataManager != null)
                            {
                                var keyValue = GetDataManagerValue(dataManager, indexDataKeyProperty.stringValue);
                                if (keyValue != null)
                                {
                                    totalHeight += (lineHeight + spacing); // Index Key Value
                                }
                            }
                        }
                    }
                    
                    // 如果 List 元素是 Protobuf Message，添加 Element Field 选择的高度
                    if (IsProtobufListElementMessage(protobufFieldProperty.stringValue))
                    {
                        totalHeight += (lineHeight + spacing); // Element Field dropdown
                    }
                    
                    // 最终值显示
                    totalHeight += (lineHeight + spacing);
                }
            }

            return totalHeight;
        }
        
        /// <summary>
        /// 计算 Protobuf 字段导航需要的行数
        /// </summary>
        private static int CalculateProtobufFieldHeight(string fieldPath)
        {
            var dataManager = UnityEngine.Object.FindObjectOfType<UnityEngine.Timeline.TimelineDataManager>();
            if (dataManager == null || dataManager.genericFishDeadSync == null)
            {
                return 1; // 只显示一个输入框
            }
            
            if (string.IsNullOrEmpty(fieldPath))
            {
                return 1; // 只显示第一级选择
            }
            
            var pathParts = fieldPath.Split('.');
            object currentObject = dataManager.genericFishDeadSync;
            System.Type currentType = currentObject.GetType();
            int lineCount = 0;
            
            object finalValue = null;
            System.Type finalType = null;
            
            try
            {
                for (int depth = 0; depth < pathParts.Length && depth < 10; depth++)
                {
                    var availableFields = GetProtobufFieldsAtLevel(currentType);
                    if (availableFields.Length == 0)
                        break;
                    
                    // 每一级字段选择占一行
                    lineCount++;
                    
                    var fieldName = pathParts[depth];
                    var property = currentType.GetProperty(fieldName);
                    if (property == null)
                        break;
                    
                    var value = currentObject != null ? property.GetValue(currentObject) : null;
                    var valueType = property.PropertyType;
                    
                    // 保存最终的值和类型
                    finalValue = value;
                    finalType = valueType;
                    
                    // 如果是叶子节点（string/numeric）
                    if (valueType == typeof(string) || IsNumericType(valueType))
                    {
                        break;
                    }
                    // 如果是列表 - 停止
                    else if (IsProtobufList(valueType))
                    {
                        break;
                    }
                    // 如果是嵌套消息，继续下一层
                    else if (IsProtobufMessage(valueType))
                    {
                        currentObject = value;
                        currentType = valueType;
                    }
                    else
                    {
                        break;
                    }
                }
                
                // 统一显示 Type 和 Current Value（只要有类型信息）
                if (finalType != null)
                {
                    lineCount++; // Type 信息
                    
                    // 对于非列表类型，如果有值则显示 Current Value
                    if (!IsProtobufList(finalType) && finalValue != null)
                    {
                        lineCount++; // Current Value
                    }
                }
            }
            catch
            {
                // 出错时返回保守估计
                return pathParts.Length + 2;
            }
            
            return lineCount > 0 ? lineCount : 1;
        }
        
        /// <summary>
        /// 绘制 DataManager Key 下拉选择（Rect 版本，完整版本）
        /// </summary>
        private static Rect DrawDataManagerKeyDropdown(Rect currentRect, SerializedProperty dataKeyProperty, 
                                                      SerializedProperty indexTypeProperty, SerializedProperty indexNumericValueProperty, 
                                                      SerializedProperty indexDataKeyProperty, float lineHeight, float spacing)
        {
            var dataManager = UnityEngine.Object.FindObjectOfType<UnityEngine.Timeline.TimelineDataManager>();
            
            if (dataManager != null)
            {
                var availableKeys = GetAvailableDataKeys(dataManager);
                if (availableKeys != null && availableKeys.Length > 0)
                {
                    var currentKey = dataKeyProperty.stringValue;
                    var currentIndex = System.Array.IndexOf(availableKeys, currentKey);
                    if (currentIndex < 0) currentIndex = 0;
                    
                    var labelWidth = EditorGUIUtility.labelWidth;
                    var labelRect = new Rect(currentRect.x, currentRect.y, labelWidth, lineHeight);
                    var popupRect = new Rect(currentRect.x + labelWidth, currentRect.y, currentRect.width - labelWidth - 30, lineHeight);
                    var buttonRect = new Rect(currentRect.x + currentRect.width - 25, currentRect.y, 25, lineHeight);
                    
                    EditorGUI.LabelField(labelRect, Styles.DataKey);
                    var newIndex = EditorGUI.Popup(popupRect, currentIndex, availableKeys);
                    
                    if (newIndex >= 0 && newIndex < availableKeys.Length)
                    {
                        dataKeyProperty.stringValue = availableKeys[newIndex];
                    }
                    
                    // 刷新按钮
                    if (GUI.Button(buttonRect, "↻"))
                    {
                        EditorUtility.SetDirty(dataManager);
                    }
                    
                    currentRect.y += lineHeight + spacing;
                    
                    // 显示当前选中key的值和类型信息
                    if (!string.IsNullOrEmpty(currentKey))
                    {
                        var keyValue = GetDataManagerValue(dataManager, currentKey);
                        if (keyValue != null)
                        {
                            var valueInfo = GetValueTypeInfo(keyValue);
                            EditorGUI.LabelField(currentRect, $"Type: {valueInfo.typeName}", EditorStyles.miniLabel);
                            currentRect.y += lineHeight + spacing;
                            
                            // 如果是列表或数组，显示索引数据源选择
                            if (valueInfo.isList)
                            {
                                EditorGUI.LabelField(currentRect, $"List Size: {valueInfo.listSize}", EditorStyles.miniLabel);
                                currentRect.y += lineHeight + spacing;
                                
                                // 索引数据源类型选择
                                labelRect = new Rect(currentRect.x, currentRect.y, labelWidth, lineHeight);
                                popupRect = new Rect(currentRect.x + labelWidth, currentRect.y, currentRect.width - labelWidth, lineHeight);
                                
                                EditorGUI.LabelField(labelRect, new GUIContent("Index Source"));
                                indexTypeProperty.intValue = EditorGUI.Popup(popupRect, indexTypeProperty.intValue, Styles.IndexSourceTypeNames);
                                currentRect.y += lineHeight + spacing;
                                
                                if (indexTypeProperty.intValue == 0) // Numeric
                                {
                                    indexNumericValueProperty.intValue = EditorGUI.IntField(currentRect, new GUIContent("Index Value"), indexNumericValueProperty.intValue);
                                    
                                    // 限制索引范围
                                    if (indexNumericValueProperty.intValue < 0)
                                        indexNumericValueProperty.intValue = 0;
                                    if (indexNumericValueProperty.intValue >= valueInfo.listSize)
                                        indexNumericValueProperty.intValue = valueInfo.listSize - 1;
                                    
                                    currentRect.y += lineHeight + spacing;
                                        
                                    // 显示当前索引的值
                                    if (indexNumericValueProperty.intValue >= 0 && indexNumericValueProperty.intValue < valueInfo.listSize)
                                    {
                                        var indexValue = GetListValueAtIndex(keyValue, indexNumericValueProperty.intValue);
                                        EditorGUI.LabelField(currentRect, $"Value at [{indexNumericValueProperty.intValue}]: {indexValue}", EditorStyles.miniLabel);
                                        currentRect.y += lineHeight + spacing;
                                    }
                                }
                                else // DataManager
                                {
                                    // 索引键选择（简化版，不需要递归处理）
                                    currentRect = DrawSimpleDataKeyDropdown(currentRect, indexDataKeyProperty, lineHeight, spacing);
                                    
                                    // 如果选择了索引键，尝试显示实际索引值和对应的列表值
                                    if (!string.IsNullOrEmpty(indexDataKeyProperty.stringValue))
                                    {
                                        var indexFromData = GetDataManagerValue(dataManager, indexDataKeyProperty.stringValue);
                                        if (indexFromData != null && indexFromData is int actualIndex)
                                        {
                                            EditorGUI.LabelField(currentRect, $"Index from DataManager: {actualIndex}", EditorStyles.miniLabel);
                                            currentRect.y += lineHeight + spacing;
                                            
                                            if (actualIndex >= 0 && actualIndex < valueInfo.listSize)
                                            {
                                                var indexValue = GetListValueAtIndex(keyValue, actualIndex);
                                                EditorGUI.LabelField(currentRect, $"Value at [{actualIndex}]: {indexValue}", EditorStyles.miniLabel);
                                                currentRect.y += lineHeight + spacing;
                                            }
                                            else
                                            {
                                                EditorGUI.LabelField(currentRect, "Index out of range", EditorStyles.miniLabel);
                                                currentRect.y += lineHeight + spacing;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                EditorGUI.LabelField(currentRect, $"Current Value: {keyValue}", EditorStyles.miniLabel);
                                currentRect.y += lineHeight + spacing;
                            }
                        }
                    }
                }
                else
                {
                    // 没有可用的 keys，显示文本输入
                    EditorGUI.PropertyField(currentRect, dataKeyProperty, Styles.DataKey);
                    currentRect.y += lineHeight + spacing;
                }
            }
            else
            {
                // 没有找到 DataManager，显示文本输入
                EditorGUI.PropertyField(currentRect, dataKeyProperty, Styles.DataKey);
                currentRect.y += lineHeight + spacing;
            }
            
            return currentRect;
        }
        
        /// <summary>
        /// 简化版数据键下拉选择（用于索引键选择，Rect 版本）
        /// </summary>
        private static Rect DrawSimpleDataKeyDropdown(Rect currentRect, SerializedProperty dataKeyProperty, float lineHeight, float spacing)
        {
            var dataManager = UnityEngine.Object.FindObjectOfType<UnityEngine.Timeline.TimelineDataManager>();
            if (dataManager != null)
            {
                var availableKeys = GetAvailableDataKeys(dataManager);
                if (availableKeys.Length > 0)
                {
                    var currentKey = dataKeyProperty.stringValue;
                    var currentIndex = System.Array.IndexOf(availableKeys, currentKey);
                    if (currentIndex < 0) currentIndex = 0;
                    
                    var labelWidth = EditorGUIUtility.labelWidth;
                    var labelRect = new Rect(currentRect.x, currentRect.y, labelWidth, lineHeight);
                    var popupRect = new Rect(currentRect.x + labelWidth, currentRect.y, currentRect.width - labelWidth - 30, lineHeight);
                    var buttonRect = new Rect(currentRect.x + currentRect.width - 25, currentRect.y, 25, lineHeight);
                    
                    EditorGUI.LabelField(labelRect, new GUIContent("Index Key"));
                    var newIndex = EditorGUI.Popup(popupRect, currentIndex, availableKeys);
                    
                    if (newIndex >= 0 && newIndex < availableKeys.Length)
                    {
                        var selectedKey = availableKeys[newIndex];
                        if (dataKeyProperty.stringValue != selectedKey)
                        {
                            dataKeyProperty.stringValue = selectedKey;
                            dataKeyProperty.serializedObject.ApplyModifiedProperties();
                        }
                    }
                    
                    // 刷新按钮
                    if (GUI.Button(buttonRect, "↻"))
                    {
                        EditorUtility.SetDirty(dataManager);
                    }
                    
                    currentRect.y += lineHeight + spacing;
                    
                    // 显示当前选中索引键的值
                    if (!string.IsNullOrEmpty(dataKeyProperty.stringValue))
                    {
                        var keyValue = GetDataManagerValue(dataManager, dataKeyProperty.stringValue);
                        if (keyValue != null)
                        {
                            EditorGUI.LabelField(currentRect, $"Index Key Value: {keyValue}", EditorStyles.miniLabel);
                            currentRect.y += lineHeight + spacing;
                        }
                    }
                }
                else
                {
                    EditorGUI.PropertyField(currentRect, dataKeyProperty, new GUIContent("Index Key"));
                    currentRect.y += lineHeight + spacing;
                }
            }
            else
            {
                EditorGUI.PropertyField(currentRect, dataKeyProperty, new GUIContent("Index Key"));
                currentRect.y += lineHeight + spacing;
            }
            
            return currentRect;
        }
        
        /// <summary>
        /// 从 TimelineDataManager 获取所有可用的数据键列表
        /// </summary>
        private static string[] GetAvailableDataKeys(UnityEngine.Timeline.TimelineDataManager dataManager)
        {
            if (dataManager == null)
                return new string[0];
            
            var field = typeof(UnityEngine.Timeline.TimelineDataManager).GetField("dataStorage", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                var storage = field.GetValue(dataManager);
                if (storage != null)
                {
                    var keysProperty = storage.GetType().GetProperty("Keys");
                    if (keysProperty != null)
                    {
                        var keys = keysProperty.GetValue(storage) as System.Collections.IEnumerable;
                        if (keys != null)
                        {
                            var keyList = new System.Collections.Generic.List<string>();
                            foreach (var key in keys)
                            {
                                keyList.Add(key.ToString());
                            }
                            return keyList.ToArray();
                        }
                    }
                }
            }
            
            return new string[0];
        }
        
        /// <summary>
        /// 绘制 DataManager Key 下拉选择（EditorGUILayout 版本）
        /// </summary>
        private static void DrawDataManagerKeyDropdownLayout(SerializedProperty dataKeyProperty, SerializedProperty indexTypeProperty, 
                                              SerializedProperty indexNumericValueProperty, SerializedProperty indexDataKeyProperty)
        {
            var dataManager = UnityEngine.Object.FindObjectOfType<UnityEngine.Timeline.TimelineDataManager>();
            
            if (dataManager != null)
            {
                var availableKeys = GetAvailableDataKeys(dataManager);
                if (availableKeys != null && availableKeys.Length > 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    
                    var currentKey = dataKeyProperty.stringValue;
                    var currentIndex = System.Array.IndexOf(availableKeys, currentKey);
                    if (currentIndex < 0) currentIndex = 0;
                    
                    var newIndex = EditorGUILayout.Popup(Styles.DataKey, currentIndex, availableKeys);
                    if (newIndex >= 0 && newIndex < availableKeys.Length)
                    {
                        dataKeyProperty.stringValue = availableKeys[newIndex];
                    }
                    
                    // 刷新按钮
                    if (GUILayout.Button("↻", GUILayout.Width(25)))
                    {
                        EditorUtility.SetDirty(dataManager);
                    }
                    
                    EditorGUILayout.EndHorizontal();
                    
                    // 显示当前选中key的值和类型信息
                    if (!string.IsNullOrEmpty(currentKey))
                    {
                        var keyValue = GetDataManagerValue(dataManager, currentKey);
                        if (keyValue != null)
                        {
                            var valueInfo = GetValueTypeInfo(keyValue);
                            EditorGUILayout.LabelField($"Type: {valueInfo.typeName}", EditorStyles.miniLabel);
                            
                            // 如果是列表或数组，显示索引数据源选择
                            if (valueInfo.isList)
                            {
                                EditorGUILayout.LabelField($"List Size: {valueInfo.listSize}", EditorStyles.miniLabel);
                                
                                // 索引数据源类型选择
                                indexTypeProperty.intValue = EditorGUILayout.Popup("Index Source", indexTypeProperty.intValue, Styles.IndexSourceTypeNames);
                                
                                EditorGUI.indentLevel++;
                                if (indexTypeProperty.intValue == 0) // Numeric
                                {
                                    indexNumericValueProperty.intValue = EditorGUILayout.IntField("Index Value", indexNumericValueProperty.intValue);
                                    
                                    // 限制索引范围
                                    if (indexNumericValueProperty.intValue < 0)
                                        indexNumericValueProperty.intValue = 0;
                                    if (indexNumericValueProperty.intValue >= valueInfo.listSize)
                                        indexNumericValueProperty.intValue = valueInfo.listSize - 1;
                                        
                                    // 显示当前索引的值
                                    if (indexNumericValueProperty.intValue >= 0 && indexNumericValueProperty.intValue < valueInfo.listSize)
                                    {
                                        var indexValue = GetListValueAtIndex(keyValue, indexNumericValueProperty.intValue);
                                        EditorGUILayout.LabelField($"Value at [{indexNumericValueProperty.intValue}]: {indexValue}", EditorStyles.miniLabel);
                                    }
                                }
                                else // DataManager
                                {
                                    // 索引键选择（简化版，不需要递归处理）
                                    DrawSimpleDataKeyDropdown(indexDataKeyProperty);
                                    
                                    // 如果选择了索引键，尝试显示实际索引值和对应的列表值
                                    if (!string.IsNullOrEmpty(indexDataKeyProperty.stringValue))
                                    {
                                        var indexFromData = GetDataManagerValue(dataManager, indexDataKeyProperty.stringValue);
                                        if (indexFromData != null && indexFromData is int actualIndex)
                                        {
                                            EditorGUILayout.LabelField($"Index from DataManager: {actualIndex}", EditorStyles.miniLabel);
                                            
                                            if (actualIndex >= 0 && actualIndex < valueInfo.listSize)
                                            {
                                                var indexValue = GetListValueAtIndex(keyValue, actualIndex);
                                                EditorGUILayout.LabelField($"Value at [{actualIndex}]: {indexValue}", EditorStyles.miniLabel);
                                            }
                                            else
                                            {
                                                EditorGUILayout.LabelField("Index out of range", EditorStyles.miniLabel);
                                            }
                                        }
                                    }
                                }
                                EditorGUI.indentLevel--;
                            }
                            else
                            {
                                EditorGUILayout.LabelField($"Current Value: {keyValue}", EditorStyles.miniLabel);
                            }
                        }
                    }
                }
                else
                {
                    // 没有可用的 keys，显示文本输入
                    EditorGUILayout.PropertyField(dataKeyProperty, Styles.DataKey);
                }
            }
            else
            {
                // 没有找到 DataManager，显示文本输入
                EditorGUILayout.PropertyField(dataKeyProperty, Styles.DataKey);
            }
        }
        
        /// <summary>
        /// 简化版数据键下拉选择（用于索引键选择）
        /// </summary>
        private static void DrawSimpleDataKeyDropdown(SerializedProperty dataKeyProperty)
        {
            var dataManager = UnityEngine.Object.FindObjectOfType<UnityEngine.Timeline.TimelineDataManager>();
            if (dataManager != null)
            {
                var availableKeys = GetAvailableDataKeys(dataManager);
                if (availableKeys.Length > 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    
                    var currentKey = dataKeyProperty.stringValue;
                    var currentIndex = System.Array.IndexOf(availableKeys, currentKey);
                    if (currentIndex < 0) currentIndex = 0;
                    
                    var newIndex = EditorGUILayout.Popup("Index Key", currentIndex, availableKeys);
                    if (newIndex >= 0 && newIndex < availableKeys.Length)
                    {
                        var selectedKey = availableKeys[newIndex];
                        if (dataKeyProperty.stringValue != selectedKey)
                        {
                            dataKeyProperty.stringValue = selectedKey;
                            dataKeyProperty.serializedObject.ApplyModifiedProperties();
                        }
                    }
                    
                    // 刷新按钮
                    if (GUILayout.Button("↻", GUILayout.Width(25)))
                    {
                        EditorUtility.SetDirty(dataManager);
                    }
                    
                    EditorGUILayout.EndHorizontal();
                    
                    // 显示当前选中索引键的值
                    if (!string.IsNullOrEmpty(dataKeyProperty.stringValue))
                    {
                        var keyValue = GetDataManagerValue(dataManager, dataKeyProperty.stringValue);
                        if (keyValue != null)
                        {
                            EditorGUILayout.LabelField($"Index Key Value: {keyValue}", EditorStyles.miniLabel);
                        }
                    }
                }
                else
                {
                    EditorGUILayout.PropertyField(dataKeyProperty, new GUIContent("Index Key"));
                }
            }
            else
            {
                EditorGUILayout.PropertyField(dataKeyProperty, new GUIContent("Index Key"));
            }
        }
        
        /// <summary>
        /// Value type information for Inspector display
        /// </summary>
        private struct ValueTypeInfo
        {
            public string typeName;
            public bool isList;
            public int listSize;
        }
        
        /// <summary>
        /// Gets type information about a value for Inspector display
        /// </summary>
        private static ValueTypeInfo GetValueTypeInfo(object value)
        {
            var info = new ValueTypeInfo();
            
            if (value == null)
            {
                info.typeName = "null";
                info.isList = false;
                info.listSize = 0;
                return info;
            }
            
            var valueType = value.GetType();
            
            // 检查是否是 List<T>
            if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(System.Collections.Generic.List<>))
            {
                var listValue = value as System.Collections.IList;
                info.isList = true;
                info.listSize = listValue != null ? listValue.Count : 0;
                info.typeName = $"List<{valueType.GetGenericArguments()[0].Name}>";
            }
            // 检查是否是数组
            else if (valueType.IsArray)
            {
                var arrayValue = value as System.Array;
                info.isList = true;
                info.listSize = arrayValue != null ? arrayValue.Length : 0;
                info.typeName = $"{valueType.GetElementType().Name}[]";
            }
            // 普通值类型
            else
            {
                info.isList = false;
                info.listSize = 0;
                info.typeName = valueType.Name;
            }
            
            return info;
        }
        
        /// <summary>
        /// 从 TimelineDataManager 获取指定键的值
        /// </summary>
        private static object GetDataManagerValue(UnityEngine.Timeline.TimelineDataManager dataManager, string key)
        {
            if (dataManager == null || string.IsNullOrEmpty(key))
                return null;
            
            try
            {
                var field = typeof(UnityEngine.Timeline.TimelineDataManager).GetField("dataStorage", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (field != null)
                {
                    var storage = field.GetValue(dataManager);
                    if (storage != null)
                    {
                        var indexer = storage.GetType().GetProperty("Item");
                        if (indexer != null)
                        {
                            return indexer.GetValue(storage, new object[] { key });
                        }
                    }
                }
            }
            catch
            {
                // 忽略错误，返回 null
            }
            
            return null;
        }
        
        /// <summary>
        /// 检查 DataManager 中指定键的值是否为列表类型
        /// </summary>
        private static bool IsDataManagerValueList(string dataKey)
        {
            if (string.IsNullOrEmpty(dataKey))
                return false;
            
            var dataManager = UnityEngine.Object.FindObjectOfType<UnityEngine.Timeline.TimelineDataManager>();
            if (dataManager == null)
                return false;
            
            var value = GetDataManagerValue(dataManager, dataKey);
            if (value == null)
                return false;
            
            // 检查是否为列表、数组或 IList
            var type = value.GetType();
            
            // 检查是否为数组
            if (type.IsArray)
                return true;
            
            // 检查是否实现了 IList 接口
            if (typeof(System.Collections.IList).IsAssignableFrom(type))
                return true;
            
            // 检查是否为泛型列表
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(System.Collections.Generic.List<>))
                return true;
            
            return false;
        }
        
        /// <summary>
        /// 获取解析后的索引值
        /// </summary>
        private static int GetResolvedIndex(UnityEngine.Timeline.TimelineDataManager dataManager, int indexType, int numericIndex, string indexDataKey)
        {
            if (indexType == 0) // Numeric
            {
                return numericIndex;
            }
            else // DataManager
            {
                if (!string.IsNullOrEmpty(indexDataKey))
                {
                    var indexValue = GetDataManagerValue(dataManager, indexDataKey);
                    if (indexValue != null && indexValue is int intValue)
                    {
                        return intValue;
                    }
                }
                return 0;
            }
        }
        
        /// <summary>
        /// 从列表中获取指定索引的值
        /// </summary>
        private static object GetListValueAtIndex(object list, int index)
        {
            if (list == null)
                return null;
            
            try
            {
                if (list is System.Collections.IList ilist)
                {
                    if (index >= 0 && index < ilist.Count)
                    {
                        return ilist[index];
                    }
                }
            }
            catch
            {
                // 忽略错误
            }
            
            return null;
        }
        
        /// <summary>
        /// 绘制 Protobuf 字段下拉选择（EditorGUILayout 版本）
        /// </summary>
        private static void DrawProtobufFieldDropdownLayout(SerializedProperty protobufFieldProperty)
        {
            var dataManager = UnityEngine.Object.FindObjectOfType<UnityEngine.Timeline.TimelineDataManager>();
            
            if (dataManager == null || dataManager.genericFishDeadSync == null)
            {
                EditorGUILayout.PropertyField(protobufFieldProperty, Styles.ProtobufField);
                return;
            }
            
            // 解析当前字段路径（例如 "DrawWheelInfo.WheelId"）
            var fieldPath = protobufFieldProperty.stringValue;
            var pathParts = string.IsNullOrEmpty(fieldPath) ? new string[0] : fieldPath.Split('.');
            
            // 导航嵌套字段
            object currentObject = dataManager.genericFishDeadSync;
            System.Type currentType = currentObject.GetType();
            const int MaxDepth = 10;
            
            object finalValue = null;
            System.Type finalType = null;
            
            for (int depth = 0; depth < MaxDepth; depth++)
            {
                // 获取当前层级的可用字段
                var availableFields = GetProtobufFieldsAtLevel(currentType);
                
                if (availableFields.Length == 0)
                    break;
                
                // 当前层级的选择
                string currentSelection = depth < pathParts.Length ? pathParts[depth] : "";
                int currentIndex = System.Array.IndexOf(availableFields, currentSelection);
                int displayIndex = currentIndex >= 0 ? currentIndex : 0;
                
                EditorGUILayout.BeginHorizontal();
                
                string label = depth == 0 ? "Protobuf Field" : $"  → {availableFields[displayIndex]}";
                var newIndex = EditorGUILayout.Popup(depth == 0 ? Styles.ProtobufField : new GUIContent(label), displayIndex, availableFields);
                
                if (depth == 0 && GUILayout.Button("↻", GUILayout.Width(25)))
                {
                    EditorUtility.SetDirty(dataManager);
                }
                
                EditorGUILayout.EndHorizontal();
                
                // 处理选择
                string selectedField = availableFields[newIndex];
                
                // 重建路径
                var newPathParts = new string[depth + 1];
                for (int i = 0; i < depth; i++)
                {
                    newPathParts[i] = pathParts[i];
                }
                newPathParts[depth] = selectedField;
                string newPath = string.Join(".", newPathParts);
                
                bool selectionChanged = (depth >= pathParts.Length) || (pathParts[depth] != selectedField);
                if (selectionChanged && protobufFieldProperty.stringValue != newPath)
                {
                    protobufFieldProperty.stringValue = newPath;
                    pathParts = newPathParts;
                }
                
                // 获取字段信息
                var property = currentType.GetProperty(selectedField);
                if (property == null)
                    break;
                
                var value = currentObject != null ? property.GetValue(currentObject) : null;
                var valueType = property.PropertyType;
                
                // 保存最终的值和类型
                finalValue = value;
                finalType = valueType;
                
                // 检查是否为字符串类型（叶子节点）
                if (valueType == typeof(string) || IsNumericType(valueType))
                {
                    break;
                }
                // 检查是否为列表 - 在列表处停止
                else if (IsProtobufList(valueType))
                {
                    break;
                }
                // 检查是否为嵌套消息
                else if (IsProtobufMessage(valueType))
                {
                    // 即使 value 为 null，我们仍然可以继续导航类型结构
                    currentObject = value;
                    currentType = valueType;
                }
                else
                {
                    break;
                }
            }
            
            // 统一显示最终类型和值（只要有类型信息）
            if (finalType != null)
            {
                // 显示类型信息
                if (IsProtobufList(finalType))
                {
                    EditorGUILayout.LabelField($"Type: List (需要配置索引)", EditorStyles.miniLabel);
                    // List 类型不显示当前值，因为还需要配置索引才能得到最终值
                }
                else
                {
                    // 非列表类型，这就是最终类型
                    EditorGUILayout.LabelField($"Type: {finalType.Name}", EditorStyles.miniLabel);
                    
                    // 显示最终值
                    if (finalValue != null)
                    {
                        EditorGUILayout.LabelField($"Current Value: {finalValue}", EditorStyles.miniLabel);
                    }
                }
            }
        }

        /// <summary>
        /// 绘制 Protobuf 字段下拉选择（Rect 版本，支持多层级导航）
        /// </summary>
        private static Rect DrawProtobufFieldDropdown(Rect currentRect, SerializedProperty protobufFieldProperty, float lineHeight, float spacing)
        {
            var dataManager = UnityEngine.Object.FindObjectOfType<UnityEngine.Timeline.TimelineDataManager>();

            if (dataManager == null || dataManager.genericFishDeadSync == null)
            {
                EditorGUI.PropertyField(currentRect, protobufFieldProperty, Styles.ProtobufField);
                currentRect.y += lineHeight + spacing;
                return currentRect;
            }

            // 解析当前字段路径（例如 "DrawWheelInfo.WheelId"）
            var fieldPath = protobufFieldProperty.stringValue;
            var pathParts = string.IsNullOrEmpty(fieldPath) ? new string[0] : fieldPath.Split('.');
            
            // 导航嵌套字段
            object currentObject = dataManager.genericFishDeadSync;
            System.Type currentType = currentObject.GetType();
            const int MaxDepth = 10;
            
            var labelWidth = EditorGUIUtility.labelWidth;
            
            object finalValue = null;
            System.Type finalType = null;
            
            for (int depth = 0; depth < MaxDepth; depth++)
            {
                // 获取当前层级的可用字段
                var availableFields = GetProtobufFieldsAtLevel(currentType);
                
                if (availableFields.Length == 0)
                    break;
                
                // 当前层级的选择
                string currentSelection = depth < pathParts.Length ? pathParts[depth] : "";
                int currentIndex = System.Array.IndexOf(availableFields, currentSelection);
                int displayIndex = currentIndex >= 0 ? currentIndex : 0;
                
                var labelRect = new Rect(currentRect.x, currentRect.y, labelWidth, lineHeight);
                var popupRect = new Rect(currentRect.x + labelWidth, currentRect.y, currentRect.width - labelWidth - 30, lineHeight);
                var buttonRect = new Rect(currentRect.x + currentRect.width - 25, currentRect.y, 25, lineHeight);
                
                string label = depth == 0 ? "Protobuf Field" : $"  → {availableFields[displayIndex]}";
                EditorGUI.LabelField(labelRect, new GUIContent(label));
                var newIndex = EditorGUI.Popup(popupRect, displayIndex, availableFields);
                
                if (depth == 0 && GUI.Button(buttonRect, "↻"))
                {
                    EditorUtility.SetDirty(dataManager);
                }
                
                currentRect.y += lineHeight + spacing;
                
                // 处理选择
                string selectedField = availableFields[newIndex];
                
                // 重建路径
                var newPathParts = new string[depth + 1];
                for (int i = 0; i < depth; i++)
                {
                    newPathParts[i] = pathParts[i];
                }
                newPathParts[depth] = selectedField;
                string newPath = string.Join(".", newPathParts);
                
                bool selectionChanged = (depth >= pathParts.Length) || (pathParts[depth] != selectedField);
                if (selectionChanged && protobufFieldProperty.stringValue != newPath)
                {
                    protobufFieldProperty.stringValue = newPath;
                    pathParts = newPathParts;
                }
                
                // 获取字段信息
                var property = currentType.GetProperty(selectedField);
                if (property == null)
                    break;
                
                var value = currentObject != null ? property.GetValue(currentObject) : null;
                var valueType = property.PropertyType;
                
                // 保存最终的值和类型
                finalValue = value;
                finalType = valueType;
                
                // 检查是否为字符串或数值类型（叶子节点）
                if (valueType == typeof(string) || IsNumericType(valueType))
                {
                    break;
                }
                // 检查是否为列表 - 在列表处停止
                else if (IsProtobufList(valueType))
                {
                    break;
                }
                // 检查是否为嵌套消息
                else if (IsProtobufMessage(valueType))
                {
                    // 即使 value 为 null，我们仍然可以继续导航类型结构
                    currentObject = value;
                    currentType = valueType;
                }
                else
                {
                    break;
                }
            }
            
            // 统一显示最终类型和值（只要有类型信息）
            if (finalType != null)
            {
                // 显示类型信息
                if (IsProtobufList(finalType))
                {
                    EditorGUI.LabelField(currentRect, $"Type: List (需要配置索引)", EditorStyles.miniLabel);
                    currentRect.y += lineHeight + spacing;
                    // List 类型不显示当前值，因为还需要配置索引才能得到最终值
                }
                else
                {
                    // 非列表类型，这就是最终类型
                    EditorGUI.LabelField(currentRect, $"Type: {finalType.Name}", EditorStyles.miniLabel);
                    currentRect.y += lineHeight + spacing;
                    
                    // 显示最终值
                    if (finalValue != null)
                    {
                        EditorGUI.LabelField(currentRect, $"Current Value: {finalValue}", EditorStyles.miniLabel);
                        currentRect.y += lineHeight + spacing;
                    }
                }
            }

            return currentRect;
        }
        
        /// <summary>
        /// 获取 Protobuf 类型指定层级的可用字段
        /// </summary>
        /// 
        private static string[] GetProtobufFieldsAtLevel(System.Type type)
        {
            if (type == null)
                return new string[0];
            
            var fields = new System.Collections.Generic.List<string>();
            var properties = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            
            foreach (var prop in properties)
            {
                // 跳过 Protobuf 内部属性
                if (prop.Name == "Parser" || prop.Name == "Descriptor")
                    continue;

                var propType = prop.PropertyType;
                if (IsNumericType(propType) || IsProtobufList(propType) || IsProtobufMessage(propType))
                {
                    fields.Add(prop.Name);
                }
            }

            fields.Sort();

            return fields.ToArray();
        }
        
        /// <summary>
        /// 绘制 Protobuf 索引后的最终值（EditorGUILayout 版本）
        /// </summary>
        private static void DrawProtobufFinalValue(string fieldPath, string elementField, int indexType, int indexNumericValue, string indexDataKey)
        {
            var dataManager = UnityEngine.Object.FindObjectOfType<UnityEngine.Timeline.TimelineDataManager>();
            if (dataManager == null || string.IsNullOrEmpty(fieldPath))
                return;
            
            try
            {
                // 分析路径，找到 List 的位置和后续的元素字段
                var (listPath, elementFieldPath) = SplitProtobufPath(dataManager.genericFishDeadSync, fieldPath);
                
                // 如果有单独的 elementField，使用它
                if (!string.IsNullOrEmpty(elementField))
                {
                    elementFieldPath = elementField;
                }
                
                // 获取列表值
                var listValue = string.IsNullOrEmpty(listPath) ? null : GetProtobufFieldValue(dataManager.genericFishDeadSync, listPath);
                if (listValue == null)
                {
                    EditorGUILayout.LabelField("List is null", EditorStyles.miniLabel);
                    return;
                }
                
                // 解析索引
                int actualIndex = 0;
                if (indexType == 0) // Numeric
                {
                    actualIndex = indexNumericValue;
                }
                else // DataManager
                {
                    var indexValue = GetDataManagerValue(dataManager, indexDataKey);
                    if (indexValue != null)
                    {
                        // 尝试转换为 int
                        if (indexValue is int intValue)
                        {
                            actualIndex = intValue;
                        }
                        else if (indexValue is long longValue)
                        {
                            actualIndex = (int)longValue;
                        }
                        else
                        {
                            // 尝试解析字符串或其他类型
                            if (int.TryParse(indexValue.ToString(), out int parsedValue))
                            {
                                actualIndex = parsedValue;
                            }
                        }
                    }
                }
                
                // 获取列表中的元素
                var elementValue = GetListValueAtIndex(listValue, actualIndex);
                if (elementValue == null)
                {
                    EditorGUILayout.LabelField($"Value at [{actualIndex}] is null or out of range", EditorStyles.miniLabel);
                    return;
                }
                
                // 如果有元素字段路径，继续获取字段值
                object finalValue = elementValue;
                if (!string.IsNullOrEmpty(elementFieldPath))
                {
                    finalValue = GetProtobufFieldValue(elementValue, elementFieldPath);
                }
                
                if (finalValue != null)
                {
                    // 如果最终值是 string 或数值类型，直接显示
                    var finalType = finalValue.GetType();
                    if (finalValue is string || IsNumericType(finalType))
                    {
                        EditorGUILayout.LabelField($"Final Value at [{actualIndex}]: {finalValue}", EditorStyles.miniLabel);
                    }
                    else if (IsProtobufMessage(finalType))
                    {
                        // 如果是 Protobuf Message，显示类型提示
                        EditorGUILayout.LabelField($"Value at [{actualIndex}]: {finalType.Name} (Protobuf Message)", EditorStyles.miniLabel);
                        EditorGUILayout.HelpBox("列表元素是 Protobuf Message 对象，请在 Protobuf Field 路径中继续选择具体字段", MessageType.Info);
                    }
                    else
                    {
                        // 其他类型，显示 ToString
                        EditorGUILayout.LabelField($"Final Value at [{actualIndex}]: {finalValue}", EditorStyles.miniLabel);
                    }
                }
                else
                {
                    EditorGUILayout.LabelField($"Final Value at [{actualIndex}] is null", EditorStyles.miniLabel);
                }
            }
            catch (System.Exception ex)
            {
                EditorGUILayout.LabelField($"Error: {ex.Message}", EditorStyles.miniLabel);
            }
        }
        
        /// <summary>
        /// 绘制 Protobuf 索引后的最终值（Rect 版本）
        /// </summary>
        private static Rect DrawProtobufFinalValue(Rect currentRect, string fieldPath, string elementField, int indexType, 
                                                   int indexNumericValue, string indexDataKey, float lineHeight, float spacing)
        {
            var dataManager = UnityEngine.Object.FindObjectOfType<UnityEngine.Timeline.TimelineDataManager>();
            if (dataManager == null || string.IsNullOrEmpty(fieldPath))
                return currentRect;
            
            try
            {
                // 分析路径，找到 List 的位置和后续的元素字段
                var (listPath, elementFieldPath) = SplitProtobufPath(dataManager.genericFishDeadSync, fieldPath);
                
                // 如果有单独的 elementField，使用它
                if (!string.IsNullOrEmpty(elementField))
                {
                    elementFieldPath = elementField;
                }
                
                // 获取列表值
                var listValue = string.IsNullOrEmpty(listPath) ? null : GetProtobufFieldValue(dataManager.genericFishDeadSync, listPath);
                if (listValue == null)
                {
                    EditorGUI.LabelField(currentRect, "List is null", EditorStyles.miniLabel);
                    currentRect.y += lineHeight + spacing;
                    return currentRect;
                }
                
                // 解析索引
                int actualIndex = 0;
                if (indexType == 0) // Numeric
                {
                    actualIndex = indexNumericValue;
                }
                else // DataManager
                {
                    var indexValue = GetDataManagerValue(dataManager, indexDataKey);
                    if (indexValue != null)
                    {
                        // 尝试转换为 int
                        if (indexValue is int intValue)
                        {
                            actualIndex = intValue;
                        }
                        else if (indexValue is long longValue)
                        {
                            actualIndex = (int)longValue;
                        }
                        else
                        {
                            // 尝试解析字符串或其他类型
                            if (int.TryParse(indexValue.ToString(), out int parsedValue))
                            {
                                actualIndex = parsedValue;
                            }
                        }
                    }
                }
                
                // 获取列表中的元素
                var elementValue = GetListValueAtIndex(listValue, actualIndex);
                if (elementValue == null)
                {
                    EditorGUI.LabelField(currentRect, $"Value at [{actualIndex}] is null or out of range", EditorStyles.miniLabel);
                    currentRect.y += lineHeight + spacing;
                    return currentRect;
                }
                
                // 如果有元素字段路径，继续获取字段值
                object finalValue = elementValue;
                if (!string.IsNullOrEmpty(elementFieldPath))
                {
                    finalValue = GetProtobufFieldValue(elementValue, elementFieldPath);
                }
                
                if (finalValue != null)
                {
                    // 如果最终值是 string 或数值类型，直接显示
                    var finalType = finalValue.GetType();
                    if (finalValue is string || IsNumericType(finalType))
                    {
                        EditorGUI.LabelField(currentRect, $"Final Value at [{actualIndex}]: {finalValue}", EditorStyles.miniLabel);
                        currentRect.y += lineHeight + spacing;
                    }
                    else if (IsProtobufMessage(finalType))
                    {
                        // 如果是 Protobuf Message，显示类型提示
                        EditorGUI.LabelField(currentRect, $"Value at [{actualIndex}]: {finalType.Name} (Protobuf Message)", EditorStyles.miniLabel);
                        currentRect.y += lineHeight + spacing;
                        EditorGUI.HelpBox(new Rect(currentRect.x, currentRect.y, currentRect.width, lineHeight * 2), 
                                         "列表元素是 Protobuf Message 对象，请在 Protobuf Field 路径中继续选择具体字段", MessageType.Info);
                        currentRect.y += lineHeight * 2 + spacing;
                    }
                    else
                    {
                        // 其他类型，显示 ToString
                        EditorGUI.LabelField(currentRect, $"Final Value at [{actualIndex}]: {finalValue}", EditorStyles.miniLabel);
                        currentRect.y += lineHeight + spacing;
                    }
                }
                else
                {
                    EditorGUI.LabelField(currentRect, $"Final Value at [{actualIndex}] is null", EditorStyles.miniLabel);
                    currentRect.y += lineHeight + spacing;
                }
            }
            catch (System.Exception ex)
            {
                EditorGUI.LabelField(currentRect, $"Error: {ex.Message}", EditorStyles.miniLabel);
                currentRect.y += lineHeight + spacing;
            }
            
            return currentRect;
        }
        
        /// <summary>
        /// 将 Protobuf 路径分割为 List 路径和元素字段路径
        /// 例如: "DrawWheelInfo.ExcelId" → ("DrawWheelInfo", "ExcelId")
        ///      "SomeField.ListField.NestedField" → ("SomeField.ListField", "NestedField")
        /// </summary>
        private static (string listPath, string elementFieldPath) SplitProtobufPath(object rootObject, string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath))
                return (null, null);
            
            var pathParts = fullPath.Split('.');
            object currentObject = rootObject;
            System.Type currentType = rootObject.GetType();
            
            for (int i = 0; i < pathParts.Length; i++)
            {
                var fieldName = pathParts[i];
                var property = currentType.GetProperty(fieldName);
                if (property == null)
                    return (fullPath, null); // 无法解析，返回完整路径
                
                var valueType = property.PropertyType;
                
                // 如果找到 List，分割路径
                if (IsProtobufList(valueType))
                {
                    var listPath = string.Join(".", pathParts, 0, i + 1);
                    var elementFieldPath = i + 1 < pathParts.Length ? string.Join(".", pathParts, i + 1, pathParts.Length - i - 1) : null;
                    return (listPath, elementFieldPath);
                }
                
                // 继续导航
                var value = currentObject != null ? property.GetValue(currentObject) : null;
                currentObject = value;
                currentType = valueType;
            }
            
            // 没有找到 List，返回完整路径
            return (fullPath, null);
        }
        
        /// <summary>
        /// 获取 Protobuf 字段的值（通过字段路径）
        /// </summary>
        private static object GetProtobufFieldValue(object rootObject, string fieldPath)
        {
            if (rootObject == null || string.IsNullOrEmpty(fieldPath))
                return null;
            
            var pathParts = fieldPath.Split('.');
            object currentObject = rootObject;
            
            foreach (var fieldName in pathParts)
            {
                if (currentObject == null)
                    return null;
                
                var property = currentObject.GetType().GetProperty(fieldName);
                if (property == null)
                    return null;
                
                currentObject = property.GetValue(currentObject);
            }
            
            return currentObject;
        }

        /// <summary>
        /// 获取 List 的元素类型
        /// </summary>
        private static System.Type GetListElementType(System.Type listType)
        {
            if (listType == null || !listType.IsGenericType)
                return null;
            
            var genericArgs = listType.GetGenericArguments();
            return genericArgs.Length > 0 ? genericArgs[0] : null;
        }
        
        /// <summary>
        /// 检查类型是否为数值类型
        /// </summary>
        private static bool IsNumericType(System.Type type)
        {
            return type == typeof(int) || type == typeof(float) || type == typeof(double) ||
                   type == typeof(long) || type == typeof(uint) || type == typeof(ulong) ||
                   type == typeof(short) || type == typeof(ushort) || type == typeof(byte) ||
                   type == typeof(sbyte) || type == typeof(decimal);
        }
        
        /// <summary>
        /// 检查类型是否为 Protobuf 列表
        /// </summary>
        private static bool IsProtobufList(System.Type type)
        {
            if (!type.IsGenericType)
                return false;
            
            var genericTypeDef = type.GetGenericTypeDefinition();
            return genericTypeDef.FullName != null && genericTypeDef.FullName.Contains("RepeatedField");
        }
        
        /// <summary>
        /// 检查类型是否为 Protobuf 消息类型
        /// </summary>
        private static bool IsProtobufMessage(System.Type type)
        {
            // 检查是否有 Descriptor 属性（Protobuf 消息的特征）
            var descriptorProp = type.GetProperty("Descriptor", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            return descriptorProp != null;
        }
        
        /// <summary>
        /// 检查 Protobuf List 的元素是否为 Protobuf Message 类型
        /// </summary>
        private static bool IsProtobufListElementMessage(string listFieldPath)
        {
            if (string.IsNullOrEmpty(listFieldPath))
                return false;
            
            var dataManager = UnityEngine.Object.FindObjectOfType<UnityEngine.Timeline.TimelineDataManager>();
            if (dataManager == null || dataManager.genericFishDeadSync == null)
                return false;
            
            try
            {
                var pathParts = listFieldPath.Split('.');
                object currentObject = dataManager.genericFishDeadSync;
                System.Type currentType = currentObject.GetType();
                
                // 导航到最后一个字段
                for (int i = 0; i < pathParts.Length; i++)
                {
                    var fieldName = pathParts[i];
                    var property = currentType.GetProperty(fieldName);
                    if (property == null)
                        return false;
                    
                    var valueType = property.PropertyType;
                    
                    // 如果是最后一个字段，检查是否为列表且元素是 Protobuf Message
                    if (i == pathParts.Length - 1)
                    {
                        if (IsProtobufList(valueType))
                        {
                            var elementType = GetListElementType(valueType);
                            return elementType != null && IsProtobufMessage(elementType);
                        }
                        return false;
                    }
                    
                    // 继续导航
                    var value = currentObject != null ? property.GetValue(currentObject) : null;
                    if (IsProtobufMessage(valueType))
                    {
                        currentObject = value;
                        currentType = valueType;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch
            {
                // 忽略错误
            }
            
            return false;
        }
        
        /// <summary>
        /// 绘制 List 元素字段下拉选择（EditorGUILayout 版本）
        /// </summary>
        private static void DrawElementFieldDropdownLayout(string listFieldPath, SerializedProperty elementFieldProperty)
        {
            var dataManager = UnityEngine.Object.FindObjectOfType<UnityEngine.Timeline.TimelineDataManager>();
            if (dataManager == null || dataManager.genericFishDeadSync == null)
            {
                EditorGUILayout.PropertyField(elementFieldProperty, new GUIContent("Element Field"));
                return;
            }
            
            // 获取 List 元素类型
            var elementType = GetProtobufListElementType(dataManager.genericFishDeadSync, listFieldPath);
            if (elementType == null)
            {
                EditorGUILayout.PropertyField(elementFieldProperty, new GUIContent("Element Field"));
                return;
            }
            
            // 获取元素类型的字段
            var availableFields = GetProtobufFieldsAtLevel(elementType);
            if (availableFields.Length > 0)
            {
                EditorGUILayout.BeginHorizontal();
                
                var currentField = elementFieldProperty.stringValue;
                var currentIndex = System.Array.IndexOf(availableFields, currentField);
                if (currentIndex < 0) currentIndex = 0;
                
                var newIndex = EditorGUILayout.Popup("Element Field", currentIndex, availableFields);
                if (newIndex >= 0 && newIndex < availableFields.Length)
                {
                    elementFieldProperty.stringValue = availableFields[newIndex];
                }
                
                if (GUILayout.Button("↻", GUILayout.Width(25)))
                {
                    EditorUtility.SetDirty(dataManager);
                }
                
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.PropertyField(elementFieldProperty, new GUIContent("Element Field"));
            }
        }
        
        /// <summary>
        /// 绘制 List 元素字段下拉选择（Rect 版本）
        /// </summary>
        private static Rect DrawElementFieldDropdown(Rect currentRect, string listFieldPath, SerializedProperty elementFieldProperty, float lineHeight, float spacing)
        {
            var dataManager = UnityEngine.Object.FindObjectOfType<UnityEngine.Timeline.TimelineDataManager>();
            if (dataManager == null || dataManager.genericFishDeadSync == null)
            {
                EditorGUI.PropertyField(currentRect, elementFieldProperty, new GUIContent("Element Field"));
                currentRect.y += lineHeight + spacing;
                return currentRect;
            }
            
            // 获取 List 元素类型
            var elementType = GetProtobufListElementType(dataManager.genericFishDeadSync, listFieldPath);
            if (elementType == null)
            {
                EditorGUI.PropertyField(currentRect, elementFieldProperty, new GUIContent("Element Field"));
                currentRect.y += lineHeight + spacing;
                return currentRect;
            }
            
            // 获取元素类型的字段
            var availableFields = GetProtobufFieldsAtLevel(elementType);
            if (availableFields.Length > 0)
            {
                var currentField = elementFieldProperty.stringValue;
                var currentIndex = System.Array.IndexOf(availableFields, currentField);
                if (currentIndex < 0) currentIndex = 0;
                
                var labelWidth = EditorGUIUtility.labelWidth;
                var labelRect = new Rect(currentRect.x, currentRect.y, labelWidth, lineHeight);
                var popupRect = new Rect(currentRect.x + labelWidth, currentRect.y, currentRect.width - labelWidth - 30, lineHeight);
                var buttonRect = new Rect(currentRect.x + currentRect.width - 25, currentRect.y, 25, lineHeight);
                
                EditorGUI.LabelField(labelRect, new GUIContent("Element Field"));
                var newIndex = EditorGUI.Popup(popupRect, currentIndex, availableFields);
                
                if (newIndex >= 0 && newIndex < availableFields.Length)
                {
                    elementFieldProperty.stringValue = availableFields[newIndex];
                }
                
                if (GUI.Button(buttonRect, "↻"))
                {
                    EditorUtility.SetDirty(dataManager);
                }
                
                currentRect.y += lineHeight + spacing;
            }
            else
            {
                EditorGUI.PropertyField(currentRect, elementFieldProperty, new GUIContent("Element Field"));
                currentRect.y += lineHeight + spacing;
            }
            
            return currentRect;
        }

        /// <summary>
        /// 获取 Protobuf List 的元素类型
        /// </summary>
        private static System.Type GetProtobufListElementType(object rootObject, string fieldPath)
        {
            if (rootObject == null || string.IsNullOrEmpty(fieldPath))
                return null;
            
            try
            {
                var pathParts = fieldPath.Split('.');
                object currentObject = rootObject;
                System.Type currentType = rootObject.GetType();
                
                for (int i = 0; i < pathParts.Length; i++)
                {
                    var fieldName = pathParts[i];
                    var property = currentType.GetProperty(fieldName);
                    if (property == null)
                        return null;
                    
                    var valueType = property.PropertyType;
                    
                    // 如果是最后一个字段且是 List，返回元素类型
                    if (i == pathParts.Length - 1)
                    {
                        if (IsProtobufList(valueType))
                        {
                            return GetListElementType(valueType);
                        }
                        return null;
                    }
                    
                    // 继续导航
                    var value = currentObject != null ? property.GetValue(currentObject) : null;
                    currentObject = value;
                    currentType = valueType;
                }
            }
            catch
            {
                return null;
            }
            
            return null;
        }

        /// <summary>
        /// 检查 Protobuf 字段路径最终是否指向列表类型
        /// </summary>
        private static bool IsProtobufFieldList(string fieldPath)
        {
            if (string.IsNullOrEmpty(fieldPath))
                return false;
            
            var dataManager = UnityEngine.Object.FindObjectOfType<UnityEngine.Timeline.TimelineDataManager>();
            if (dataManager == null || dataManager.genericFishDeadSync == null)
                return false;
            
            try
            {
                var pathParts = fieldPath.Split('.');
                object currentObject = dataManager.genericFishDeadSync;
                System.Type currentType = currentObject.GetType();
                
                // 导航到最后一个字段
                for (int i = 0; i < pathParts.Length; i++)
                {
                    var fieldName = pathParts[i];
                    var property = currentType.GetProperty(fieldName);
                    if (property == null)
                        return false;
                    
                    var valueType = property.PropertyType;
                    
                    // 如果是最后一个字段，检查是否为列表
                    if (i == pathParts.Length - 1)
                    {
                        return IsProtobufList(valueType);
                    }
                    
                    // 继续导航
                    var value = currentObject != null ? property.GetValue(currentObject) : null;
                    if (IsProtobufMessage(valueType))
                    {
                        currentObject = value;
                        currentType = valueType;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch
            {
                // 忽略错误
            }
            
            return false;
        }
    }
}
