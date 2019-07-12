using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

[CreateNodeMenu("Value/Float")]
public class FloatNode : Node {
    [Output] public float output;
    public float value;

	// Use this for initialization
	protected override void Init() {
		base.Init();
		
	}

	// Return the correct value of an output port when requested
	public override object GetValue(NodePort port) {
        if(port.fieldName == "output") {
            return value;
        } else { return 0f; }
	}
}