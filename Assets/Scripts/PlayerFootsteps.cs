using UnityEngine;

public class PlayerFootsteps : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip[] footstepSounds;

    [Header("Einstellungen")]
    public float stepInterval = 0.8f;      // Zeit zwischen Schritten (bei Laufen)
    public float sprintStepInterval = 0.25f;
		public float jumpHeight = .5f;
		public float sprintSpeed = 6f;

    private CharacterController controller;
    private float stepTimer = 0f;
    private int currentStepIndex = 0;

    void Start()
    {
        // Beim FPS-PlayerCapsule sitzt der CharacterController auf demselben Objekt.
        // Beim XR Origin (XR Rig) sitzt er auf dem Root-Objekt, waehrend dieses Script
        // z.B. an der Main Camera haengt (fuer kopfnahes Audio) - daher zusaetzlich
        // in Eltern/Kindern suchen.
        controller = GetComponent<CharacterController>();
        if (controller == null)
            controller = GetComponentInParent<CharacterController>();
        if (controller == null)
            controller = GetComponentInChildren<CharacterController>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        // Falls AudioSource fehlt, eine erstellen
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
            audioSource.volume = 0.8f;
        }
    }

    void Update()
    {
        if (controller == null) return;

				Vector3 horizontalVelocity = new Vector3(controller.velocity.x, 0f, controller.velocity.z);
    		float horizontalSpeed = horizontalVelocity.magnitude;
        // Geschwindigkeit auf dem Boden pruefen
				bool isGrounded = GetDistanceToGround() < jumpHeight;
				bool isMoving = isGrounded && controller.velocity.magnitude > 0.1f;
    		bool isSprinting = horizontalSpeed > sprintSpeed; // an deine Sprint-Speed anpassen
        if (isMoving)
        {
            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0f || (isSprinting && stepTimer > sprintStepInterval))
            {
                PlayNextFootstep();
            		stepTimer = isSprinting ? sprintStepInterval : stepInterval;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    void PlayNextFootstep()
    {
        if (footstepSounds.Length == 0) return;

        AudioClip clip = footstepSounds[currentStepIndex];
        currentStepIndex = (currentStepIndex + 1) % footstepSounds.Length;

        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.PlayOneShot(clip);
    }

		float GetDistanceToGround()
		{
		    float radius = controller.radius;
		    // Von der Position des CharacterControllers aus messen, nicht von transform.position
		    // dieses Scripts - relevant, wenn das Script an einem Kind (z.B. Main Camera) haengt.
		    Vector3 origin = controller.transform.position + Vector3.up * radius;

		    if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, Mathf.Infinity))
		        return hit.distance - radius;

		    return Mathf.Infinity;
		}
}
