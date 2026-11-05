using UnityEngine;

namespace Aerofobia.Flight
{
    /// <summary>
    /// Escolhe em qual poltrona o paciente senta. Ao iniciar a cena, move o jogador (XR Origin),
    /// as gotas de chuva da janela e os painéis para a poltrona escolhida.
    /// </summary>
    /// <remarks>
    /// O avião é de asa alta: dependendo da fileira, a hélice fica à vista ou não. A fileira será decidida
    /// no teste com o terapeuta (PENDENCIAS, seção 2); por isso a troca é só um número no Inspector.
    /// Os objetos mantêm a posição relativa que têm na poltrona padrão.
    /// </remarks>
    public class SeatSelector : MonoBehaviour
    {
        [Tooltip("Pontos das poltronas (no piso, na janela). O nome de cada um diz onde fica.")]
        [SerializeField] private Transform[] _seats = new Transform[0];

        [Tooltip("Índice da poltrona usada (0 = primeira da lista).")]
        [SerializeField, Min(0)] private int _seatIndex;

        [Tooltip("O que acompanha o paciente: XR_Player, gotas de chuva, painéis.")]
        [SerializeField] private Transform[] _passengerObjects = new Transform[0];

        /// <summary>Poltrona em uso.</summary>
        public Transform CurrentSeat => _seats.Length == 0 ? null : _seats[Mathf.Clamp(_seatIndex, 0, _seats.Length - 1)];

        void Awake()
        {
            var seat = CurrentSeat;
            if (seat == null)
            {
                return;
            }

            foreach (var passengerObject in _passengerObjects)
            {
                if (passengerObject != null && passengerObject.parent != seat)
                {
                    passengerObject.SetParent(seat, worldPositionStays: false);
                }
            }
        }

        void OnValidate()
        {
            if (_seats.Length > 0)
            {
                _seatIndex = Mathf.Clamp(_seatIndex, 0, _seats.Length - 1);
            }
        }
    }
}
