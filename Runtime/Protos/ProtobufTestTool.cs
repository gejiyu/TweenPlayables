using System;
using System.IO;
using UnityEngine;
using Google.Protobuf;

/// <summary>
/// Protobuf 序列化与反序列化测试工具
/// </summary>
public class ProtobufTestTool : MonoBehaviour
{
    [Header("测试设置")]
    [Tooltip("是否在启动时自动测试")]
    public bool autoTestOnStart = true;

    [Tooltip("保存文件的路径")]
    public string saveFileName = "fishDead_test.bin";

    void Start()
    {
        if (autoTestOnStart)
        {
            TestWrite();
        }
    }

    /// <summary>
    /// 测试写入（序列化）
    /// </summary>
    [ContextMenu("测试写入")]
    public void TestWrite()
    {
        Debug.Log("=== 开始测试写入（序列化）===\n");

        try
        {
            // 1. 创建测试数据
            var fishDead = CreateTestData();
            Debug.Log("✅ 创建测试数据成功");
            PrintFishDeadSync("原始数据", fishDead);

            // 2. 写入二进制文件
            WriteToFile(fishDead);

            // 3. 写入 Base64 文件
            WriteToBase64File(fishDead);

            Debug.Log("\n🎉 写入测试完成！");
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ 写入测试失败: {e.Message}\n{e.StackTrace}");
        }
    }

    /// <summary>
    /// 测试读取（反序列化）
    /// </summary>
    [ContextMenu("测试读取")]
    public void TestRead()
    {
        Debug.Log("=== 开始测试读取（反序列化）===\n");

        try
        {
            // 1. 从二进制文件读取
            ReadFromFile();

            // 2. 从 Base64 文件读取
            ReadFromBase64File();

            Debug.Log("\n🎉 读取测试完成！");
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ 读取测试失败: {e.Message}\n{e.StackTrace}");
        }
    }

    /// <summary>
    /// 完整测试（写入+读取+验证）
    /// </summary>
    [ContextMenu("完整测试")]
    public void TestComplete()
    {
        Debug.Log("=== 开始完整测试 ===\n");

        try
        {
            // 1. 创建测试数据
            var fishDead = CreateTestData();
            Debug.Log("✅ 创建测试数据成功");
            PrintFishDeadSync("原始数据", fishDead);

            // 2. 测试字节数组序列化
            TestByteArraySerialization(fishDead);

            // 3. 测试文件序列化
            TestFileSerialization(fishDead);

            // 4. 测试 Base64 序列化
            TestBase64Serialization(fishDead);

            Debug.Log("\n🎉 所有测试通过！");
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ 测试失败: {e.Message}\n{e.StackTrace}");
        }
    }

    /// <summary>
    /// 创建测试数据
    /// </summary>
    GenericFishDeadSync CreateTestData()
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

        // 添加道具奖励
        fishDead.Items.Add(new ItemInfo
        {
            ExcelId = 3001,
            Count = 1,
            Param1 = 0
        });
        fishDead.Items.Add(new ItemInfo
        {
            ExcelId = 3002,
            Count = 5,
            Param1 = 100
        });

        // 添加抽奖转盘数据
        fishDead.DrawWheelInfo = new SkinShenDrawWheel
        {
            DrawIndex = 3,
            BaseScore = 1000
        };
        fishDead.DrawWheelInfo.RateList.Add(2);
        fishDead.DrawWheelInfo.RateList.Add(5);
        fishDead.DrawWheelInfo.RateList.Add(10);
        fishDead.DrawWheelInfo.RateList.Add(20);
        fishDead.DrawWheelInfo.RateList.Add(50);

        // 添加技能得分详情
        var skillInfo = new FishCommonAwardInfo
        {
            TEventType = 1,
            MulType = 2
        };
        skillInfo.Mul.Add(2);
        skillInfo.Mul.Add(3);
        skillInfo.Mul.Add(5);
        fishDead.SkillComInfos.Add(skillInfo);

        // 添加额外参数
        fishDead.ExtraParams1.Add(100);
        fishDead.ExtraParams1.Add(200);
        fishDead.ExtraParams1.Add(300);
        
        fishDead.ExtraParams2.Add(1);
        fishDead.ExtraParams2.Add(2);
        fishDead.ExtraParams2.Add(3);

        return fishDead;
    }

    /// <summary>
    /// 写入二进制文件
    /// </summary>
    void WriteToFile(GenericFishDeadSync fishDead)
    {
        Debug.Log("\n--- 写入二进制文件 ---");

        string filePath = Path.Combine(Application.persistentDataPath, saveFileName);

        // 写入文件
        using (var fileStream = File.Create(filePath))
        {
            fishDead.WriteTo(fileStream);
        }
        Debug.Log($"✅ 保存到文件: {filePath}");
        Debug.Log($"文件大小: {new FileInfo(filePath).Length} bytes");
    }

    /// <summary>
    /// 从二进制文件读取
    /// </summary>
    void ReadFromFile()
    {
        Debug.Log("\n--- 从二进制文件读取 ---");

        string filePath = Path.Combine(Application.persistentDataPath, saveFileName);

        if (!File.Exists(filePath))
        {
            Debug.LogError($"❌ 文件不存在: {filePath}");
            return;
        }

        // 从文件读取
        GenericFishDeadSync decoded;
        using (var fileStream = File.OpenRead(filePath))
        {
            decoded = GenericFishDeadSync.Parser.ParseFrom(fileStream);
        }
        Debug.Log("✅ 从文件读取成功");
        PrintFishDeadSync("从文件读取", decoded);
    }

    /// <summary>
    /// 写入 Base64 文件
    /// </summary>
    void WriteToBase64File(GenericFishDeadSync fishDead)
    {
        Debug.Log("\n--- 写入 Base64 文件 ---");

        // 序列化为 Base64
        byte[] bytes = fishDead.ToByteArray();
        string base64 = Convert.ToBase64String(bytes);

        // 保存 Base64 到文件
        string base64FilePath = Path.Combine(Application.persistentDataPath, "fishDead_base64.txt");
        File.WriteAllText(base64FilePath, base64);
        Debug.Log($"✅ Base64 保存到文件: {base64FilePath}");
        Debug.Log($"Base64 长度: {base64.Length}");
        Debug.Log($"Base64 (前100字符): {base64.Substring(0, Math.Min(100, base64.Length))}...");
    }

    /// <summary>
    /// 从 Base64 文件读取
    /// </summary>
    void ReadFromBase64File()
    {
        Debug.Log("\n--- 从 Base64 文件读取 ---");

        string base64FilePath = Path.Combine(Application.persistentDataPath, "fishDead_base64.txt");

        if (!File.Exists(base64FilePath))
        {
            Debug.LogError($"❌ 文件不存在: {base64FilePath}");
            return;
        }

        // 从 Base64 文件读取
        string base64FromFile = File.ReadAllText(base64FilePath);
        byte[] decodedBytes = Convert.FromBase64String(base64FromFile);
        var decoded = GenericFishDeadSync.Parser.ParseFrom(decodedBytes);
        Debug.Log("✅ 从 Base64 文件反序列化成功");
        PrintFishDeadSync("从 Base64 读取", decoded);
    }

    /// <summary>
    /// 测试字节数组序列化
    /// </summary>
    void TestByteArraySerialization(GenericFishDeadSync original)
    {
        Debug.Log("\n--- 测试字节数组序列化 ---");

        // 序列化
        byte[] bytes = original.ToByteArray();
        Debug.Log($"序列化后大小: {bytes.Length} bytes");
        Debug.Log($"前20字节: {BitConverter.ToString(bytes, 0, Math.Min(20, bytes.Length))}");

        // 反序列化
        var decoded = GenericFishDeadSync.Parser.ParseFrom(bytes);
        Debug.Log("✅ 反序列化成功");

        // 验证
        bool isEqual = original.Equals(decoded);
        Debug.Log($"数据一致性: {(isEqual ? "✅ 通过" : "❌ 失败")}");

        if (!isEqual)
        {
            Debug.LogWarning("数据不一致！");
            PrintFishDeadSync("反序列化后", decoded);
        }
    }

    /// <summary>
    /// 测试文件序列化
    /// </summary>
    void TestFileSerialization(GenericFishDeadSync original)
    {
        Debug.Log("\n--- 测试文件序列化 ---");

        string filePath = Path.Combine(Application.persistentDataPath, saveFileName);

        // 写入文件
        using (var fileStream = File.Create(filePath))
        {
            original.WriteTo(fileStream);
        }
        Debug.Log($"✅ 保存到文件: {filePath}");
        Debug.Log($"文件大小: {new FileInfo(filePath).Length} bytes");

        // 从文件读取
        GenericFishDeadSync decoded;
        using (var fileStream = File.OpenRead(filePath))
        {
            decoded = GenericFishDeadSync.Parser.ParseFrom(fileStream);
        }
        Debug.Log("✅ 从文件读取成功");
        PrintFishDeadSync("从文件读取", decoded);

        // 验证
        bool isEqual = original.Equals(decoded);
        Debug.Log($"数据一致性: {(isEqual ? "✅ 通过" : "❌ 失败")}");
    }

    /// <summary>
    /// 测试 Base64 序列化
    /// </summary>
    void TestBase64Serialization(GenericFishDeadSync original)
    {
        Debug.Log("\n--- 测试 Base64 序列化 ---");

        // 序列化为 Base64
        byte[] bytes = original.ToByteArray();
        string base64 = Convert.ToBase64String(bytes);
        Debug.Log($"Base64 长度: {base64.Length}");
        Debug.Log($"Base64 (前100字符): {base64.Substring(0, Math.Min(100, base64.Length))}...");

        // 保存 Base64 到文件
        string base64FilePath = Path.Combine(Application.persistentDataPath, "fishDead_base64.txt");
        File.WriteAllText(base64FilePath, base64);
        Debug.Log($"✅ Base64 保存到文件: {base64FilePath}");

        // 从 Base64 文件读取
        string base64FromFile = File.ReadAllText(base64FilePath);
        byte[] decodedBytes = Convert.FromBase64String(base64FromFile);
        var decoded = GenericFishDeadSync.Parser.ParseFrom(decodedBytes);
        Debug.Log("✅ 从 Base64 文件反序列化成功");
        PrintFishDeadSync("从 Base64 读取", decoded);

        // 验证
        bool isEqual = original.Equals(decoded);
        Debug.Log($"数据一致性: {(isEqual ? "✅ 通过" : "❌ 失败")}");
    }

    /// <summary>
    /// 测试 JSON 序列化
    /// </summary>
    void TestJsonSerialization(GenericFishDeadSync original)
    {
        Debug.Log("\n--- 测试 JSON 序列化 ---");

        // 序列化为 JSON
        string json = JsonFormatter.Default.Format(original);
        Debug.Log($"JSON 长度: {json.Length}");
        Debug.Log($"JSON 内容:\n{json}");

        // 从 JSON 反序列化
        var decoded = JsonParser.Default.Parse<GenericFishDeadSync>(json);
        Debug.Log("✅ 从 JSON 反序列化成功");

        // 验证
        bool isEqual = original.Equals(decoded);
        Debug.Log($"数据一致性: {(isEqual ? "✅ 通过" : "❌ 失败")}");
    }

    /// <summary>
    /// 打印 GenericFishDeadSync 详细信息
    /// </summary>
    void PrintFishDeadSync(string title, GenericFishDeadSync data)
    {
        Debug.Log($"\n【{title}】");
        Debug.Log($"玩家ID: {data.Uid}");
        Debug.Log($"鱼ID: {data.FishId}, 配置ID: {data.FishExcelId}");
        Debug.Log($"炮倍: {data.KillBatteryMul}");
        Debug.Log($"当前金币: {data.CurCoin}, 当前钻石: {data.CurDiamond}");
        Debug.Log($"实际奖励金币: {data.AwardCoin}");
        Debug.Log($"展示奖励金币: {data.AwardCoinForShow}");
        Debug.Log($"奖励钻石: {data.AwardDiamond}");
        Debug.Log($"道具数量: {data.Items.Count}");
        foreach (var item in data.Items)
        {
            Debug.Log($"  - 道具 {item.ExcelId}: 数量={item.Count}, 参数={item.Param1}");
        }
        Debug.Log($"总倍率: {data.TotalRate}");
        Debug.Log($"通用数据: {data.GeneralFishData}");
        if (data.DrawWheelInfo != null)
        {
            Debug.Log($"转盘索引: {data.DrawWheelInfo.DrawIndex}, 基础分数: {data.DrawWheelInfo.BaseScore}");
            Debug.Log($"倍率列表: [{string.Join(", ", data.DrawWheelInfo.RateList)}]");
        }
        Debug.Log($"技能信息数量: {data.SkillComInfos.Count}");
        Debug.Log($"额外参数1: [{string.Join(", ", data.ExtraParams1)}]");
        Debug.Log($"额外参数2: [{string.Join(", ", data.ExtraParams2)}]");
    }

    /// <summary>
    /// 清理测试文件
    /// </summary>
    [ContextMenu("清理测试文件")]
    public void CleanupTestFiles()
    {
        string filePath = Path.Combine(Application.persistentDataPath, saveFileName);
        string base64FilePath = Path.Combine(Application.persistentDataPath, "fishDead_base64.txt");
        
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log($"✅ 已删除测试文件: {filePath}");
        }
        
        if (File.Exists(base64FilePath))
        {
            File.Delete(base64FilePath);
            Debug.Log($"✅ 已删除 Base64 文件: {base64FilePath}");
        }
        
        if (!File.Exists(filePath) && !File.Exists(base64FilePath))
        {
            Debug.Log("没有找到测试文件");
        }
    }

    /// <summary>
    /// 获取并打印所有字段信息
    /// </summary>
    [ContextMenu("列出所有字段")]
    public void ListAllFields()
    {
        var fishDead = CreateTestData();
        var descriptor = GenericFishDeadSync.Descriptor;
        
        Debug.Log("=== GenericFishDeadSync 所有字段信息 ===\n");
        
        foreach (var field in descriptor.Fields.InDeclarationOrder())
        {
            PrintField(field, fishDead, 0);
        }
    }

    /// <summary>
    /// 打印字段信息（支持递归）
    /// </summary>
    void PrintField(Google.Protobuf.Reflection.FieldDescriptor field, Google.Protobuf.IMessage message, int indent)
    {
        string indentStr = new string(' ', indent * 2);
        var fieldValue = field.Accessor.GetValue(message);
        
        // 基本信息
        string fieldInfo = $"{indentStr}[{field.FieldNumber}] {field.Name} ({field.FieldType})";
        
        if (fieldValue == null)
        {
            Debug.Log($"{fieldInfo}: null");
            return;
        }

        // 根据字段类型处理
        switch (field.FieldType)
        {
            case Google.Protobuf.Reflection.FieldType.Message:
                if (field.IsRepeated)
                {
                    // 重复的消息字段（列表）
                    var list = fieldValue as System.Collections.IEnumerable;
                    int count = 0;
                    foreach (var item in list) count++;
                    
                    Debug.Log($"{fieldInfo}: [{count} items]");
                    
                    if (count > 0)
                    {
                        int index = 0;
                        foreach (var item in list)
                        {
                            Debug.Log($"{indentStr}  [{index}]:");
                            var itemMessage = item as Google.Protobuf.IMessage;
                            if (itemMessage != null)
                            {
                                foreach (var subField in itemMessage.Descriptor.Fields.InDeclarationOrder())
                                {
                                    PrintField(subField, itemMessage, indent + 2);
                                }
                            }
                            index++;
                        }
                    }
                }
                else
                {
                    // 单个消息字段
                    Debug.Log($"{fieldInfo}:");
                    var subMessage = fieldValue as Google.Protobuf.IMessage;
                    if (subMessage != null)
                    {
                        foreach (var subField in subMessage.Descriptor.Fields.InDeclarationOrder())
                        {
                            PrintField(subField, subMessage, indent + 1);
                        }
                    }
                }
                break;
                
            default:
                // 基本类型或重复的基本类型
                if (field.IsRepeated)
                {
                    var list = fieldValue as System.Collections.IEnumerable;
                    var values = new System.Collections.Generic.List<string>();
                    foreach (var item in list)
                    {
                        values.Add(item.ToString());
                    }
                    Debug.Log($"{fieldInfo}: [{string.Join(", ", values)}]");
                }
                else
                {
                    Debug.Log($"{fieldInfo}: {fieldValue}");
                }
                break;
        }
    }
}
