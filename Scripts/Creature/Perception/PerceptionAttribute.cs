using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PerceptionAttribute : ISerializationCallbackReceiver {
    public string name = "";

    public enum Type {
        Float,
        Bool,
        Vector3,
        None
    };
    public Type type = Type.None;

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

    public PerceptionAttributeValue value = null;

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

    public virtual PerceptionAttribute Clone() {
        var clone = new PerceptionAttribute();
        clone.name = name;
        clone.type = type;
        clone.value = value.Clone();
        return clone;
    }

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

    public string _serialized = "";

    public virtual void OnBeforeSerialize() {
        _serialized = value.Serialize();
    }
    public virtual void OnAfterDeserialize() {
        if (type == Type.Float) {
            value = new PAFloat();
        } else if (type == Type.Bool) {
            value = new PABool();
        } else if (type == Type.Vector3) {
            value = new PAVector3();
        }

        if (value != null) {
            value.Deserialize(_serialized);
        }
    }
}

// ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

[System.Serializable]
public abstract class PerceptionAttributeValue {
    public abstract PerceptionAttributeValue Clone();
    public abstract string Serialize();
    public abstract void Deserialize(string serialized);
}

[System.Serializable]
public class PAFloat : PerceptionAttributeValue {
    public float value = 0.0f;

    public override PerceptionAttributeValue Clone() {
        var newObj = new PAFloat();
        newObj.value = this.value;
        return newObj;
    }

    public override string Serialize() {
        return value.ToString();
    }

    public override void Deserialize(string serialized) {
        float.TryParse(serialized, out value);
    }
}


[System.Serializable]
public class PABool : PerceptionAttributeValue {
    public bool value = false;

    public override PerceptionAttributeValue Clone() {
        var newObj = new PABool();
        newObj.value = this.value;
        return newObj;
    }

    public override string Serialize() {
        return value.ToString();
    }

    public override void Deserialize(string serialized) {
        bool.TryParse(serialized, out value);
    }
}

[System.Serializable]
public class PAVector3 : PerceptionAttributeValue {
    public Vector3 value = new Vector3();

    public override PerceptionAttributeValue Clone() {
        var newObj = new PAVector3();
        newObj.value = this.value;
        return newObj;
    }

    public override string Serialize() {
        return( value.x.ToString() + " " + value.y.ToString() + value.z.ToString() );
    }

    public override void Deserialize(string serialized) {
        var splitted = serialized.Split(new char[]{' '});
        value.x = 0; float.TryParse(splitted[0], out value.x);
        value.y = 0; float.TryParse(splitted[0], out value.y);
        value.z = 0; float.TryParse(splitted[0], out value.z);
    }
}

