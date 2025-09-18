using System;
using System.Collections.Generic;
using System.Text;
using Unity.XGamingRuntime;
using UnityEngine;
using UnityEngine.Events;

public class XboxJsonCloudSync : MonoBehaviour
{
    public static XboxJsonCloudSync Instance { get; private set; }

    [Header("Container/Blob")]
    public bool useSingleContainer = true;
    public string defaultContainer = "json_sync_container";

    [Header("Eventos")]
    public UnityEvent<string> onSaving; // name
    [Serializable] public class NameJsonFoundEvent : UnityEvent<string, string, bool> { }
    public NameJsonFoundEvent onLoaded; // name, json, found

    [Serializable] public class NameHrEvent : UnityEvent<string, int> { }
    public NameHrEvent onSaved; // name, hr

    private XUserHandle _userHandle;
    private XGameSaveWrapper _gameSave;
    private bool _initialized;
    private readonly Queue<Action> _pendingGlobal = new Queue<Action>();

    // Estado por chave (blob)
    private class KeyState
    {
        public bool loadInFlight;
        public bool saveInFlight;
        public bool loadedOnce;
        public string lastJson; // opcional: último json lido (se quiser usar)
        public Action queuedSave; // mantém o ÚLTIMO save solicitado enquanto há um em voo
        public readonly Queue<Action> queue = new Queue<Action>(); // fila estrita por chave
    }
    private readonly Dictionary<string, KeyState> _byKey = new Dictionary<string, KeyState>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        TryInit();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (_userHandle != null) { SDK.XUserCloseHandle(_userHandle); _userHandle = null; }
    }

    public bool IsReady => _initialized;

    // ---------- API PÚBLICA ----------

    /// <summary>
    /// Carrega JSON da nuvem e dispara onLoaded(name, json, found).
    /// Serializa por "name": várias chamadas coexistem, mas só 1 load por vez executa.
    /// </summary>
    public void LoadJson(string name, Action<string, bool, int> onLoadedCb = null)
    {
        void Work()
        {
            var state = GetState(name);
            EnqueueKeyWork(state, () =>
            {
                state.loadInFlight = true;
                var (container, blob) = GetNames(name);
                Debug.Log($"[CloudSync] LOAD {name} <- {container}/{blob}");

                _gameSave.Load(container, blob, (int hr, byte[] data) =>
                {
                    state.loadInFlight = false;
                    bool found = false;
                    string json = string.Empty;

                    if (hr >= 0 && data != null && data.Length > 0)
                    {
                        try
                        {
                            json = Encoding.UTF8.GetString(data);
                            found = true;
                            state.loadedOnce = true;
                            state.lastJson = json;
                            Debug.Log($"[CloudSync] LOAD ok ({data.Length} bytes) para {name}.");
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"[CloudSync] LOAD decode erro: {ex}");
                        }
                    }
                    else if (hr < 0)
                    {
                        Debug.LogError($"[CloudSync] LOAD falhou: 0x{hr:X}");
                    }
                    else
                    {
                        Debug.LogWarning($"[CloudSync] LOAD vazio para {name}.");
                        state.loadedOnce = true; // marcou: primeiro load terminou (mesmo sem dados)
                    }

                    onLoaded?.Invoke(name, json, found);
                    onLoadedCb?.Invoke(json, found, hr);

                    // Se havia um SAVE adiado (coalescido) esperando o primeiro load, roda agora
                    if (state.queuedSave != null && !state.saveInFlight)
                    {
                        var pending = state.queuedSave;
                        state.queuedSave = null;
                        EnqueueKeyWork(state, pending);
                    }

                    DequeueNext(state);
                });
            });
        }

        if (!_initialized) { _pendingGlobal.Enqueue(Work); TryInit(); return; }
        Work();
    }

    /// <summary>
    /// Salva JSON na nuvem. Por padrão adia o SAVE até o primeiro LOAD dessa chave terminar
    /// para evitar sobrescrever com defaults.
    /// </summary>
    public void SaveJson(string name, string json, Action<int> onSavedHr = null, bool deferUntilFirstLoad = true)
    {
        void SaveOp()
        {
            var state = GetState(name);

            // Se já há um save em voo, coalesce: guarda apenas o MAIS RECENTE.
            Action doSave = () =>
            {
                state.saveInFlight = true;
                onSaving?.Invoke(name);

                var (container, blob) = GetNames(name);
                byte[] payload = Encoding.UTF8.GetBytes(json ?? "");
                Debug.Log($"[CloudSync] SAVE {name} -> {container}/{blob} ({payload.Length} bytes)");

                _gameSave.Save(container, blob, payload, (int hr) =>
                {
                    state.saveInFlight = false;

                    if (hr >= 0) Debug.Log($"[CloudSync] SAVE ok para {name} (0x{hr:X}).");
                    else Debug.LogError($"[CloudSync] SAVE falhou para {name}: 0x{hr:X}");

                    onSaved?.Invoke(name, hr);
                    onSavedHr?.Invoke(hr);

                    // Se enquanto salvava alguém solicitou outro Save, executa o ÚLTIMO agora
                    if (state.queuedSave != null)
                    {
                        var pending = state.queuedSave;
                        state.queuedSave = null;
                        EnqueueKeyWork(state, pending);
                    }

                    DequeueNext(state);
                });
            };

            // Guardas: adia até o primeiro LOAD se pedido
            if (deferUntilFirstLoad && !state.loadedOnce)
            {
                // Ainda não carregou: se tem load em voo, coalesce o SAVE e executa após o load.
                Debug.Log($"[CloudSync] SAVE adiado até primeiro LOAD para {name}.");
                state.queuedSave = doSave; // substitui qualquer anterior
                if (!state.loadInFlight)
                {
                    // Ninguém iniciou o LOAD ainda: vamos forçar um
                    LoadJson(name, null);
                }
                return;
            }

            // Se já há SAVE em voo, coalesce
            if (state.saveInFlight)
            {
                Debug.Log($"[CloudSync] SAVE coalescido (ainda em voo) para {name}.");
                state.queuedSave = doSave; // substitui
                return;
            }

            // Se há LOAD em voo, encadeia após o LOAD para manter ordem (Load->Save)
            if (state.loadInFlight)
            {
                Debug.Log($"[CloudSync] SAVE aguardando LOAD terminar para {name}.");
                state.queuedSave = doSave; // rodará ao final do LOAD
                return;
            }

            // Sem conflitos: pode salvar já
            EnqueueKeyWork(state, doSave);
        }

        if (!_initialized) { _pendingGlobal.Enqueue(SaveOp); TryInit(); return; }
        SaveOp();
    }

    // ---------- Internals ----------

    private (string container, string blob) GetNames(string name)
    {
        if (useSingleContainer) return (defaultContainer, name);

        string container = name;
        int dot = name.IndexOf('.');
        if (dot > 0) container = name.Substring(0, dot);
        return (container, name);
    }

    private KeyState GetState(string name)
    {
        if (!_byKey.TryGetValue(name, out var st))
        {
            st = new KeyState();
            _byKey[name] = st;
        }
        return st;
    }

    private void EnqueueKeyWork(KeyState st, Action op)
    {
        st.queue.Enqueue(op);
        if (st.queue.Count == 1) // ninguém em execução
            st.queue.Peek()?.Invoke();
    }

    private void DequeueNext(KeyState st)
    {
        if (st.queue.Count == 0) return;
        st.queue.Dequeue();
        if (st.queue.Count > 0)
            st.queue.Peek()?.Invoke();
    }

    private void TryInit(Action onReady = null)
    {
        if (_initialized) { onReady?.Invoke(); return; }

        if (!GDKGameRuntime.TryInitialize())
        {
            Debug.LogError("[CloudSync] Falha ao inicializar GDKGameRuntime. Execute no ambiente Microsoft Game Core.");
            return;
        }

        Debug.Log("[CloudSync] Adicionando usuário padrão…");
        SDK.XUserAddAsync(XUserAddOptions.AddDefaultUserAllowingUI, (int hrAdd, XUserHandle handle) =>
        {
            if (hrAdd < 0)
            {
                Debug.LogError($"[CloudSync] XUserAddAsync falhou: 0x{hrAdd:X}");
                return;
            }

            _userHandle = handle;
            _gameSave = new XGameSaveWrapper();

            string scid = GDKGameRuntime.GameConfigScid;
            _gameSave.InitializeAsync(_userHandle, scid, (int hrInit) =>
            {
                if (hrInit < 0)
                {
                    Debug.LogError($"[CloudSync] XGameSave.InitializeAsync falhou: 0x{hrInit:X}");
                    return;
                }

                _initialized = true;
                Debug.Log("[CloudSync] XGameSave pronto.");

                while (_pendingGlobal.Count > 0)
                {
                    try { _pendingGlobal.Dequeue()?.Invoke(); }
                    catch (Exception ex) { Debug.LogError($"[CloudSync] erro pendente: {ex}"); }
                }

                onReady?.Invoke();
            });
        });
    }
}
