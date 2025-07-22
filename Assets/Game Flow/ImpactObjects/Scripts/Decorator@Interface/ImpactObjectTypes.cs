using System;
using Game_Flow.ImpactObjects.Scripts.Types;
using Game_Flow.ImpactObjects.Scripts.UnityMonoSOScripts;

namespace Game_Flow.ImpactObjects.Scripts.Decorator_Interface
{
    public enum ImpactObjectTypes
    {
        OneBlockGrid = 5,
        TwoBlockHorizontalGrid = 6,
        TwoBlockVerticalGrid = 7,
        ThreeBlockHorizontalGrid = 8,
        ThreeBlockVerticalGrid = 9,
        FourBlocksSquareGrid = 10,
        MovingShader = 11,
        LShapeImpactObject = 12,
    }
    public static class ImpactObjectFactory
    {
        public static IImpactObject CreateImpactObject(ImpactObjectTypes type, IImpactObject inner, MonoImpactObject mono,ImpactObjectStats stats, Grid grid)
        {
            return type switch
            {
                ImpactObjectTypes.OneBlockGrid => new OneBlockGridImpactObject(inner,mono,stats,grid),
                ImpactObjectTypes.TwoBlockHorizontalGrid => new TwoBlockHorizontalGridImpactObject(inner,mono,stats,grid),
                ImpactObjectTypes.TwoBlockVerticalGrid => new TwoBlockVerticalGridImpactObject(inner,mono,stats,grid),
                ImpactObjectTypes.ThreeBlockHorizontalGrid => new ThreeBlockHorizontalGridImpactObject(inner,mono,stats,grid),
                ImpactObjectTypes.ThreeBlockVerticalGrid => new ThreeBlockVerticalGridImpactObject(inner,mono,stats,grid),
                ImpactObjectTypes.FourBlocksSquareGrid => new FourBlocksSquareGridImpactObject(inner,mono,stats,grid),
                ImpactObjectTypes.MovingShader => new MovingShaderImpactObject(inner,mono,stats,grid),
                ImpactObjectTypes.LShapeImpactObject => new LShapeImpactObject(inner,mono,stats,grid),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
}