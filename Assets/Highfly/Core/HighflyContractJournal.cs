using UnityEngine;

namespace Highfly.Core
{
    public sealed class HighflyContractJournal : MonoBehaviour
    {
        private const string ContractFStateKey = "HIGHFLY_CONTRACT_F_STATE";

        // 0 available, 1 active, 2 cleared, 3 claimed.
        public int ContractFState { get; private set; }

        private HighflyHunterProgression _progression;

        private void Awake()
        {
            _progression = GetComponent<HighflyHunterProgression>();
            ContractFState = Mathf.Clamp(PlayerPrefs.GetInt(ContractFStateKey, 0), 0, 3);
        }

        public string InteractWithSerin()
        {
            if (ContractFState == 2)
            {
                if (_progression != null)
                {
                    _progression.AddExperience(180);
                    _progression.AddGold(120);
                    _progression.AddGateKeys(1);
                }

                ContractFState = 3;
                Save();
                return "CONTRATO COMPLETADO: Cripta F. Recompensa: +180 XP, +120 oro y +1 llave.";
            }

            if (ContractFState == 1)
                return "Contrato activo: limpiá la Cripta F y derrotá al Guardián del Portal.";

            ContractFState = 1;
            Save();
            return "CONTRATO ACEPTADO: Cripta F. Objetivo: limpiar las cámaras y derrotar al Guardián.";
        }

        public void MarkCriptaFCleared()
        {
            if (ContractFState == 2)
                return;

            ContractFState = 2;
            Save();
        }

        private void Save()
        {
            PlayerPrefs.SetInt(ContractFStateKey, ContractFState);
            PlayerPrefs.Save();
        }
    }
}
