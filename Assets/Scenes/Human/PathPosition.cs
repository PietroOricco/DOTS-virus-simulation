using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
[InternalBufferCapacity(20)]
public struct PathPosition : IBufferElementData {

    public int2 position;

}
