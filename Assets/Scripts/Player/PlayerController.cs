using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Collections;

public class PlayerController : MonoBehaviourPunCallbacks
{
    public bool InvertLook = false;

    [Header("Player Skin")]
    [SerializeField] private Material[] _allSkins;

    [Header("Player General")]
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private GameObject _playerHitImpact;
    [SerializeField] private GameObject _playerModel;

    [Header("Player Health")]
    [SerializeField] private int _maxHealth = 100;
    private int _currentHealth;

    [Header("Player Vision")]
    [SerializeField] private Transform _viewPoint;
    [SerializeField] private Vector2 _mouseInput;
    [SerializeField] private float _mouseSensitivity = 5f;
    [SerializeField] private float _verticalRotStore;
    [SerializeField] private Camera _cam;

    [Header("Player Movement")]
    [SerializeField] private Vector3 _moveDirection, _movement;
    [SerializeField] private float _currentMoveSpeed, _moveSpeed = 5f, _runSpeed = 8f;
    [SerializeField] private float _moveY;
    [SerializeField] private float _jumpForce = 12f, _gravityMode = 2.5f;

    [Header("Player Jump")]
    [SerializeField] private Transform _groundCheckPoint;
    [SerializeField] private LayerMask _groundLayers;
    private bool _isGrounded;

    [Header("Player Animation")]
    [SerializeField] private Animator _playerAnimator;
    [SerializeField] private Transform _modelGunPoint, _gunHolder;

    [Header("Gun")]
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private float _maxHeat = 10f, _coolRate = 4f, _overHeatCoolRate = 5f;
    [SerializeField] private Gun[] _allGuns;
    [SerializeField] private Transform _adsInPoint, _adsOutPoint;
    private int _selectedGun;
    private float _shotCounter;
    private bool _overHeated;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        _cam = Camera.main;

        UIController.Instance.WeaponSlider.maxValue = _maxHeat;

        photonView.RPC("SetGun", RpcTarget.All, _selectedGun);

        _currentHealth = _maxHealth;

        UIController.Instance.MouseSensitivity.value = _mouseSensitivity;
        UIController.Instance.MouseSensitivyValue.text = _mouseSensitivity.ToString("0.0");

        if (photonView.IsMine)
        {
            _playerModel.SetActive(false);

            UIController.Instance.HealthSlider.maxValue = _maxHealth;
            UIController.Instance.HealthSlider.value = _currentHealth;
            UIController.Instance.HealthValue.text = _currentHealth.ToString();
        }
        else
        {
            _gunHolder.parent = _modelGunPoint;
            _gunHolder.localPosition = Vector3.zero;
            _gunHolder.localRotation = Quaternion.identity;
        }

        _playerModel.GetComponent<Renderer>().material = _allSkins[photonView.Owner.ActorNumber % _allSkins.Length];
    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            PlayerVision();
            PlayerMovement();
            VerifyCanShoot();
            TemperatureManager();

            if (Input.GetMouseButton(1))
            {
                _cam.fieldOfView = _allGuns[_selectedGun].AdsZoom;
                _gunHolder.position = _adsInPoint.position;
            }
            else
            {
                _cam.fieldOfView = 60f;
                _gunHolder.position = _adsOutPoint.position;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else if (Cursor.lockState == CursorLockMode.None)
            {
                if (Input.GetMouseButtonDown(0) && !UIController.Instance.ConfigPanel.activeInHierarchy)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    if (!_overHeated) _allGuns[_selectedGun].MuzzleFlash.SetActive(true);
                }
            }

            _mouseSensitivity = UIController.Instance.MouseSensitivity.value;
            UIController.Instance.MouseSensitivyValue.text = _mouseSensitivity.ToString("0.0");
        }
    }

    private void LateUpdate()
    {
        if (photonView.IsMine)
        {
            if (MatchManager.Instance.State == MatchManager.GameStates.Playing)
            {
                _cam.transform.SetPositionAndRotation(_viewPoint.position, _viewPoint.rotation);
            }
            else
            {
                _cam.transform.SetPositionAndRotation(MatchManager.Instance.MapCamPoint.position, MatchManager.Instance.MapCamPoint.rotation);
            }

            _allGuns[_selectedGun].MuzzleFlash.SetActive(false);
        }
    }

    private void PlayerVision()
    {
        _mouseInput = new Vector2(Input.GetAxisRaw("Mouse X") * _mouseSensitivity, Input.GetAxisRaw("Mouse Y")) * _mouseSensitivity;

        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + _mouseInput.x, transform.rotation.eulerAngles.z);

        _verticalRotStore += _mouseInput.y;
        _verticalRotStore = Mathf.Clamp(_verticalRotStore, -60f, 60f);

        if (InvertLook) _viewPoint.rotation = Quaternion.Euler(_verticalRotStore, _viewPoint.rotation.eulerAngles.y, _viewPoint.rotation.eulerAngles.z);
        else _viewPoint.rotation = Quaternion.Euler(-_verticalRotStore, _viewPoint.rotation.eulerAngles.y, _viewPoint.rotation.eulerAngles.z);
    }

    private void PlayerMovement()
    {
        _moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));

        if (Input.GetKey(KeyCode.LeftShift))
        {
            _currentMoveSpeed = _runSpeed;
        }
        else
        {
            _currentMoveSpeed = _moveSpeed;
        }

        float yVelocity = _movement.y;
        _movement = ((transform.forward * _moveDirection.z) + (transform.right * _moveDirection.x)).normalized * _currentMoveSpeed;
        _movement.y = yVelocity;

        if (_characterController.isGrounded)
        {
            _movement.y = 0f;
        }

        _isGrounded = Physics.Raycast(_groundCheckPoint.position, Vector3.down, .25f, _groundLayers);

        if (Input.GetButtonDown("Jump") && _isGrounded)
        {
            _movement.y = _jumpForce;
        }

        _movement.y += Physics.gravity.y * Time.deltaTime * _gravityMode;

        _characterController.Move(_currentMoveSpeed * Time.deltaTime * _movement);

        _playerAnimator.SetBool("grounded", _isGrounded);
        _playerAnimator.SetFloat("speed", _moveDirection.magnitude);
    }

    private void Shoot()
    {
        Ray ray = _cam.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
        ray.origin = _cam.transform.position;

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject.CompareTag("Player"))
            {
                PhotonNetwork.Instantiate(_playerHitImpact.name, hit.point, Quaternion.identity);

                hit.collider.gameObject.GetPhotonView().RPC("DealDamage", RpcTarget.All, photonView.Owner.NickName, _allGuns[_selectedGun].ShotDamage, PhotonNetwork.LocalPlayer.ActorNumber);
            }
            else
            {
                GameObject bulletImpact = Instantiate(_bulletPrefab, hit.point + (hit.normal * .002f), Quaternion.LookRotation(Vector3.forward, Vector3.up));

                Destroy(bulletImpact, 5f);
            }
        }

        _shotCounter = _allGuns[_selectedGun].TimeBetweenShot;

        _allGuns[_selectedGun].HeatCounter += _allGuns[_selectedGun].HeatPerShot;
        if (_allGuns[_selectedGun].HeatCounter >= _maxHeat)
        {
            _allGuns[_selectedGun].HeatCounter = _maxHeat;

            _overHeated = true;

            UIController.Instance.Crosshair.GetComponent<Image>().sprite = UIController.Instance.CrosshairList[0];
        }
        _allGuns[_selectedGun].ShotAudio.Stop();
        _allGuns[_selectedGun].ShotAudio.Play();
    }

    private void VerifyCanShoot()
    {
        if (!_overHeated)
        {
            if (Input.GetMouseButtonDown(0) && !UIController.Instance.ConfigPanel.activeInHierarchy) Shoot();

            if (Input.GetMouseButton(0) && _allGuns[_selectedGun].IsAutomatic)
            {
                _shotCounter -= Time.deltaTime;

                if (_shotCounter <= 0 && !UIController.Instance.ConfigPanel.activeInHierarchy) Shoot();
            }
            _allGuns[_selectedGun].HeatCounter -= _coolRate * Time.deltaTime;
        }
        else
        {
            _allGuns[_selectedGun].HeatCounter -= _overHeatCoolRate * Time.deltaTime;
        }

        if (_allGuns[_selectedGun].HeatCounter <= 0f)
        {
            _allGuns[_selectedGun].HeatCounter = 0f;
            _overHeated = false;

            UIController.Instance.Crosshair.GetComponent<Image>().sprite = UIController.Instance.CrosshairList[1];
        }

        SwitchGun();
    }

    private void TemperatureManager()
    {
        UIController.Instance.WeaponSlider.value = _allGuns[_selectedGun].HeatCounter;
        _allGuns[_selectedGun].LastHeatCounter = _allGuns[_selectedGun].HeatCounter;
    }

    private void SwitchGun()
    {
        if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
        {
            _selectedGun++;

            if (_selectedGun >= _allGuns.Length) _selectedGun = 0;
            photonView.RPC("SetGun", RpcTarget.All, _selectedGun);
        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
        {
            _selectedGun--;

            if (_selectedGun < 0) _selectedGun = _allGuns.Length - 1;
            photonView.RPC("SetGun", RpcTarget.All, _selectedGun);
        }

        for (int i = 0; i < _allGuns.Length; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                _selectedGun = i;
                photonView.RPC("SetGun", RpcTarget.All, _selectedGun);
            }
        }
    }

    private void ChangeGun()
    {
        foreach (Gun gun in _allGuns)
        {
            gun.gameObject.SetActive(false);
        }
        _allGuns[_selectedGun].gameObject.SetActive(true);
        _allGuns[_selectedGun].HeatCounter = _allGuns[_selectedGun].LastHeatCounter;
        if (_allGuns[_selectedGun].LastHeatCounter > 0f)
        {
            _overHeated = true;
            UIController.Instance.Crosshair.GetComponent<Image>().sprite = UIController.Instance.CrosshairList[0];
        }
    }

    public void TakeDamage(string damager, int damageAmount, int actor)
    {
        if (photonView.IsMine)
        {
            _currentHealth -= damageAmount;
            if (_currentHealth <= 0)
            {
                _currentHealth = 0;
                PlayerSpawner.Instance.Die(damager);

                MatchManager.Instance.UpdateStatsSend(actor, 0, 1);
            }
            UIController.Instance.HealthSlider.value = _currentHealth;
            UIController.Instance.HealthValue.text = _currentHealth.ToString();
        }
    }

    [PunRPC]
    public void DealDamage(string damager, int damageAmount, int actor)
    {
        TakeDamage(damager, damageAmount, actor);
    }

    [PunRPC]
    public void SetGun(int gunToSwitchTo)
    {
        if (gunToSwitchTo < _allGuns.Length)
        {
            _selectedGun = gunToSwitchTo;
            ChangeGun();
        }
    }

}
