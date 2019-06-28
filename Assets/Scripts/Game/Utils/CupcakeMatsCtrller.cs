using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UncleBear
{
    public class CupcakeMatsCtrller : MonoBehaviour
    {

        public Material[] matCups;
        public Material[] matCreams;
        public Material[] matIngreds;

        public Material RandomCupMat()
        {
            //在外部用mat,已经是new
            return matCups[Random.Range(0, matCups.Length)];
            //return GameObject.Instantiate(matCups[Random.Range(0, matCups.Length)]) as Material;
        }
        public Material RandomCreamMat()
        {
            return matCreams[Random.Range(0, matCreams.Length)];
            //return GameObject.Instantiate() as Material;
        }
    }
}
