using UnityEngine;
using UnityEngine.Timeline;
using TweenPlayables;
using Sirenix.Utilities.Editor;
using System.Linq;

namespace UnityEditor.Timeline
{
    /// <summary>
    /// Calculator Inspector 辅助类 - 用于在 TweenCalculatorClip 检视面板中显示计算器配置UI
    /// </summary>
    public static class CalculatorInspectorHelper
    {
        /// <summary>
        /// UI样式定义
        /// </summary>
        public static class Styles
        {
            public static readonly GUIContent IsActive = EditorGUIUtility.TrTextContent("Enable Calculation", "Enable or disable this calculator");
            public static readonly GUIContent CalculationOperator = EditorGUIUtility.TrTextContent("Operator", "Calculation operator");
            public static readonly GUIContent ResultKey = EditorGUIUtility.TrTextContent("Result Key", "TimelineDataManager key to store the result");
            public static readonly GUIContent DataType = EditorGUIUtility.TrTextContent("Type", "Data source type");
            public static readonly GUIContent NumericValue = EditorGUIUtility.TrTextContent("Value", "Numeric value");
            public static readonly GUIContent DataKey = EditorGUIUtility.TrTextContent("Data Key", "TimelineDataManager key");
            
            public static readonly string[] CalculationOperatorNames = { "+", "-", "*", "/", "%", "^", "Min", "Max" };
            public static readonly string[] DataSourceTypeNames = { "Numeric", "Data Manager", "Protobuf" };
            public static readonly string[] IndexSourceTypeNames = { "Numeric", "Data Manager" };
        }

        /// <summary>
        /// 绘制计算器配置UI的主入口方法
        /// </summary>
        /// <param name="serializedObject">包含计算器节点的序列化对象</param>
        public static void DrawCalculatorGUI(SerializedObject serializedObject)
        {
            var calculatorNodeProperty = serializedObject.FindProperty("calculatorNode");
            if (calculatorNodeProperty == null)
                return;

            // Is Active with box
            var isActiveProperty = calculatorNodeProperty.FindPropertyRelative("isActive");
            SirenixEditorGUI.BeginBox();
            EditorGUILayout.PropertyField(isActiveProperty, Styles.IsActive);
            SirenixEditorGUI.EndBox();
            
            if (!isActiveProperty.boolValue)
            {
                SirenixEditorGUI.InfoMessageBox("Calculator is disabled. Enable to configure calculation settings.");
                return;
            }

            EditorGUILayout.Space(10);

            // Left Operand Section
            SirenixEditorGUI.BeginBox("Left Operand");
            DrawOperandGUI(
                calculatorNodeProperty.FindPropertyRelative("leftDataType"),
                calculatorNodeProperty.FindPropertyRelative("leftNumericValue"),
                calculatorNodeProperty.FindPropertyRelative("leftDataKey"),
                calculatorNodeProperty.FindPropertyRelative("leftProtobufField"),
                calculatorNodeProperty.FindPropertyRelative("leftIndexType"),
                calculatorNodeProperty.FindPropertyRelative("leftIndexNumericValue"),
                calculatorNodeProperty.FindPropertyRelative("leftIndexDataKey")
            );
            SirenixEditorGUI.EndBox();
            
            EditorGUILayout.Space(5);
            
            // Calculation Operator with centered display
            var operatorProperty = calculatorNodeProperty.FindPropertyRelative("calculationOperator");
            SirenixEditorGUI.BeginBox();
            operatorProperty.intValue = EditorGUILayout.Popup(Styles.CalculationOperator, operatorProperty.intValue, Styles.CalculationOperatorNames);
            SirenixEditorGUI.EndBox();
            
            EditorGUILayout.Space(5);
            
            // Right Operand Section
            SirenixEditorGUI.BeginBox("Right Operand");
            DrawOperandGUI(
                calculatorNodeProperty.FindPropertyRelative("rightDataType"),
                calculatorNodeProperty.FindPropertyRelative("rightNumericValue"),
                calculatorNodeProperty.FindPropertyRelative("rightDataKey"),
                calculatorNodeProperty.FindPropertyRelative("rightProtobufField"),
                calculatorNodeProperty.FindPropertyRelative("rightIndexType"),
                calculatorNodeProperty.FindPropertyRelative("rightIndexNumericValue"),
                calculatorNodeProperty.FindPropertyRelative("rightIndexDataKey")
            );
            SirenixEditorGUI.EndBox();
            
            EditorGUILayout.Space(10);
            
            // Result Storage Section
            SirenixEditorGUI.BeginBox("Result Storage");
            var resultKeyProperty = calculatorNodeProperty.FindPropertyRelative("resultDataKey");
            DrawResultKeyDropdown(
                resultKeyProperty,
                calculatorNodeProperty.FindPropertyRelative("resultIndexType"),
                calculatorNodeProperty.FindPropertyRelative("resultIndexNumericValue"),
                calculatorNodeProperty.FindPropertyRelative("resultIndexDataKey")
            );
            
            if (string.IsNullOrEmpty(resultKeyProperty.stringValue))
            {
                EditorGUILayout.Space(3);
                SirenixEditorGUI.WarningMessageBox("Result Key is required to store the calculation result.");
            }
            SirenixEditorGUI.EndBox();
        }

        /// <summary>
        /// 绘制操作数数据源选择UI（左操作数或右操作数）
        /// </summary>
        private static void DrawOperandGUI(SerializedProperty dataTypeProperty, SerializedProperty numericValueProperty, 
                                          SerializedProperty dataKeyProperty, SerializedProperty protobufFieldProperty,
                                          SerializedProperty indexTypeProperty, 
                                          SerializedProperty indexNumericValueProperty, SerializedProperty indexDataKeyProperty)
        {
            dataTypeProperty.intValue = EditorGUILayout.Popup(Styles.DataType, dataTypeProperty.intValue, Styles.DataSourceTypeNames);
            
            EditorGUI.indentLevel++;
            if (dataTypeProperty.intValue == 0) // Numeric
            {
                EditorGUILayout.PropertyField(numericValueProperty, Styles.NumericValue);
            }
            else if (dataTypeProperty.intValue == 1) // DataManager
            {
                DrawDataKeyDropdown(dataKeyProperty, indexTypeProperty, indexNumericValueProperty, indexDataKeyProperty);
            }
            else if (dataTypeProperty.intValue == 2) // Protobuf
            {
                DrawProtobufFieldDropdown(protobufFieldProperty, indexTypeProperty, indexNumericValueProperty, indexDataKeyProperty);
            }
            EditorGUI.indentLevel--;
        }

        /// <summary>
        /// 绘制 DataManager 数据键下拉选择框（支持列表索引）
        /// </summary>
        private static void DrawDataKeyDropdown(SerializedProperty dataKeyProperty, SerializedProperty indexTypeProperty, 
                                              SerializedProperty indexNumericValueProperty, SerializedProperty indexDataKeyProperty)
        {
            var dataManager = Object.FindObjectOfType<TimelineDataManager>();
            if (dataManager != null)
            {
                var availableKeys = GetAvailableDataKeys(dataManager);
                if (availableKeys.Length > 0)
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
                    
                    // Refresh button
                    if (GUILayout.Button("↻", GUILayout.Width(25)))
                    {
                        EditorUtility.SetDirty(dataManager);
                    }
                    
                    EditorGUILayout.EndHorizontal();
                    
                    // Display current key value and type info
                    if (!string.IsNullOrEmpty(currentKey))
                    {
                        var keyValue = GetDataManagerValue(dataManager, currentKey);
                        if (keyValue != null)
                        {
                            var valueInfo = GetValueTypeInfo(keyValue);
                            EditorGUILayout.LabelField($"Type: {valueInfo.typeName}", EditorStyles.miniLabel);
                            
                            // If list or array, show index data source selection
                            if (valueInfo.isList)
                            {
                                EditorGUILayout.LabelField($"List Size: {valueInfo.listSize}", EditorStyles.miniLabel);
                                
                                // Index data source type selection
                                indexTypeProperty.intValue = EditorGUILayout.Popup("Index Source", indexTypeProperty.intValue, Styles.IndexSourceTypeNames);
                                
                                EditorGUI.indentLevel++;
                                if (indexTypeProperty.intValue == 0) // Numeric
                                {
                                    indexNumericValueProperty.intValue = EditorGUILayout.IntField("Index Value", indexNumericValueProperty.intValue);
                                    
                                    // Clamp index range
                                    if (indexNumericValueProperty.intValue < 0)
                                        indexNumericValueProperty.intValue = 0;
                                    if (indexNumericValueProperty.intValue >= valueInfo.listSize)
                                        indexNumericValueProperty.intValue = valueInfo.listSize - 1;
                                        
                                    // Show value at current index
                                    if (indexNumericValueProperty.intValue >= 0 && indexNumericValueProperty.intValue < valueInfo.listSize)
                                    {
                                        var indexValue = GetListValueAtIndex(keyValue, indexNumericValueProperty.intValue);
                                        EditorGUILayout.LabelField($"Value at [{indexNumericValueProperty.intValue}]: {indexValue}", EditorStyles.miniLabel);
                                    }
                                }
                                else // DataManager
                                {
                                    DrawSimpleDataKeyDropdown(indexDataKeyProperty);
                                    
                                    // If index key selected, try to show actual index value and corresponding list value
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
                                // Not a list, reset index properties
                                indexTypeProperty.intValue = 0; // Numeric
                                indexNumericValueProperty.intValue = 0;
                                indexDataKeyProperty.stringValue = "";
                            }
                        }
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("No data keys found in TimelineDataManager. Add some data first.", MessageType.Info);
                    EditorGUILayout.PropertyField(dataKeyProperty, Styles.DataKey);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No TimelineDataManager found in scene. Please add one to use data keys.", MessageType.Warning);
                EditorGUILayout.PropertyField(dataKeyProperty, Styles.DataKey);
            }
        }

        /// <summary>
        /// 绘制简化版 DataManager 数据键下拉框（用于索引选择，不支持嵌套）
        /// </summary>
        private static void DrawSimpleDataKeyDropdown(SerializedProperty dataKeyProperty)
        {
            var dataManager = Object.FindObjectOfType<TimelineDataManager>();
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
                        dataKeyProperty.stringValue = availableKeys[newIndex];
                    }
                    
                    if (GUILayout.Button("↻", GUILayout.Width(25)))
                    {
                        EditorUtility.SetDirty(dataManager);
                    }
                    
                    EditorGUILayout.EndHorizontal();
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
        /// 从 TimelineDataManager 获取所有可用的数据键列表
        /// </summary>
        private static string[] GetAvailableDataKeys(TimelineDataManager dataManager)
        {
            if (dataManager == null)
                return new string[0];
            
            try
            {
                var field = typeof(TimelineDataManager).GetField("dataStorage", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (field != null)
                {
                    var dataStorage = field.GetValue(dataManager) as System.Collections.Generic.Dictionary<string, object>;
                    if (dataStorage != null)
                    {
                        var keys = new System.Collections.Generic.List<string>(dataStorage.Keys);
                        keys.Sort();
                        return keys.ToArray();
                    }
                }
            }
            catch (System.Exception)
            {
            }
            
            return new string[0];
        }

        /// <summary>
        /// 通过键名从 TimelineDataManager 获取数据值
        /// </summary>
        private static object GetDataManagerValue(TimelineDataManager dataManager, string key)
        {
            if (dataManager == null || string.IsNullOrEmpty(key))
                return null;
                
            try
            {
                var field = typeof(TimelineDataManager).GetField("dataStorage", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (field != null)
                {
                    var dataStorage = field.GetValue(dataManager) as System.Collections.Generic.Dictionary<string, object>;
                    if (dataStorage != null && dataStorage.ContainsKey(key))
                    {
                        return dataStorage[key];
                    }
                }
            }
            catch (System.Exception)
            {
            }
            
            return null;
        }

        /// <summary>
        /// 值类型信息结构体 - 用于在 Inspector 中显示类型和列表信息
        /// </summary>
        private struct ValueTypeInfo
        {
            public string typeName;    // 类型名称
            public bool isList;        // 是否为列表或数组
            public int listSize;       // 列表大小
        }

        /// <summary>
        /// 获取值的类型信息（用于 Inspector 显示）
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
            
            if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(System.Collections.Generic.List<>))
            {
                var listValue = value as System.Collections.IList;
                info.isList = true;
                info.listSize = listValue != null ? listValue.Count : 0;
                info.typeName = $"List<{valueType.GetGenericArguments()[0].Name}>";
            }
            else if (valueType.IsArray)
            {
                var arrayValue = value as System.Array;
                info.isList = true;
                info.listSize = arrayValue != null ? arrayValue.Length : 0;
                info.typeName = $"{valueType.GetElementType().Name}[]";
            }
            else
            {
                info.isList = false;
                info.listSize = 0;
                info.typeName = valueType.Name;
            }
            
            return info;
        }

        /// <summary>
        /// 从列表或数组中获取指定索引的值
        /// </summary>
        private static object GetListValueAtIndex(object listValue, int index)
        {
            if (listValue == null || index < 0)
                return null;
                
            if (listValue is System.Collections.IList list)
            {
                if (index < list.Count)
                    return list[index];
            }
            else if (listValue is System.Array array)
            {
                if (index < array.Length)
                    return array.GetValue(index);
            }
            
            return null;
        }

        /// <summary>
        /// 绘制结果存储键下拉框（支持新建键和列表索引）
        /// </summary>
        private static void DrawResultKeyDropdown(SerializedProperty dataKeyProperty, SerializedProperty indexTypeProperty, 
                                                  SerializedProperty indexNumericValueProperty, SerializedProperty indexDataKeyProperty)
        {
            var dataManager = Object.FindObjectOfType<TimelineDataManager>();
            
            if (dataManager != null)
            {
                var availableKeys = GetAvailableDataKeys(dataManager);
                
                // Add "New Key..." option at the beginning
                var displayKeys = new string[availableKeys.Length + 1];
                displayKeys[0] = "<New Key...>";
                System.Array.Copy(availableKeys, 0, displayKeys, 1, availableKeys.Length);
                
                var currentKey = dataKeyProperty.stringValue;
                var currentIndex = System.Array.IndexOf(availableKeys, currentKey);
                
                // If current key is not found in available keys, it's a new key
                var displayIndex = currentIndex >= 0 ? currentIndex + 1 : 0;
                
                EditorGUILayout.BeginHorizontal();
                var newDisplayIndex = EditorGUILayout.Popup(Styles.ResultKey, displayIndex, displayKeys);
                
                // Refresh button
                if (GUILayout.Button("↻", GUILayout.Width(25)))
                {
                    EditorUtility.SetDirty(dataManager);
                }
                EditorGUILayout.EndHorizontal();
                
                // Handle selection change
                if (newDisplayIndex != displayIndex)
                {
                    if (newDisplayIndex == 0)
                    {
                        // User selected "New Key..." - clear the current value to allow manual input
                        if (string.IsNullOrEmpty(currentKey) || currentIndex >= 0)
                        {
                            dataKeyProperty.stringValue = "";
                        }
                    }
                    else if (newDisplayIndex > 0 && newDisplayIndex <= availableKeys.Length)
                    {
                        // User selected an existing key
                        dataKeyProperty.stringValue = availableKeys[newDisplayIndex - 1];
                    }
                }
                
                // If "New Key..." is selected (displayIndex == 0), show text field
                if (newDisplayIndex == 0)
                {
                    EditorGUI.indentLevel++;
                    dataKeyProperty.stringValue = EditorGUILayout.DelayedTextField("New Key Name", dataKeyProperty.stringValue);
                    EditorGUI.indentLevel--;
                }
            }
            else
            {
                EditorGUILayout.PropertyField(dataKeyProperty, Styles.ResultKey);
                EditorGUILayout.HelpBox("No TimelineDataManager found in scene.", MessageType.Warning);
                return;
            }
            
            // If an existing key is selected or a new key name is entered, show type info
            if (!string.IsNullOrEmpty(dataKeyProperty.stringValue))
            {
                var keyValue = GetDataManagerValue(dataManager, dataKeyProperty.stringValue);
                
                if (keyValue != null)
                {
                    var valueInfo = GetValueTypeInfo(keyValue);
                    EditorGUILayout.LabelField($"Existing Type: {valueInfo.typeName}", EditorStyles.miniLabel);
                    
                    // If list or array, show index configuration
                    if (valueInfo.isList)
                    {
                        EditorGUILayout.LabelField($"List Size: {valueInfo.listSize}", EditorStyles.miniLabel);
                        
                        // Index data source type selection
                        indexTypeProperty.intValue = EditorGUILayout.Popup("Result Index Source", indexTypeProperty.intValue, Styles.IndexSourceTypeNames);
                        
                        EditorGUI.indentLevel++;
                        if (indexTypeProperty.intValue == 0) // Numeric
                        {
                            indexNumericValueProperty.intValue = EditorGUILayout.IntField("Index Value", indexNumericValueProperty.intValue);
                            
                            // Allow index to be equal to list size (for appending)
                            if (indexNumericValueProperty.intValue < 0)
                                indexNumericValueProperty.intValue = 0;
                            
                            if (indexNumericValueProperty.intValue < valueInfo.listSize)
                            {
                                EditorGUILayout.LabelField($"Will modify element at index [{indexNumericValueProperty.intValue}]", EditorStyles.miniLabel);
                            }
                            else if (indexNumericValueProperty.intValue == valueInfo.listSize)
                            {
                                EditorGUILayout.LabelField("Will append new element to list", EditorStyles.miniLabel);
                            }
                            else
                            {
                                EditorGUILayout.LabelField("Index out of range (will be clamped)", EditorStyles.miniLabel);
                            }
                        }
                        else // DataManager
                        {
                            DrawSimpleDataKeyDropdown(indexDataKeyProperty);
                            
                            if (!string.IsNullOrEmpty(indexDataKeyProperty.stringValue))
                            {
                                var indexFromData = GetDataManagerValue(dataManager, indexDataKeyProperty.stringValue);
                                if (indexFromData != null && indexFromData is int actualIndex)
                                {
                                    EditorGUILayout.LabelField($"Index from DataManager: {actualIndex}", EditorStyles.miniLabel);
                                    
                                    if (actualIndex < valueInfo.listSize)
                                    {
                                        EditorGUILayout.LabelField($"Will modify element at index [{actualIndex}]", EditorStyles.miniLabel);
                                    }
                                    else if (actualIndex == valueInfo.listSize)
                                    {
                                        EditorGUILayout.LabelField("Will append new element to list", EditorStyles.miniLabel);
                                    }
                                }
                            }
                        }
                        EditorGUI.indentLevel--;
                    }
                    else
                    {
                        EditorGUILayout.LabelField("Will overwrite existing value", EditorStyles.miniLabel);
                        // Not a list, reset index properties
                        indexTypeProperty.intValue = 0;
                        indexNumericValueProperty.intValue = 0;
                        indexDataKeyProperty.stringValue = "";
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("New key - will create as float value", EditorStyles.miniLabel);
                    // Reset index properties for new keys
                    indexTypeProperty.intValue = 0;
                    indexNumericValueProperty.intValue = 0;
                    indexDataKeyProperty.stringValue = "";
                }
            }
        }

        /// <summary>
        /// 绘制 Protobuf 字段下拉选择框（支持多层嵌套导航）
        /// </summary>
        private static void DrawProtobufFieldDropdown(SerializedProperty fieldProperty, SerializedProperty indexTypeProperty,
                                                     SerializedProperty indexNumericValueProperty, SerializedProperty indexDataKeyProperty)
        {
            if (fieldProperty == null)
            {
                EditorGUILayout.HelpBox("Protobuf field property not found", MessageType.Error);
                return;
            }

            var dataManager = Object.FindObjectOfType<TimelineDataManager>();
            
            if (dataManager == null || dataManager.genericFishDeadSync == null)
            {
                EditorGUILayout.HelpBox("No TimelineDataManager or GenericFishDeadSync found in scene.", MessageType.Warning);
                EditorGUILayout.PropertyField(fieldProperty, new GUIContent("Protobuf Field"));
                return;
            }

            // Parse current field path (e.g., "DrawWheelInfo.WheelId")
            var fieldPath = fieldProperty.stringValue;
            var pathParts = string.IsNullOrEmpty(fieldPath) ? new string[0] : fieldPath.Split('.');
            
            // Navigate through nested fields
            object currentObject = dataManager.genericFishDeadSync;
            System.Type currentType = currentObject.GetType();
            bool foundListField = false;
            
            for (int depth = 0; depth < 10 && currentObject != null; depth++) // Max depth of 10 to prevent infinite loops
            {
                // Get available fields at current level
                var availableFields = GetProtobufFieldsAtLevel(currentType, depth == 0);
                
                if (availableFields.Length == 0)
                    break;
                
                // Current selection at this depth
                string currentSelection = depth < pathParts.Length ? pathParts[depth] : "";
                int currentIndex = System.Array.IndexOf(availableFields, currentSelection);
                
                // If current selection is not found, use first item but don't force currentIndex to 0
                // This allows us to detect when user actually changes selection
                int displayIndex = currentIndex >= 0 ? currentIndex : 0;
                
                EditorGUILayout.BeginHorizontal();
                
                string label = depth == 0 ? "Protobuf Field" : $"Nested Field";
                var newIndex = EditorGUILayout.Popup(label, displayIndex, availableFields);
                
                if (depth == 0 && GUILayout.Button("↻", GUILayout.Width(25)))
                {
                    EditorUtility.SetDirty(dataManager);
                }
                
                EditorGUILayout.EndHorizontal();
                
                // Handle selection change - rebuild path
                string selectedField = availableFields[newIndex];
                
                // Rebuild path up to current depth
                var newPathParts = new string[depth + 1];
                for (int i = 0; i < depth; i++)
                {
                    newPathParts[i] = pathParts[i];
                }
                newPathParts[depth] = selectedField;
                
                // Only update path if it actually changed (user made a selection change)
                string newPath = string.Join(".", newPathParts);
                bool pathChanged = false;
                
                // Check if this depth's selection actually changed
                bool selectionChanged = (depth >= pathParts.Length) || (pathParts[depth] != selectedField);
                
                // Only update if selection changed, or if we need to extend/truncate the path
                if (selectionChanged)
                {
                    pathChanged = fieldProperty.stringValue != newPath;
                    
                    if (pathChanged)
                    {
                        fieldProperty.stringValue = newPath;
                        
                        // Only update pathParts when we actually changed the path
                        // This prevents breaking nested field navigation
                        pathParts = newPathParts;
                    }
                }
                
                // Get selected field info
                var property = currentType.GetProperty(selectedField);
                if (property == null)
                    break;
                
                var value = property.GetValue(currentObject);
                var valueType = property.PropertyType;
                
                // Check if it's a numeric type (leaf node)
                if (IsNumericType(valueType))
                {
                    EditorGUILayout.LabelField($"Type: {valueType.Name}", EditorStyles.miniLabel);
                    if (value != null)
                    {
                        EditorGUILayout.LabelField($"Value: {value}", EditorStyles.miniLabel);
                    }
                    break; // End of path
                }
                // Check if it's a list
                else if (IsProtobufList(valueType))
                {
                    foundListField = true;
                    
                    // Get generic type of the list
                    var genericArgs = valueType.GetGenericArguments();
                    if (genericArgs.Length > 0)
                    {
                        var elementType = genericArgs[0];
                        
                        // Show list info
                        var fieldInfo = GetProtobufFieldInfo(currentObject, selectedField);
                        EditorGUILayout.LabelField($"Type: RepeatedField<{elementType.Name}>", EditorStyles.miniLabel);
                        EditorGUILayout.LabelField($"List Size: {fieldInfo.listSize}", EditorStyles.miniLabel);
                        
                        // If element is a message (nested object), allow further navigation
                        if (IsProtobufMessage(elementType))
                        {
                            // Reset index only when path changed (field selection changed)
                            if (pathChanged)
                            {
                                indexTypeProperty.intValue = 0;
                                indexNumericValueProperty.intValue = 0;
                                indexDataKeyProperty.stringValue = "";
                            }
                            
                            // For nested objects in list, show index dropdown (0, 1, 2...)
                            if (fieldInfo.listSize > 0)
                            {
                                var indexOptions = new string[fieldInfo.listSize];
                                for (int i = 0; i < fieldInfo.listSize; i++)
                                {
                                    indexOptions[i] = i.ToString();
                                }
                                
                                // Force index type to Numeric for Protobuf lists
                                indexTypeProperty.intValue = 0;
                                
                                int selectedIndex = indexNumericValueProperty.intValue;
                                
                                if (selectedIndex < 0 || selectedIndex >= fieldInfo.listSize)
                                    selectedIndex = 0;
                                
                                selectedIndex = EditorGUILayout.Popup("List Index", selectedIndex, indexOptions);
                                
                                indexNumericValueProperty.intValue = selectedIndex;
                                
                                // Get the element at the index to continue navigation
                                var listValue = property.GetValue(currentObject);
                                var indexer = valueType.GetProperty("Item");
                                if (indexer != null && selectedIndex >= 0 && selectedIndex < fieldInfo.listSize)
                                {
                                    var elementValue = indexer.GetValue(listValue, new object[] { selectedIndex });
                                    if (elementValue != null)
                                    {
                                        currentObject = elementValue;
                                        currentType = elementType;
                                        
                                        // Now show nested field selection for the list element
                                        // We need to check if there's a saved nested field in the path
                                        var nestedFields = GetProtobufFieldsAtLevel(elementType, false);
                                        if (nestedFields.Length > 0)
                                        {
                                            // Check if we have a nested field selection in the path
                                            string nestedSelection = depth + 1 < pathParts.Length ? pathParts[depth + 1] : "";
                                            int nestedIndex = System.Array.IndexOf(nestedFields, nestedSelection);
                                            int nestedDisplayIndex = nestedIndex >= 0 ? nestedIndex : 0;
                                            
                                            var newNestedIndex = EditorGUILayout.Popup("Nested Field", nestedDisplayIndex, nestedFields);
                                            
                                            if (newNestedIndex >= 0 && newNestedIndex < nestedFields.Length)
                                            {
                                                string selectedNestedField = nestedFields[newNestedIndex];
                                                
                                                // Update path to include nested field
                                                string fullPath = newPath + "." + selectedNestedField;
                                                if (fieldProperty.stringValue != fullPath)
                                                {
                                                    fieldProperty.stringValue = fullPath;
                                                }
                                                
                                                // Show the nested field info
                                                var nestedProp = elementType.GetProperty(selectedNestedField);
                                                if (nestedProp != null)
                                                {
                                                    var nestedValue = nestedProp.GetValue(elementValue);
                                                    var nestedType = nestedProp.PropertyType;
                                                    EditorGUILayout.LabelField($"Type: {nestedType.Name}", EditorStyles.miniLabel);
                                                    if (nestedValue != null)
                                                    {
                                                        EditorGUILayout.LabelField($"Value: {nestedValue}", EditorStyles.miniLabel);
                                                    }
                                                }
                                            }
                                        }
                                        
                                        break; // Don't continue the loop, we've handled nested field selection here
                                    }
                                }
                            }
                            else
                            {
                                EditorGUILayout.HelpBox("List is empty", MessageType.Info);
                            }
                            break;
                        }
                        else
                        {
                            // 元素为基础类型 - 显示简单的索引下拉框（带值预览）
                            if (pathChanged)
                            {
                                indexTypeProperty.intValue = 0;
                                indexNumericValueProperty.intValue = 0;
                                indexDataKeyProperty.stringValue = "";
                            }
                            
                            // 显示列表元素值的下拉框
                            if (fieldInfo.listSize > 0)
                            {
                                var indexOptions = new string[fieldInfo.listSize];
                                for (int i = 0; i < fieldInfo.listSize; i++)
                                {
                                    indexOptions[i] = $"{i} ({fieldInfo.listValues[i]})";
                                }
                                
                                // Force index type to Numeric for Protobuf lists
                                indexTypeProperty.intValue = 0;
                                
                                int selectedIndex = indexNumericValueProperty.intValue;
                                
                                if (selectedIndex < 0 || selectedIndex >= fieldInfo.listSize)
                                    selectedIndex = 0;
                                
                                selectedIndex = EditorGUILayout.Popup("List Index", selectedIndex, indexOptions);
                                indexNumericValueProperty.intValue = selectedIndex;
                                
                                EditorGUILayout.LabelField($"Selected Value: {fieldInfo.listValues[selectedIndex]}", EditorStyles.miniLabel);
                            }
                            else
                            {
                                EditorGUILayout.HelpBox("List is empty", MessageType.Info);
                            }
                            break; // End of path (list of primitives is terminal)
                        }
                    }
                    break;
                }
                // It's a nested object - continue to next level
                else if (value != null && IsProtobufMessage(valueType))
                {
                    EditorGUILayout.LabelField($"Type: {valueType.Name} (nested object)", EditorStyles.miniLabel);
                    currentObject = value;
                    currentType = valueType;
                    // Continue to next depth
                }
                else
                {
                    EditorGUILayout.LabelField($"Type: {valueType.Name} (unsupported)", EditorStyles.miniLabel);
                    break;
                }
            }
            
            // Reset index properties if no list field found
            if (!foundListField)
            {
                indexTypeProperty.intValue = 0;
                indexNumericValueProperty.intValue = 0;
                indexDataKeyProperty.stringValue = "";
            }
        }

        /// <summary>
        /// 获取指定层级的 Protobuf 字段列表
        /// </summary>
        private static string[] GetProtobufFieldsAtLevel(System.Type type, bool isRootLevel)
        {
            if (type == null)
                return new string[0];

            var fields = new System.Collections.Generic.List<string>();
            var properties = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            foreach (var prop in properties)
            {
                // Filter out internal Protobuf properties
                if (prop.Name == "Descriptor" || prop.Name == "Parser")
                    continue;
                    
                var propType = prop.PropertyType;
                
                // Include numeric types, lists, and nested messages
                if (IsNumericType(propType) || 
                    IsProtobufList(propType) || 
                    IsProtobufMessage(propType))
                {
                    fields.Add(prop.Name);
                }
            }

            fields.Sort();
            return fields.ToArray();
        }

        /// <summary>
        /// 判断类型是否为数值类型
        /// </summary>
        private static bool IsNumericType(System.Type type)
        {
            return type == typeof(uint) || type == typeof(int) || type == typeof(float) || 
                   type == typeof(double) || type == typeof(long) || type == typeof(ulong) ||
                   type == typeof(short) || type == typeof(ushort) || type == typeof(byte) ||
                   type == typeof(sbyte);
        }

        /// <summary>
        /// 判断类型是否为 Protobuf 的 RepeatedField（列表类型）
        /// </summary>
        private static bool IsProtobufList(System.Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition().Name.Contains("RepeatedField");
        }

        /// <summary>
        /// 判断类型是否为 Protobuf 消息类型（嵌套对象）
        /// </summary>
        private static bool IsProtobufMessage(System.Type type)
        {
            // Check if it implements IMessage interface (Protobuf messages)
            return type.GetInterfaces().Any(i => 
                i.IsGenericType && i.GetGenericTypeDefinition().Name.Contains("IMessage"));
        }

        /// <summary>
        /// Protobuf 字段信息结构体
        /// </summary>
        private struct ProtobufFieldInfo
        {
            public bool exists;        // 字段是否存在
            public string typeName;    // 类型名称
            public bool isList;        // 是否为列表
            public int listSize;       // 列表大小
            public object value;       // 字段值（非列表时）
            public object[] listValues; // 列表值（列表时）
        }

        /// <summary>
        /// 获取 Protobuf 字段的详细信息
        /// </summary>
        private static ProtobufFieldInfo GetProtobufFieldInfo(object protobufObject, string fieldName)
        {
            var info = new ProtobufFieldInfo();
            
            if (protobufObject == null || string.IsNullOrEmpty(fieldName))
                return info;

            try
            {
                var type = protobufObject.GetType();
                var property = type.GetProperty(fieldName);
                
                if (property != null)
                {
                    info.exists = true;
                    var value = property.GetValue(protobufObject);
                    
                    if (value != null)
                    {
                        var valueType = value.GetType();
                        info.typeName = valueType.Name;
                        
                        // 检查是否为 RepeatedField 列表类型
                        if (valueType.IsGenericType && valueType.GetGenericTypeDefinition().Name.Contains("RepeatedField"))
                        {
                            info.isList = true;
                            var countProp = valueType.GetProperty("Count");
                            if (countProp != null)
                            {
                                info.listSize = (int)countProp.GetValue(value);
                                
                                // 获取列表值（最多10个）
                                var listValues = new System.Collections.Generic.List<object>();
                                var indexer = valueType.GetProperty("Item");
                                if (indexer != null)
                                {
                                    for (int i = 0; i < info.listSize && i < 10; i++)
                                    {
                                        listValues.Add(indexer.GetValue(value, new object[] { i }));
                                    }
                                }
                                info.listValues = listValues.ToArray();
                            }
                        }
                        else
                        {
                            info.isList = false;
                            info.value = value;
                        }
                    }
                }
            }
            catch (System.Exception)
            {
            }

            return info;
        }
    }
}
