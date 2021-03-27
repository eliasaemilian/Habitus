using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkaroundInstantiateCmptShader : MonoBehaviour
{
    public static ComputeShader InstantiateComputeShader( ComputeShader computeShader)
    {
        return Instantiate( computeShader );
    }
}
