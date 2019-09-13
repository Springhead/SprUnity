using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VGent{
    public abstract class MentalExistence : MonoBehaviour {
        public abstract Type GetAttribute<Type>() where Type : MentalAttribute;
    }
}
