using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PerceptionValue {
    string name = "";

    public static Type Create<Type>() where Type : PerceptionValue {
        if (typeof(Type) == typeof(PerceptionValueFloat)) {
            return new PerceptionValueFloat() as Type;
        }
        if (typeof(Type) == typeof(PerceptionValueBool)) {
            return new PerceptionValueBool() as Type;
        }
        if (typeof(Type) == typeof(PerceptionValueVector3)) {
            return new PerceptionValueVector3() as Type;
        }
        return null;
    }
}

public class PerceptionValueFloat : PerceptionValue {
    float value = 0.0f;
}

public class PerceptionValueBool : PerceptionValue {
    bool value = false;
}

public class PerceptionValueVector3 : PerceptionValue {
    Vector3 value = new Vector3();
}

// ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

public class PerceptionObject : MonoBehaviour {

    public Dictionary<string, PerceptionValue> values = new Dictionary<string, PerceptionValue>();

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

    private void Start() {
        PerceptionScene.Add(this);
    }

    private void FixedUpdate() {
    }

    private void OnDrawGizmos() {
    }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

    // Get PerceptionValue by Name (CREATE NEW if not exists)
    public Type Get<Type>(string valueName) where Type : PerceptionValue {
        if (!values.ContainsKey(valueName)) {
            values[valueName] = PerceptionValue.Create<Type>();
        }
        return values[valueName] as Type;
    }

    // Get PerceptionValue by Name (RETURN NULL if not exists)
    public Type Check<Type>(string valueName) where Type : PerceptionValue {
        if (values.ContainsKey(valueName)) {
            return values[valueName] as Type;
        } else {
            return null;
        }
    }

}
