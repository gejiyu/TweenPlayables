using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace TweenPlayables
{
    /// <summary>
    /// Data source types for calculator operands
    /// </summary>
    [Serializable]
    public enum CalculatorDataSource
    {
        Numeric = 0,         // Direct numeric value
        DataManager = 1,     // TimelineDataManager key
        Protobuf = 2         // Protobuf field from GenericFishDeadSync
    }

    /// <summary>
    /// Calculation operators
    /// </summary>
    [Serializable]
    public enum CalculationOperator
    {
        Add = 0,        // +
        Subtract = 1,   // -
        Multiply = 2,   // *
        Divide = 3,     // /
        Modulo = 4,     // %
        Power = 5,      // ^
        Min = 6,        // min
        Max = 7         // max
    }

    [Serializable]
    public struct CalculatorNode
    {
        public bool isActive;
        
        // 左操作数数据源
        public CalculatorDataSource leftDataType;
        public float leftNumericValue;
        public string leftDataKey;
        public string leftProtobufField;  // Protobuf字段名
        
        // 左操作数列表索引数据源
        public CalculatorDataSource leftIndexType;
        public int leftIndexNumericValue;
        public string leftIndexDataKey;
        
        // 左操作数元素字段（当 leftProtobufField 指向 List 且元素是 Protobuf Message 时使用）
        public string leftElementField;
        
        // 左操作数元素索引数据源（当 leftElementField 也指向 List 时使用）
        public CalculatorDataSource leftElementIndexType;
        public int leftElementIndexNumericValue;
        public string leftElementIndexDataKey;
        
        // 运算符
        public CalculationOperator calculationOperator;
        
        // 右操作数数据源
        public CalculatorDataSource rightDataType;
        public float rightNumericValue;
        public string rightDataKey;
        public string rightProtobufField;  // Protobuf字段名
        
        // 右操作数列表索引数据源
        public CalculatorDataSource rightIndexType;
        public int rightIndexNumericValue;
        public string rightIndexDataKey;
        
        // 右操作数元素字段（当 rightProtobufField 指向 List 且元素是 Protobuf Message 时使用）
        public string rightElementField;
        
        // 右操作数元素索引数据源（当 rightElementField 也指向 List 时使用）
        public CalculatorDataSource rightElementIndexType;
        public int rightElementIndexNumericValue;
        public string rightElementIndexDataKey;
        
        // 结果存储配置
        public string resultDataKey;
        public CalculatorDataSource resultIndexType;
        public int resultIndexNumericValue;
        public string resultIndexDataKey;
    }

    [Serializable]
    public sealed class TweenCalculatorClip : PlayableAsset, ITimelineClipAsset
    {
        public CalculatorNode calculatorNode = new CalculatorNode 
        { 
            isActive = true,
            leftDataType = CalculatorDataSource.Numeric,
            rightDataType = CalculatorDataSource.Numeric,
            calculationOperator = CalculationOperator.Add
        };

        public ClipCaps clipCaps => ClipCaps.None;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<TweenCalculatorBehaviour>.Create(graph);
            var behaviour = playable.GetBehaviour();

            // Pass calculatorNode data to behaviour
            behaviour.SetCalculatorNode(calculatorNode);
            
            return playable;
        }
    }
}