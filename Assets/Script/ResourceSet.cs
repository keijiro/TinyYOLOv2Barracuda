using UnityEngine;
using Unity.Barracuda;

namespace TinyYoloV2
{
    [CreateAssetMenu(fileName = "TinyYOLOv2",
                     menuName = "ScriptableObjects/TinyYOLOv2 Resource Set")]
    public sealed class ResourceSet : ScriptableObject
    {
        public NNModel model;
        public ComputeShader preprocess;
        public ComputeShader postprocess1;
        public ComputeShader postprocess2;
    }
}
