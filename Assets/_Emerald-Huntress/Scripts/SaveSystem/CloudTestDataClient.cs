using System;
using TMPro;
using UnityEngine;

/// <summary>
/// Exemplo mínimo que usa o XboxJsonCloudSync como serviço.
/// Hotkeys:
///  - P: incrementa o contador em memória
///  - S: salva na nuvem (json)
///  - L: carrega da nuvem e aplica no contador
/// </summary>
public class CloudTestDataClient : MonoBehaviour
{
    [Header("Identificador do blob (name)")]
    public string nameKey = "TestData.json";

    [Header("Estado em memória (exemplo)")]
    public int count;

    public TMP_Text view;
    private PlayerInputs _inputs;

    private void Awake()
    {
        // opcional: assina eventos globais
        if (XboxJsonCloudSync.Instance != null)
        {
            XboxJsonCloudSync.Instance.onSaving.AddListener(OnSaving);
            XboxJsonCloudSync.Instance.onLoaded.AddListener(OnLoaded);
        }

        _inputs = new PlayerInputs();
        _inputs.Enable();
    }

    private void OnDestroy()
    {
        if (XboxJsonCloudSync.Instance != null)
        {
            XboxJsonCloudSync.Instance.onSaving.RemoveListener(OnSaving);
            XboxJsonCloudSync.Instance.onLoaded.RemoveListener(OnLoaded);
        }

        _inputs.Disable();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) || _inputs.Gameplay.Attack0.triggered)
        {
            count += 1;
            Debug.Log($"[Client] count = {count}");
        }

        if (Input.GetKeyDown(KeyCode.S) || _inputs.Gameplay.Attack1.triggered)
        {
            // Monta um JSON simples
            var payload = JsonUtility.ToJson(new TestData { count = count, updatedAt = DateTime.UtcNow.ToString("o") });
            XboxJsonCloudSync.Instance.SaveJson(nameKey, payload, (hr) =>
            {
                if (IsSuccess(hr)) Debug.Log("[Client] SAVE ok.");
                else Debug.LogError($"[Client] SAVE falhou: hr=0x{hr:X}");
            });
        }

        if (Input.GetKeyDown(KeyCode.L) || _inputs.Gameplay.Special.triggered)
        {
            XboxJsonCloudSync.Instance.LoadJson(nameKey, (json, found, hr) =>
            {
                if (!found)
                {
                    Debug.LogWarning($"[Client] LOAD sem dados para {nameKey} (hr=0x{hr:X}). Mantendo estado atual.");
                    return;
                }

                try
                {
                    var data = JsonUtility.FromJson<TestData>(json);
                    count = data.count;
                    Debug.Log($"[Client] LOAD ok. count={count} updatedAt={data.updatedAt}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[Client] Erro ao aplicar JSON: {ex}");
                }
            });
        }

        view.text = count.ToString();
    }


    [Serializable]
    private class TestData
    {
        public int count;
        public string updatedAt;
    }

    // --- Utilitário local para HRESULT ---
    private static bool IsSuccess(int hr) => hr >= 0;

    // --- Handlers dos eventos globais (opcional) ---
    private void OnSaving(string name) => Debug.Log($"[Client] onSaving: {name}");
    private void OnLoaded(string name, string json, bool found) =>
        Debug.Log($"[Client] onLoaded: {name}, found={found}, size={(json?.Length ?? 0)} chars");
}
