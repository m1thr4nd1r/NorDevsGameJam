using System;
using UnityEngine;

namespace UnityStandardAssets._2D
{
    public class PlatformerCharacter2D : MonoBehaviour
    {
        [SerializeField] private float m_MaxSpeed = 10f;                    // The fastest the player can travel in the x axis.
        [SerializeField] private float m_JumpForce = 400f;                  // Amount of force added when the player jumps.
        [Range(0, 1)] [SerializeField] private float m_CrouchSpeed = .36f;  // Amount of maxSpeed applied to crouching movement. 1 = 100%
        [SerializeField] private bool m_AirControl = false;                 // Whether or not a player can steer while jumping;
        [SerializeField] private LayerMask m_WhatIsGround;                  // A mask determining what is ground to the character

        private Transform m_GroundCheck;    // A position marking where to check if the player is grounded.
		const float k_GroundedRadius = .35f; // Radius of the overlap circle to determine if grounded
        private bool m_Grounded = false;            // Whether or not the player is grounded.
        private Transform m_CeilingCheck;   // A position marking where to check for ceilings
        float k_CeilingRadius = .01f; // Radius of the overlap circle to determine if the player can stand up
        private Animator m_Anim;            // Reference to the player's animator component.
        private Rigidbody2D m_Rigidbody2D;
        private bool m_FacingRight = true;  // For determining which way the player is currently facing.
		private AudioSource steps;
		public GameObject menu;
		private bool alive;
		private float timeStart;

		private void Awake()
        {
            // Setting up references.
            m_GroundCheck = transform.Find("GroundCheck");
            m_CeilingCheck = transform.Find("CeilingCheck");
            m_Anim = GetComponent<Animator>();
            m_Rigidbody2D = GetComponent<Rigidbody2D>();
			steps = GetComponents<AudioSource>()[1];
			alive = true;
        }

		void Start()
		{
			timeStart = Time.time;
		}

        void Update()
        {
			if (Input.GetKeyDown(KeyCode.R))
				die();
        }

        private void FixedUpdate()
        {
			if (!alive)
				return;

			m_Grounded = false;

			// The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
			// This can be done using layers instead but Sample Assets will not overwrite your project settings.
			Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
			for (int i = 0; i < colliders.Length; i++)
			{
				print(colliders[i].name);
				if (colliders[i].gameObject.name.Equals("Win"))
				{
					AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("Personagem principal Falling"), transform.position);
					win();
				}
				if (colliders[i].gameObject != gameObject)
				{
					m_Grounded = true;
					break;
				}
			}

			m_Anim.SetBool("grounded", m_Grounded);
			//print(m_Grounded);
			// Set the vertical animation
			//m_Anim.SetFloat("vSpeed", m_Rigidbody2D.velocity.y);
		}

		void win()
		{
			alive = false;
			m_Anim.Stop();
			GetComponent<AudioSource>().Stop();
			Camera.main.GetComponent<LevelGen>().StopCamera();

			//Time.timeScale = 0;
			if (menu != null)
			{
				menu.GetComponent<Tempo>().setTime(timeStart);
				menu.SetActive(true);
			}
		}
		
		public void Move(float move, bool crouch, bool jump)
        {
			if (!alive)
				return;

            // If crouching, check to see if the character can stand up
            /*
            if (!crouch && m_Anim.GetBool("Crouch"))
            {
                // If the character has a ceiling preventing them from standing up, keep them crouching
                if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround))
                {
                    crouch = true;
                }
            } */

            // Set whether or not the character is crouching in the animator
            //m_Anim.SetBool("Crouch", crouch);

            //only control the player if grounded or airControl is turned on
            if (m_Grounded || m_AirControl)
            {
                // Reduce the speed if crouching by the crouchSpeed multiplier
                move = (crouch ? move*m_CrouchSpeed : move);

                // The Speed animator parameter is set to the absolute value of the horizontal input.
                //m_Anim.SetFloat("Speed", Mathf.Abs(move));
                m_Anim.SetBool("movement", Mathf.Abs(move) > 0);
				if (Mathf.Abs(move) > 0 && !steps.isPlaying && m_Grounded)
					steps.Play();
				else if (!m_Grounded || move == 0)
					steps.Stop();
                
                // Move the character
                m_Rigidbody2D.velocity = new Vector2(move*m_MaxSpeed, m_Rigidbody2D.velocity.y);

                // If the input is moving the player right and the player is facing left...
                if (move > 0 && !m_FacingRight)
                {
                    // ... flip the player.
                    Flip();
                }
                    // Otherwise if the input is moving the player left and the player is facing right...
                else if (move < 0 && m_FacingRight)
                {
                    // ... flip the player.
                    Flip();
                }
            }

            // If the player should jump...
            if (m_Grounded && jump && m_Anim.GetBool("grounded"))
            {
                // Add a vertical force to the player.
                //m_Grounded = false;
                m_Anim.SetBool("grounded", m_Grounded);
				m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
                AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("Personagem principal Jump"), transform.position);
            }
        }   

        private void Flip()
        {
			if (!alive)
				return;
            // Switch the way the player is labelled as facing.
            m_FacingRight = !m_FacingRight;

            // Multiply the player's x local scale by -1.
            Vector3 theScale = transform.localScale;
            theScale.x *= -1;
            transform.localScale = theScale;
        }

        void die()
        {
			Time.timeScale = 1;
			UnityEngine.SceneManagement.SceneManager.LoadScene(0);
		}

		void OnCollisionEnter2D(Collision2D c)
        {
			if (!alive)
				return;

			if (c.collider.name.Contains("Wall"))
			{
				AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("Mr T Death 01"), transform.position);
				LevelGen t = Camera.main.gameObject.GetComponent<LevelGen>();
				//t.StopCamera();
				Invoke("win", 0.5f);
				alive = false;
			}
			else if (c.collider.name.Contains("Sand"))
			{
				AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("Mr T Death 01"), transform.position);
				//LevelGen t = Camera.main.gameObject.GetComponent<LevelGen>();
				//t.StopCamera();

				Invoke("win", 0);
				alive = false;
			}
		}
    }
}
