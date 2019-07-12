using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// verifica si el personaje esta chocando contra el suelo, para detectar el tipo de colision
public class CheckGround : MonoBehaviour {

	private VitaController vita;
	private Rigidbody2D rb2d;

	// Use this for initialization
	void Start () {
		vita = GetComponentInParent<VitaController> ();
		rb2d = GetComponentInParent<Rigidbody2D> ();

	}
	void OnCollisionEnter2D(Collision2D col){ // entrar en plataforma movil
		if (col.gameObject.tag == "TagPlataforma") {
			vita.transform.parent = col.transform; // el personaje se convierte en hijo de la plataforma sobre la que colisiona para tomar su posicion 
			vita.grounded = true;
			rb2d.velocity = new Vector3 (0f, 0f, 0f); // la vlocidad es cero (para que se quede quieto mientras baja la plataforma)

		}
	}
	void OnCollisionStay2D(Collision2D col){
		if (col.gameObject.tag == "TagSuelo") {
			vita.grounded = true;
		}
		if (col.gameObject.tag == "TagPlataforma") {
			vita.transform.parent = col.transform; // el personaje se convierte en hijo de la plataforma sobre la que colisiona para tomar su posicion 
			vita.grounded = true;
		}

	}

	void OnCollisionExit2D(Collision2D col){ 
		if(col.gameObject.tag == "TagSuelo"){
			vita.grounded = false;
		}
		if(col.gameObject.tag == "TagPlataforma"){
			vita.transform.parent = null;
			vita.grounded = false;
		}

	}
}