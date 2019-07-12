using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VitaController : MonoBehaviour {

	public float speed;
	public float maxSpeed;
	public bool grounded;
	public float jumpPower;
	public bool dobleSalto; // se activa para permitir el doble salto al personaje
	public bool hurt = false;
	public bool bolaFuego; // se activa para hablitar al personaje que dispare
	public bool disparoFuego; //se activa al disparar
	public int resistencia;
	public GameObject fuegoDerecha;
	public GameObject fuegoIzquierda;
	public float fuegoRate = 0.5f;
	Vector3 fuegoPos;
	float nextFuego = 0.0f;
	public int resistenciaInicial;
	public int huevos;
	public int puntos;

	public GameObject sonidoSalto;
	public GameObject sonidoGolpe;
	public GameObject sonidoHuevo;
	public GameObject sonidoHuevoVida;
	public GameObject sonidoHurt;
	public GameObject sonidoFuego;



	private Rigidbody2D rb2d;
	private Animator anim;
	private bool jump; // dos variables de jump porque fixedUpdate puede que no la detecte
	private int cont = 0; // contador para el doble salto
	private bool movement = true;
	private bool izq; // si el personaje mira hacia la izquierda
	private bool der; // si el personaje mira hacia la derecha
	public bool invisible = true;
	private bool reinicioEnProceso;

	void Start () {
		FindObjectOfType<Musica> ().LevelMusic ();
		sonidoSalto.gameObject.GetComponent<AudioSource> ().playOnAwake = true;
		sonidoGolpe.gameObject.GetComponent<AudioSource> ().playOnAwake = true;
		sonidoHuevo.gameObject.GetComponent<AudioSource> ().playOnAwake = true;
		sonidoHuevoVida.gameObject.GetComponent<AudioSource> ().playOnAwake = true;
		sonidoHurt.gameObject.GetComponent<AudioSource> ().playOnAwake = true;
		sonidoFuego.gameObject.GetComponent<AudioSource> ().playOnAwake = true;
		rb2d = GetComponent<Rigidbody2D> ();
		anim = GetComponent<Animator> ();
		der = true;
		resistencia = resistenciaInicial; // guarda la resistencia del jugador (que depende del dinosaurio) para restablecerla correctamente al perder una vida
		AudioScript.Instance.gameObject.GetComponent<AudioSource> ().Stop (); // IMPORTANTE: DESCOMENTAR ESTA LINEA ANTES DE EXPORTAR EL JUEGO, ASÍ EL AUDIO FUNCIONA CORRECTAMENTE.
		huevos = 0;
		puntos = 0;

		if (PlayerSelect.vitaSelected) {
			if (gameObject.name == "doux") {
				gameObject.SetActive (false);

			}
			if (gameObject.name == "mort") {
				gameObject.SetActive (false);

			}

		}
		if (PlayerSelect.douxSelected) {
			if (gameObject.name == "vita") {
				gameObject.SetActive (false);
			}
			if (gameObject.name == "mort") {
				gameObject.SetActive (false);

			}

		}
		if (PlayerSelect.mortSelected) {
			if (gameObject.name == "vita") {
				gameObject.SetActive (false);
			}
			if (gameObject.name == "doux") {
				gameObject.SetActive (false);

			}

		}

	}
	

	void Update () {
		anim.SetFloat ("speed", Mathf.Abs(rb2d.velocity.x)); //establece la velocidad en el animador
		anim.SetBool ("grounded", grounded);
		anim.SetBool ("hurt", hurt);
		if (gameObject.name == "mort") {
			anim.SetBool ("fuego", disparoFuego);
		}



		if (dobleSalto) {
			if (Input.GetKeyDown (KeyCode.UpArrow) && cont < 1) {
				Instantiate (sonidoSalto);
				jump = true;

				cont++;
			}
			if (grounded) {
				cont = 0;
			}
		}else if (Input.GetKeyDown (KeyCode.UpArrow) && grounded ) {
			Instantiate (sonidoSalto);
			jump = true;
		}
			
	}
	// REEMPLAZA A UPDATE, FUNCIONA MEJOR CON FISICAS, EVITA PROBLEMAS DE FRAMERATE
	void FixedUpdate(){ 

		Vector3 fixedVelocity = rb2d.velocity;
		fixedVelocity.x *= 0.75f; // disminuye la velocidad progresivamente para no deslizarse (debido al meterial deslizante) 

		if (grounded) {
			rb2d.velocity = fixedVelocity; // asigna las disminuciones de velocidad al personaje
		}

		float h = Input.GetAxis("Horizontal");
		if (!movement) {
			h = 0;
		}
		rb2d.AddForce(Vector2.right * speed * h); // vector de desplazamiento horizontal

		float limitedSpeed = Mathf.Clamp (rb2d.velocity.x, -maxSpeed, maxSpeed);
		rb2d.velocity = new Vector2 (limitedSpeed, rb2d.velocity.y);

		if (h > 0.1f) { //si vamos hacia la derecha
			transform.localScale = new Vector3(1f, 1f, 1f);
			der = true;
			izq = false;
		}
		if (h < -0.1f) { // si vamos hacia la izquierda (cambia scale a -1 para que el personaje mire hacia la izquieda)
			transform.localScale = new Vector3(-1f, 1f, 1f);
			izq = true;
			der = false;
		}

		if(jump){
			rb2d.velocity = new Vector2(rb2d.velocity.x, 0); // para que no se sumen los impulsos de los saltos (al saltar en la plataforma con effector)
			rb2d.AddForce (Vector2.up * jumpPower, ForceMode2D.Impulse); //fuerza hacia arriba, siendo un impulso
			jump = false; // para que no salte en cada frame
		}
		if (bolaFuego) { // si el personaje tiene habilitada la opcion de disparar fuego

			if (Input.GetButtonDown ("Fire1") && Time.time > nextFuego) {

				nextFuego = Time.time + fuegoRate;
				disparoFuego = true; // para activar la animacion de disparo
				fuego ();

			}
		}
	}


	/// <summary>
	/// FINALIZACION DE UPDATE... 
	/// </summary>

	void OnBecameInvisible(){ // reinicia la posicion cuando el personaje sale de la escena / util para puntos de control
		if (invisible) {
			Invoke("Reiniciar",2f); //GENERA PROBLEMA (a veces) AL MORIR Y REINICIAR LA CAMARA, DETECTA QUE SE VUELVE INVISIBLE
		
		}
		invisible = true;
	}
	public void Reiniciar(){
		gameObject.tag = "Player";
		transform.Find ("Ground Collider").gameObject.layer = 8;
		gameObject.layer = 8;
		gameObject.transform.Rotate (0, 0, 90.0f);

		reinicioEnProceso = false;
		movement = true; // al morir, a veces, se desactiva el movimiento, por eso vuelve a activarse
		resistencia = resistenciaInicial;

		FindObjectOfType<Musica> ().LevelMusic ();	

		transform.position = new Vector3(-6,0,0); // Vector3 = vector de 3 dimensiones
		//invisible = true; // se activa la posibilidad de usar OnBecameInvisible
		
	
	
	}

	public void EnemyJump(){ // salto al caer sobre el enemigo
		rb2d.velocity = new Vector2(rb2d.velocity.x, 0); // para que no se sumen los impulsos de los saltos (al saltar en la plataforma con effector)
		rb2d.AddForce (Vector2.up * (jumpPower/1.6f), ForceMode2D.Impulse); //fuerza hacia arriba, siendo un impulso
		Instantiate(sonidoGolpe);
		jump = false; // para que no salte en cada frame
	}

	public void EnemyKnockBack(float enemyPosX){ // salto al ser dañado por el enemigo
		jump = true;

		float side = Mathf.Sign (enemyPosX - transform.position.x); // devuelve el signo (1 o -1) o 0
		rb2d.AddForce(Vector2.left * side * (jumpPower/1.25f), ForceMode2D.Impulse); //hace el salto al ser dañado
		movement = false;
		hurt = true;
		Invoke ("EnableMovement", 0.75f);
	}

	void EnableMovement(){
		movement = true;
		hurt = false;
	}
	public void DisableMovement(){
		movement = false;
		gameObject.transform.Rotate (0, 0, -90.0f);

		rb2d.velocity = new Vector2 (0, rb2d.velocity.y);
		transform.Find ("Ground Collider").gameObject.layer = 9;

		gameObject.layer = 9;
		gameObject.tag = "Untagged";
	}
	public void ReducirResistencia(){
		resistencia -=1;
		Instantiate (sonidoHurt);
		if (resistencia < 1) {
			Invoke ("DisableMovement", 0.80f);


			invisible = false; // antes de reiniciar es necesario hacer esto para que no se activa OnBecomeInvisible
			if (!reinicioEnProceso) {
				Invoke ("Reiniciar", 3f);
			}
			reinicioEnProceso = true;
			//resistencia = resistenciaInicial; // (se movio en Reiniciar) REEMPLAZAR DESPUES POR LA DISMINUCION DE VIDAS
		}

	}

	void fuego(){
		Instantiate (sonidoFuego);
		fuegoPos = transform.position;
		if(der){
			fuegoPos += new Vector3 (0.7f, 0.6f, 1f);
			Instantiate (fuegoDerecha, fuegoPos, Quaternion.identity);
		}else{
			fuegoPos += new Vector3 (-0.9f, 0.925f, 1f);
			Instantiate (fuegoIzquierda, fuegoPos, Quaternion.Euler(new Vector3(0,0,180)));

		}
		Invoke ("detenerAnimacionDisparo", 0.2f); 
	}

	void detenerAnimacionDisparo(){

		disparoFuego = false; // para quitar la animacion de dispara y pasar a la sgte transicion

	}

	public void desactivarInvisible(){
		invisible = false;
	}

	void OnTriggerEnter2D(Collider2D col){
		if (col.gameObject.tag == "TagHuevo") {
			Instantiate (sonidoHuevo);
			huevos++;
			puntos += 100;
			Destroy(col.gameObject);


		}


		if (col.gameObject.tag == "TagHuevoVida") {
			Instantiate (sonidoHuevoVida);
			if (resistencia < resistenciaInicial) {

				resistencia++;

			}
			puntos += 200;

			Destroy (col.gameObject);
		}
	
	}

	void OnTriggerExit2D(Collider2D col){

		if (col.gameObject.tag == "TagMusicaBatalla") {
			FindObjectOfType<Musica> ().BattleMusic ();
		}
	}

}
