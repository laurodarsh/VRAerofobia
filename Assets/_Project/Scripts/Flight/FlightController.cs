using System;
using UnityEngine;
using UnityEngine.Events;

namespace Aerofobia.Flight
{
    /// <summary>
    /// Máquina de estados do voo da cena 3 (substitui o <c>Voo.cs</c> do projeto antigo).
    /// Move o avião pelo <see cref="FlightCircuit"/>: acelera na pista, sobe acima das nuvens,
    /// voa em volta do aeroporto e, quando o médico pede, desce e pousa na mesma pista.
    /// </summary>
    /// <remarks>
    /// Conforto em VR (CLAUDE.md, seção 6): o movimento é cinemático e contínuo, sem física.
    /// Aceleração, freada, subida, descida, inclinação nas curvas e arfagem mudam aos poucos
    /// (rampas e curvas suaves), nunca de um frame para o outro. O jogador (XR Origin) é filho do avião:
    /// a câmera nunca é movida por código. Este componente fica na raiz do avião, cujo pivô está no
    /// ponto em que as rodas tocam o chão.
    /// </remarks>
    public class FlightController : MonoBehaviour
    {
        const float Gravity = 9.81f;

        [Header("Trajetória")]
        [Tooltip("Oval do voo, na cabeceira da pista.")]
        [SerializeField] private FlightCircuit _circuit;

        [Tooltip("Onde o avião espera a decolagem: metros depois da cabeceira (o pivô fica no meio do avião).")]
        [SerializeField, Min(0f)] private float _startOnRunway = 18f;

        [Tooltip("Onde as rodas tocam no pouso: metros depois da cabeceira.")]
        [SerializeField, Min(0f)] private float _touchdownOnRunway = 60f;

        [Header("Velocidade (m/s)")]
        [Tooltip("Velocidade em que o avião sai do chão.")]
        [SerializeField, Min(1f)] private float _liftoffSpeed = 40f;

        [Tooltip("Velocidade no cruzeiro.")]
        [SerializeField, Min(1f)] private float _cruiseSpeed = 65f;

        [Tooltip("Velocidade na aproximação e no toque na pista.")]
        [SerializeField, Min(1f)] private float _approachSpeed = 42f;

        [Tooltip("Aceleração na pista durante a decolagem (m/s²). A pista do modelo tem 460 m: acima de ~2 m/s² o avião sai do chão antes do fim.")]
        [SerializeField, Min(0.1f)] private float _takeoffAcceleration = 2.5f;

        [Tooltip("Desaceleração na pista depois do pouso (m/s²). Com os valores padrão o avião para a ~400 m da cabeceira (a pista tem 460 m).")]
        [SerializeField, Min(0.1f)] private float _brakingDeceleration = 3.2f;

        [Tooltip("Quanto a velocidade muda por segundo no ar (m/s²).")]
        [SerializeField, Min(0.05f)] private float _airSpeedChange = 0.8f;

        [Tooltip("Tempo (s) para a aceleração e a freada chegarem ao máximo. Evita o tranco do início.")]
        [SerializeField, Min(0.1f)] private float _accelerationRampTime = 3f;

        [Header("Altitude (m)")]
        [Tooltip("Altitude de cruzeiro, acima das nuvens.")]
        [SerializeField, Min(50f)] private float _cruiseAltitude = 450f;

        [Tooltip("Distância percorrida na subida, da saída do chão até o cruzeiro. Maior = subida mais suave.")]
        [SerializeField, Min(500f)] private float _climbDistance = 5500f;

        [Tooltip("Distância mínima da descida. Se o pedido de pouso chegar perto demais da pista, o avião dá mais uma volta.")]
        [SerializeField, Min(500f)] private float _minApproachDistance = 5000f;

        [Header("Atitude (graus e segundos)")]
        [Tooltip("Inclinação máxima nas curvas. A ESPECIFICACAO pede no máximo 10–15°.")]
        [SerializeField, Range(0f, 15f)] private float _maxBankAngle = 12f;

        [Tooltip("Tempo para entrar e sair da inclinação das curvas.")]
        [SerializeField, Min(0.1f)] private float _bankSmoothTime = 2.5f;

        [Tooltip("Tempo para o nariz subir ou descer.")]
        [SerializeField, Min(0.1f)] private float _pitchSmoothTime = 1.2f;

        [Tooltip("Nariz levemente para cima quando está no ar, como num avião de verdade.")]
        [SerializeField, Range(0f, 5f)] private float _airbornePitch = 2f;

        [Header("Eventos")]
        [Tooltip("O avião começou a acelerar na pista.")]
        [SerializeField] private UnityEvent _takeoffStarted = new UnityEvent();

        [Tooltip("O médico pediu o pouso e a descida começou.")]
        [SerializeField] private UnityEvent _approachStarted = new UnityEvent();

        [Tooltip("O avião parou na pista depois do pouso. Use para mostrar o painel \"Voo concluído\".")]
        [SerializeField] private UnityEvent _flightCompleted = new UnityEvent();

        FlightState _state = FlightState.Pronto;
        float _distance;
        float _altitude;
        float _speed;
        float _acceleration;
        float _segmentTravelled;
        float _segmentLength;
        float _approachStartAltitude;
        bool _approachRequested;
        float _pitch;
        float _pitchVelocity;
        float _bank;
        float _bankVelocity;

        /// <summary>Disparado a cada troca de etapa (sons, efeitos, depuração).</summary>
        public event Action<FlightState> StateChanged;

        /// <summary>Etapa atual do voo.</summary>
        public FlightState State => _state;

        /// <summary>Velocidade atual (m/s).</summary>
        public float Speed => _speed;

        /// <summary>Altura das rodas acima da pista (m).</summary>
        public float Altitude => _altitude;

        void Start()
        {
            if (_circuit == null)
            {
                Debug.LogError("[FlightController] FlightCircuit não definido.", this);
                enabled = false;
                return;
            }

            _distance = _circuit.RunwayToDistance(_startOnRunway);
            ApplyPose(0f);
        }

        /// <summary>
        /// Comando do médico (gatilho segurado): decola se o avião está parado; pede o pouso se está
        /// subindo ou em cruzeiro. Pedido durante a subida fica guardado e vale ao chegar no cruzeiro.
        /// Nas demais etapas não faz nada.
        /// </summary>
        [ContextMenu("Decolar ou pousar")]
        public void RequestTakeoffOrLanding()
        {
            switch (_state)
            {
                case FlightState.Pronto:
                    _acceleration = 0f;
                    SetState(FlightState.Decolagem);
                    _takeoffStarted.Invoke();
                    break;
                case FlightState.Subida:
                case FlightState.Cruzeiro:
                    _approachRequested = true;
                    break;
            }
        }

        /// <summary>
        /// Atalho de teste no Editor: coloca o avião direto no cruzeiro, no fim da subida,
        /// para não esperar a decolagem inteira a cada teste.
        /// </summary>
        [ContextMenu("Teste: ir direto para o cruzeiro")]
        public void SkipToCruise()
        {
            if (_circuit == null)
            {
                return;
            }

            _distance = _circuit.RunwayToDistance(_touchdownOnRunway) + _climbDistance;
            _altitude = _cruiseAltitude;
            _speed = _cruiseSpeed;
            SetState(FlightState.Cruzeiro);
            ApplyPose(0f);
        }

        void Update()
        {
            Advance(Time.deltaTime);
        }

        /// <summary>Avança o voo <paramref name="dt"/> segundos. Separado do Update para poder ser simulado em testes.</summary>
        void Advance(float dt)
        {
            if (dt <= 0f)
            {
                return;
            }

            float slope = 0f; // dh/ds, para a arfagem
            switch (_state)
            {
                case FlightState.Decolagem:
                    _acceleration = Mathf.MoveTowards(_acceleration, _takeoffAcceleration, _takeoffAcceleration / _accelerationRampTime * dt);
                    _speed += _acceleration * dt;
                    if (_speed >= _liftoffSpeed)
                    {
                        _segmentTravelled = 0f;
                        SetState(FlightState.Subida);
                    }
                    break;

                case FlightState.Subida:
                    _speed = Mathf.MoveTowards(_speed, _cruiseSpeed, _airSpeedChange * dt);
                    _segmentTravelled += _speed * dt;
                    float climb = Mathf.Clamp01(_segmentTravelled / _climbDistance);
                    _altitude = _cruiseAltitude * SmoothStep(climb);
                    slope = _cruiseAltitude * SmoothStepDerivative(climb) / _climbDistance;
                    if (climb >= 1f)
                    {
                        SetState(FlightState.Cruzeiro);
                    }
                    break;

                case FlightState.Cruzeiro:
                    _speed = Mathf.MoveTowards(_speed, _cruiseSpeed, _airSpeedChange * dt);
                    _altitude = _cruiseAltitude;
                    if (_approachRequested)
                    {
                        BeginApproach();
                    }
                    break;

                case FlightState.Aproximacao:
                    _speed = Mathf.MoveTowards(_speed, _approachSpeed, _airSpeedChange * dt);
                    _segmentTravelled += _speed * dt;
                    float descent = Mathf.Clamp01(_segmentTravelled / _segmentLength);
                    _altitude = _approachStartAltitude * (1f - SmoothStep(descent));
                    slope = -_approachStartAltitude * SmoothStepDerivative(descent) / _segmentLength;
                    if (descent >= 1f)
                    {
                        _altitude = 0f;
                        _acceleration = 0f;
                        SetState(FlightState.Pouso);
                    }
                    break;

                case FlightState.Pouso:
                    _acceleration = Mathf.MoveTowards(_acceleration, _brakingDeceleration, _brakingDeceleration / _accelerationRampTime * dt);
                    // Perto de parar, a freada alivia (como um motorista cuidadoso), para não dar tranco no fim.
                    float easing = Mathf.Clamp01(_speed / 8f);
                    _speed = Mathf.Max(0f, _speed - _acceleration * Mathf.Max(easing, 0.15f) * dt);
                    if (_speed <= 0.01f)
                    {
                        _speed = 0f;
                        SetState(FlightState.Concluido);
                        _flightCompleted.Invoke();
                    }
                    break;
            }

            _distance += _speed * dt;
            ApplyPose(dt, slope);
        }

        void BeginApproach()
        {
            _approachRequested = false;
            float touchdown = _circuit.RunwayToDistance(_touchdownOnRunway);
            _segmentLength = _circuit.DistanceAhead(_distance, touchdown);
            if (_segmentLength < _minApproachDistance)
            {
                _segmentLength += _circuit.Length;
            }

            _segmentTravelled = 0f;
            _approachStartAltitude = _altitude;
            SetState(FlightState.Aproximacao);
            _approachStarted.Invoke();
        }

        /// <summary>
        /// Posiciona e orienta o avião. Com <paramref name="dt"/> = 0 aplica a atitude na hora
        /// (usado no início e nos atalhos de teste); senão, suaviza arfagem e inclinação.
        /// </summary>
        void ApplyPose(float dt, float slope = 0f)
        {
            _circuit.SampleLocal(_distance, out var groundPoint, out var forward, out var curvature);

            // Arfagem: acompanha a rampa de subida/descida, mais um pouco de nariz para cima no ar.
            float airborne = Mathf.Clamp01(_altitude / 30f);
            float targetPitch = Mathf.Atan(slope) * Mathf.Rad2Deg + _airbornePitch * airborne;

            // Inclinação: a de uma curva coordenada (tan = v²·curvatura/g), com teto, e só no ar.
            float coordinated = Mathf.Atan(_speed * _speed * curvature / Gravity) * Mathf.Rad2Deg;
            float targetBank = Mathf.Clamp(coordinated, -_maxBankAngle, _maxBankAngle) * Mathf.Clamp01(_altitude / 50f);

            if (dt > 0f)
            {
                _pitch = Mathf.SmoothDamp(_pitch, targetPitch, ref _pitchVelocity, _pitchSmoothTime, Mathf.Infinity, dt);
                _bank = Mathf.SmoothDamp(_bank, targetBank, ref _bankVelocity, _bankSmoothTime, Mathf.Infinity, dt);
            }
            else
            {
                _pitch = targetPitch;
                _bank = targetBank;
                _pitchVelocity = 0f;
                _bankVelocity = 0f;
            }

            var circuitTransform = _circuit.transform;
            var position = circuitTransform.TransformPoint(groundPoint + Vector3.up * _altitude);
            // Euler: X negativo levanta o nariz; Z positivo abaixa a asa esquerda (curva para a esquerda).
            var rotation = circuitTransform.rotation * Quaternion.LookRotation(forward) * Quaternion.Euler(-_pitch, 0f, _bank);
            transform.SetPositionAndRotation(position, rotation);
        }

        void SetState(FlightState state)
        {
            if (_state == state)
            {
                return;
            }

            _state = state;
            StateChanged?.Invoke(state);
        }

        static float SmoothStep(float t) => t * t * (3f - 2f * t);

        static float SmoothStepDerivative(float t) => 6f * t * (1f - t);
    }
}
