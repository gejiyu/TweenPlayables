#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;

/// <summary>
/// Protobuf 消息查看器窗口
/// </summary>
public class ProtobufInspectorWindow : OdinEditorWindow
{
    [MenuItem("Tools/Protobuf Inspector")]
    private static void OpenWindow()
    {
        GetWindow<ProtobufInspectorWindow>().Show();
    }

    private GenericFishDeadSync currentMessage;

    private Vector2 scrollPos;

    protected override void OnImGUI()
    {
        base.OnImGUI();
        DrawMessageTree();
        EditorGUILayout.Space(10);
        DrawButtons();
    }

    /// <summary>
    /// 绘制按钮
    /// </summary>
    void DrawButtons()
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        
        GUI.backgroundColor = new Color(1f, 0.8f, 0.4f);
        if (GUILayout.Button("创建测试数据", GUILayout.Height(30), GUILayout.Width(150)))
        {
            CreateTestData();
        }
        GUI.backgroundColor = Color.white;
        
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    public void CreateTestData()
    {
        currentMessage = CreateSampleData();
        scrollPos = Vector2.zero; // 重置滚动位置到顶部
        EditorUtility.DisplayDialog("成功", "测试数据创建成功！", "确定");
    }

    /// <summary>
    /// 设置要显示的消息
    /// </summary>
    public void SetMessage(GenericFishDeadSync message)
    {
        currentMessage = message;
        scrollPos = Vector2.zero;
        Repaint();
    }

    /// <summary>
    /// 绘制消息树
    /// </summary>
    void DrawMessageTree()
    {
        if (currentMessage == null)
        {
            return;
        }

        SirenixEditorGUI.Title("消息内容", "", TextAlignment.Left, true);
        EditorGUILayout.Space(5);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        
        var descriptor = GenericFishDeadSync.Descriptor;
        foreach (var field in descriptor.Fields.InDeclarationOrder())
        {
            DrawField(field, currentMessage, 0);
        }

        EditorGUILayout.EndScrollView();
    }

    /// <summary>
    /// 绘制字段（递归）
    /// </summary>
    void DrawField(FieldDescriptor field, IMessage message, int indent)
    {
        var fieldValue = field.Accessor.GetValue(message);

        // 跳过默认值
        if (IsDefaultValue(field, fieldValue))
        {
            return;
        }

        GUILayout.BeginHorizontal();
        GUILayout.Space(indent * 20);

        switch (field.FieldType)
        {
            case FieldType.Message:
                if (field.IsRepeated)
                {
                    // 重复的消息字段（列表）
                    var list = fieldValue as System.Collections.IEnumerable;
                    int count = 0;
                    foreach (var item in list) count++;

                    // 绘制带背景的字段名
                    var labelStyle = new GUIStyle(EditorStyles.boldLabel);
                    labelStyle.normal.background = MakeTex(2, 2, new Color(0.3f, 0.5f, 0.7f, 0.3f));
                    labelStyle.padding = new RectOffset(5, 5, 2, 2);
                    
                    // 获取列表元素的类型名
                    string typeName = "Message";
                    if (count > 0)
                    {
                        foreach (var item in list)
                        {
                            var itemMsg = item as IMessage;
                            if (itemMsg != null)
                            {
                                typeName = itemMsg.Descriptor.Name;
                            }
                            break;
                        }
                    }
                    
                    EditorGUILayout.LabelField($"[{field.FieldNumber}] {field.Name}", labelStyle, GUILayout.Width(250));
                    
                    // 类型名带背景
                    var typeStyle = new GUIStyle(EditorStyles.miniLabel);
                    typeStyle.normal.background = MakeTex(2, 2, new Color(0.7f, 0.5f, 0.3f, 0.3f));
                    typeStyle.padding = new RectOffset(4, 4, 2, 2);
                    EditorGUILayout.LabelField($"<{typeName}[]>", typeStyle, GUILayout.Width(150));
                    
                    // 数据类型
                    var dataTypeStyle = new GUIStyle(EditorStyles.miniLabel);
                    dataTypeStyle.normal.background = MakeTex(2, 2, new Color(0.5f, 0.5f, 0.8f, 0.25f));
                    dataTypeStyle.padding = new RectOffset(4, 4, 2, 2);
                    EditorGUILayout.LabelField("List", dataTypeStyle, GUILayout.Width(80));
                    
                    EditorGUILayout.LabelField($"{count} items", EditorStyles.miniLabel);
                    GUILayout.EndHorizontal();

                    if (count > 0)
                    {
                        int index = 0;
                        foreach (var item in list)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Space((indent + 1) * 20);
                            EditorGUILayout.LabelField($"[{index}]", EditorStyles.boldLabel, GUILayout.Width(50));
                            GUILayout.EndHorizontal();

                            var itemMessage = item as IMessage;
                            if (itemMessage != null)
                            {
                                foreach (var subField in itemMessage.Descriptor.Fields.InDeclarationOrder())
                                {
                                    DrawField(subField, itemMessage, indent + 2);
                                }
                            }
                            index++;
                        }
                    }
                }
                else
                {
                    // 单个消息字段
                    var labelStyle = new GUIStyle(EditorStyles.boldLabel);
                    labelStyle.normal.background = MakeTex(2, 2, new Color(0.3f, 0.5f, 0.7f, 0.3f));
                    labelStyle.padding = new RectOffset(5, 5, 2, 2);
                    EditorGUILayout.LabelField($"[{field.FieldNumber}] {field.Name}", labelStyle, GUILayout.Width(250));
                    
                    var subMessage = fieldValue as IMessage;
                    string typeName = subMessage != null ? subMessage.Descriptor.Name : "Message";
                    
                    // 类型名带背景
                    var typeStyle = new GUIStyle(EditorStyles.miniLabel);
                    typeStyle.normal.background = MakeTex(2, 2, new Color(0.7f, 0.5f, 0.3f, 0.3f));
                    typeStyle.padding = new RectOffset(4, 4, 2, 2);
                    EditorGUILayout.LabelField($"<{typeName}>", typeStyle, GUILayout.Width(150));
                    
                    // 数据类型
                    var dataTypeStyle = new GUIStyle(EditorStyles.miniLabel);
                    dataTypeStyle.normal.background = MakeTex(2, 2, new Color(0.5f, 0.5f, 0.8f, 0.25f));
                    dataTypeStyle.padding = new RectOffset(4, 4, 2, 2);
                    EditorGUILayout.LabelField("Message", dataTypeStyle, GUILayout.Width(80));
                    
                    GUILayout.EndHorizontal();

                    if (subMessage != null)
                    {
                        foreach (var subField in subMessage.Descriptor.Fields.InDeclarationOrder())
                        {
                            DrawField(subField, subMessage, indent + 1);
                        }
                    }
                }
                break;

            default:
                // 基本类型
                if (field.IsRepeated)
                {
                    var list = fieldValue as System.Collections.IEnumerable;
                    var values = new List<string>();
                    foreach (var item in list)
                    {
                        values.Add(item.ToString());
                    }
                    string valueStr = $"[{string.Join(", ", values)}]";
                    
                    var labelStyle = new GUIStyle(EditorStyles.label);
                    labelStyle.normal.background = MakeTex(2, 2, new Color(0.4f, 0.7f, 0.4f, 0.3f));
                    labelStyle.padding = new RectOffset(5, 5, 2, 2);
                    EditorGUILayout.LabelField($"[{field.FieldNumber}] {field.Name}", labelStyle, GUILayout.Width(250));
                    
                    // 数据类型
                    var dataTypeStyle = new GUIStyle(EditorStyles.miniLabel);
                    dataTypeStyle.normal.background = MakeTex(2, 2, new Color(0.5f, 0.5f, 0.8f, 0.25f));
                    dataTypeStyle.padding = new RectOffset(4, 4, 2, 2);
                    string fieldTypeName = GetFieldTypeName(field.FieldType);
                    EditorGUILayout.LabelField($"{fieldTypeName}[]", dataTypeStyle, GUILayout.Width(80));
                    
                    // 数值带背景
                    var valueStyle = new GUIStyle(EditorStyles.wordWrappedLabel);
                    valueStyle.normal.background = MakeTex(2, 2, new Color(0.9f, 0.7f, 0.4f, 0.25f));
                    valueStyle.padding = new RectOffset(4, 4, 2, 2);
                    EditorGUILayout.LabelField(valueStr, valueStyle);
                    GUILayout.EndHorizontal();
                }
                else
                {
                    var labelStyle = new GUIStyle(EditorStyles.label);
                    labelStyle.normal.background = MakeTex(2, 2, new Color(0.4f, 0.7f, 0.4f, 0.3f));
                    labelStyle.padding = new RectOffset(5, 5, 2, 2);
                    EditorGUILayout.LabelField($"[{field.FieldNumber}] {field.Name}", labelStyle, GUILayout.Width(250));
                    
                    // 数据类型
                    var dataTypeStyle = new GUIStyle(EditorStyles.miniLabel);
                    dataTypeStyle.normal.background = MakeTex(2, 2, new Color(0.5f, 0.5f, 0.8f, 0.25f));
                    dataTypeStyle.padding = new RectOffset(4, 4, 2, 2);
                    string fieldTypeName = GetFieldTypeName(field.FieldType);
                    EditorGUILayout.LabelField(fieldTypeName, dataTypeStyle, GUILayout.Width(80));
                    
                    // 数值带背景
                    var valueStyle = new GUIStyle(EditorStyles.boldLabel);
                    valueStyle.normal.background = MakeTex(2, 2, new Color(0.9f, 0.7f, 0.4f, 0.25f));
                    valueStyle.padding = new RectOffset(4, 4, 2, 2);
                    EditorGUILayout.LabelField(FormatValueSimple(fieldValue), valueStyle);
                    GUILayout.EndHorizontal();
                }
                break;
        }
    }

    /// <summary>
    /// 创建纯色纹理
    /// </summary>
    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; ++i)
        {
            pix[i] = col;
        }
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    /// <summary>
    /// 格式化值显示（简化版）
    /// </summary>
    string FormatValueSimple(object value)
    {
        if (value == null) return "null";
        return value.ToString();
    }

    /// <summary>
    /// 获取字段类型名称
    /// </summary>
    string GetFieldTypeName(FieldType fieldType)
    {
        switch (fieldType)
        {
            case FieldType.Double: return "double";
            case FieldType.Float: return "float";
            case FieldType.Int64: return "int64";
            case FieldType.UInt64: return "uint64";
            case FieldType.Int32: return "int32";
            case FieldType.Fixed64: return "fixed64";
            case FieldType.Fixed32: return "fixed32";
            case FieldType.Bool: return "bool";
            case FieldType.String: return "string";
            case FieldType.Bytes: return "bytes";
            case FieldType.UInt32: return "uint32";
            case FieldType.SFixed32: return "sfixed32";
            case FieldType.SFixed64: return "sfixed64";
            case FieldType.SInt32: return "sint32";
            case FieldType.SInt64: return "sint64";
            case FieldType.Enum: return "enum";
            case FieldType.Message: return "message";
            default: return fieldType.ToString();
        }
    }

    /// <summary>
    /// 判断是否为默认值
    /// </summary>
    bool IsDefaultValue(FieldDescriptor field, object value)
    {
        if (value == null) return true;

        if (field.IsRepeated)
        {
            var list = value as System.Collections.IEnumerable;
            int count = 0;
            foreach (var item in list) count++;
            return count == 0;
        }

        switch (field.FieldType)
        {
            case FieldType.String:
                return string.IsNullOrEmpty(value as string);
            case FieldType.Bool:
                return !(bool)value;
            case FieldType.UInt32:
                return (uint)value == 0;
            case FieldType.UInt64:
                return (ulong)value == 0;
            case FieldType.Int32:
                return (int)value == 0;
            case FieldType.Int64:
                return (long)value == 0;
            case FieldType.Float:
                return Math.Abs((float)value) < 0.0001f;
            case FieldType.Double:
                return Math.Abs((double)value) < 0.0001;
            case FieldType.Message:
                return value == null;
            default:
                return false;
        }
    }

    /// <summary>
    /// 创建示例数据
    /// </summary>
    GenericFishDeadSync CreateSampleData()
    {
        var fishDead = new GenericFishDeadSync
        {
            Uid = 10001,
            FishId = 12345,
            FishExcelId = 200001,
            KillBatteryMul = 10,
            CurCoin = 1000000,
            CurDiamond = 500,
            AwardCoin = 5000,
            AwardCoinForShow = 6000,
            AwardDiamond = 10,
            BUseArms = false,
            ArmsAddScore = 0,
            GeneralFishData = "{\"wingId200004\":2,\"wingId200005\":100}",
            TotalRate = 50
        };

        fishDead.Items.Add(new ItemInfo { ExcelId = 3001, Count = 1, Param1 = 0 });
        fishDead.Items.Add(new ItemInfo { ExcelId = 3002, Count = 5, Param1 = 100 });

        fishDead.DrawWheelInfo = new SkinShenDrawWheel { DrawIndex = 3, BaseScore = 1000 };
        fishDead.DrawWheelInfo.RateList.Add(2);
        fishDead.DrawWheelInfo.RateList.Add(5);
        fishDead.DrawWheelInfo.RateList.Add(10);
        fishDead.DrawWheelInfo.RateList.Add(20);
        fishDead.DrawWheelInfo.RateList.Add(50);

        var skillInfo = new FishCommonAwardInfo { TEventType = 1, MulType = 2 };
        skillInfo.Mul.Add(2);
        skillInfo.Mul.Add(3);
        skillInfo.Mul.Add(5);
        fishDead.SkillComInfos.Add(skillInfo);

        fishDead.ExtraParams1.Add(100);
        fishDead.ExtraParams1.Add(200);
        fishDead.ExtraParams1.Add(300);

        return fishDead;
    }
}
#endif
