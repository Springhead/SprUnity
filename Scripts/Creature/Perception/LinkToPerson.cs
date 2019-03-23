using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkToPerson : MonoBehaviour {

	public Person person = null;

    // ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

    void Start () {
	}
	
	void Update () {
	}

    void OnDestroy() {
        if (person != null) {
            Destroy(person.gameObject);
        }
    }
}
