using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SprUnity {
    public abstract class MentalExistance : MonoBehaviour {
        public abstract Type GetAttribute<Type>() where Type : MentalAttribute;
    }
}
