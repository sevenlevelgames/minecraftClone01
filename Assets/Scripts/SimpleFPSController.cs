using UnityEngine;

// Player objesinde bir CharacterController bileþeni olmasýný zorunlu kýlar
[RequireComponent(typeof(CharacterController))]
public class SimpleFPSController : MonoBehaviour
{
    [Header("Kamera")]
    public Transform playerCamera; // Kamerayý sürükle

    [Header("Hareket Ayarlarý")]
    public float moveSpeed = 6.0f;
    public float jumpHeight = 1.0f;
    public float gravity = -9.81f;

    [Header("Bakýþ Ayarlarý")]
    public float lookSensitivity = 2.0f;
    public float lookXLimit = 80.0f; // Dikey bakýþ limiti

    // Dahili Deðiþkenler
    private CharacterController controller;
    private Vector3 velocity; // Yerçekimi ve zýplama hýzý
    private bool isGrounded;
    private float rotationX = 0; // Dikey fare rotasyonu

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // Fareyi ekranýn ortasýna kilitle ve gizle
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Kameranýn atanýp atanmadýðýný kontrol et
        if (playerCamera == null)
        {
            Debug.LogError("Lütfen 'Player Camera' objesini Inspector'dan atayýn!");
            this.enabled = false; // Script'i devre dýþý býrak
        }
    }

    void Update()
    {
        HandleMovement();
        HandleLook();
    }

    void HandleMovement()
    {
        // Yerde olup olmadýðýmýzý kontrol et
        isGrounded = controller.isGrounded;

        // Eðer yerdeysek ve düþmüyorsak, yerçekimi hýzýný sýfýrla (hafif bir eksi deðerde tut)
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // --- HAREKET ---
        // Klasik Input Manager'dan giriþleri al
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Hareket vektörünü karakterin baktýðý yöne göre hesapla
        Vector3 move = transform.right * horizontal + transform.forward * vertical;

        // Hareketi uygula
        controller.Move(move * moveSpeed * Time.deltaTime);

        // --- ZIPLAMA ---
        // "Jump" tuþuna (varsayýlan: Space) basýldýysa ve yerdeyse zýpla
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            // Zýplama formülü: v = sqrt(h * -2 * g)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // --- YERÇEKÝMÝ ---
        // Her saniye yerçekimini uygula
        velocity.y += gravity * Time.deltaTime;

        // Son hýzý (yerçekimi/zýplama) karaktere uygula
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleLook()
    {
        // --- FARE GÝRÝÞÝ ---
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

        // --- DÝKEY BAKIÞ (Kamera) ---
        // Dikey dönüþü (Pitch) hesapla ve limitlendir
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);

        // Dikey dönüþü SADECE kameraya uygula
        playerCamera.localRotation = Quaternion.Euler(rotationX, 0, 0);

        // --- YATAY BAKIÞ (Tüm Karakter) ---
        // Yatay dönüþü (Yaw) TÜM KARAKTERE (Player objesine) uygula
        transform.Rotate(Vector3.up * mouseX);
    }
}