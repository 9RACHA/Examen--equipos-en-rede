using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode.Components;

public class NetworkTransformC : NetworkTransform
{
   protected override bool OnIsServerAuthoritative() { //Sobreescribe el NetworkTransform y autoriza que el cliente mande
        return false;
    }
}

