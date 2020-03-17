using System.Collections.Generic;
using LoL.Data;
using LoL.SimpleJSON;
using JSONObject = LoL.SimpleJSON.JSONObject;
#if LOL_GAMEFRAME
using Proto.Promises;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LoL.Data
{
    public interface IStateSaveable : IDeserializable, IResetable, ISerializable { }

    public class StateData<T> : IStateSaveable
        where T : IStateSaveable
    {
        string key;
        int version;
        T data;

        public void Set(string key, int version, T data)
        {
            this.key = key;
            this.version = version;
            this.data = data;
        }

        public void Deserialize(JSONNode jsonNode)
        {
            key = jsonNode[nameof(key)];
            version = jsonNode[nameof(version)];
            data = jsonNode[nameof(data)].Deserialize<T>();
        }

        public void Reset()
        {
            key = default;
            version = default;
            data = default;
        }

        public JSONNode Serialize()
        {
            return new SimpleJSON.JSONObject()
                .Write(nameof(key), key)
                .Write(nameof(version), version)
                .Write(nameof(data), data);
        }
    }
}

namespace LoL
{
    public class StateAttribute : DeserializableTypeAttribute
    {
        /// <summary>
        /// The order the generator files will be create or updated in.
        /// 0 == first.
        /// </summary>
        /// <value>The create order.</value>
        public int CreateOrder { get; private set; }
        /// <summary>
        /// Generates serialize, deserialize, reset, save, load, and ToString methods for the data.
        /// </summary>
        public StateAttribute() { }
        /// <summary>
        /// Generates serialize, deserialize, reset, save, load, and ToString methods for the data.
        /// </summary>
        /// <param name="createOrder">The order the generator files will be create or updated in. 0 == first</param>
        public StateAttribute(int createOrder) { CreateOrder = createOrder; }
    }

    public static class State
    {

#if LOL_GAMEFRAME
        static MakeAPICallAction _returnApiAction;
#else
        static string _returnSceneName;
#endif

        private interface IDelegateArg
        {
            void DeserializeAndInvoke(JSONNode jsonNode);
        }

        private class DelegateArg<T> : IDelegateArg where T : IStateSaveable
        {
            System.Action<T> _callback;

            public DelegateArg(System.Action<T> callback)
            {
                _callback = callback;
            }

            public void DeserializeAndInvoke(JSONNode jsonNode)
            {
                T arg = jsonNode.Deserialize<T>();
                Invoke(arg);
            }

            public void Invoke(T arg)
            {
                _callback.Invoke(arg);
            }

            public void AddCallback(System.Action<T> callback)
            {
                _callback += callback;
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Used to force reset the database entry from the editor while testing.
        /// </summary>
        private class ResetData : IStateSaveable
        {
            void IDeserializable.Deserialize(JSONNode jsonNode) { }

            void IResetable.Reset() { }

            JSONNode ISerializable.Serialize()
            {
                return new SimpleJSON.JSONObject()
                    .Write("manual_reset", true);
            }
        }
#endif

        static readonly Dictionary<string, IDelegateArg> _loadCallbacks = new Dictionary<string, IDelegateArg>();

        public static void Save<T>(string key, int version, T data) where T : IStateSaveable
        {
            var stateData = Types.TakeOrCreate<StateData<T>>();
            stateData.Set(key, version, data);

            _Save(key, version, stateData);
        }

        static void _Save(string key, int version, IStateSaveable stateData)
        {
#if LOL_GAMEFRAME
            // Save to db through ws action.
            var saveApiCall = QuestAction.Create<MakeAPICallAction>();
            saveApiCall.Event = "state_save_request";
            saveApiCall.async = true;
            saveApiCall.block_input = false;
            saveApiCall.show_spinner = false;
            saveApiCall.data = stateData.Serialize() as JSONObject;
            saveApiCall.Execute();
#else
            PlayerPrefs.SetString($@"{key}_{version}", stateData.Serialize().ToString());
#endif
        }

#if UNITY_EDITOR
        public static void Reset(string key, int version)
        {
            var stateData = Types.TakeOrCreate<StateData<ResetData>>();
            var resetData = Types.TakeOrCreate<ResetData>();
            stateData.Set(key, version, resetData);
            _Save(key, version, stateData);
        }
#endif

        public static void Load<T>(string key, int version, System.Action<T> onLoad) where T : IStateSaveable
        {
            // Get the data from the db by the key through ws action.
            if (_loadCallbacks.TryGetValue(key, out var del))
            {
                throw new System.ArgumentException("Key is already waiting to load: " + key, nameof(key));
            }
            _loadCallbacks[key] = new DelegateArg<T>(onLoad);

#if LOL_GAMEFRAME
            var loadApiCall = QuestAction.Create<MakeAPICallAction>();
            loadApiCall.Event = "state_load_request";
            loadApiCall.data = new JSONObject { [nameof(key)] = key, [nameof(version)] = version };
            GameController.GameSwitch(loadApiCall);
#else
            var stateData = (SimpleJSON.JSONObject)JSON.Parse(PlayerPrefs.GetString($"{key}_{version}"));
            if (stateData == null)
            {
                stateData = new SimpleJSON.JSONObject { [nameof(key)] = key, [nameof(version)] = version, ["data"] = null };
            }
            OnLoaded(stateData);
#endif
        }

        /// <summary>
        /// Called from the StateLoadAction or Load if without gameframe.
        /// </summary>
        /// <param name="stateData">State data.</param>
        public static void OnLoaded(SimpleJSON.JSONObject stateData)
        {
            string key = stateData[nameof(key)];
            if (_loadCallbacks.TryGetValue(key, out var del))
            {
#if UNITY_EDITOR
                // Check for manual reset and null data if found.
                if (!stateData["data"].IsNull)
                {
                    // Database will not update entry with a null data field so use this object as a reset..
                    var manual_reset = stateData["data"]["manual_reset"].AsBool;
                    if (manual_reset)
                    {
                        stateData["data"] = null;
                    }
                }
#endif
                del.DeserializeAndInvoke(stateData["data"]);
                _loadCallbacks.Remove(key);
            }
        }

        #region Scene Loading
        /// <summary>
        /// Loads the return scene.
        /// </summary>
        public static void LoadReturnScene()
        {
#if LOL_GAMEFRAME
            if (_returnApiAction != null)
            {
                ExecuteReturnAction();
                return;
            }
#else
            if (string.IsNullOrEmpty(_returnSceneName))
            {
#if LOL_GAMEFRAME
                Utils.LoLDebug.LogError("State return scene not set.");
#else
                Debug.LogError("State return scene not set.");
#endif
                return;
            }

            LoadScene(_returnSceneName);
            _returnSceneName = null;
#endif
        }

        public static void LoadSceneSetReturn(string loadSceneName)
        {
#if LOL_GAMEFRAME
            LoadSceneSetReturn(loadSceneName, "load_base_request", null);
            return;
#else
            _returnSceneName = SceneManager.GetActiveScene().name;
            LoadScene(loadSceneName);
#endif
        }

#if LOL_GAMEFRAME
        public static void LoadSceneSetReturn(string loadSceneName, string returnActionEvent, JSONObject returnActionData)
        {
            _returnApiAction = QuestAction.Create<MakeAPICallAction>();
            _returnApiAction.Event = returnActionEvent;
            _returnApiAction.data = returnActionData;
            LoadScene(loadSceneName);
        }

        static void ExecuteReturnAction()
        {
            if (_returnApiAction == null)
                return;

            GameController.GameSwitch(_returnApiAction).Then(() => _returnApiAction = null);
        }
#endif

        public static void LoadSceneSetReturn(string loadSceneName, string returnSceneName)
        {
#if LOL_GAMEFRAME
            LoadSceneSetReturn(loadSceneName, "load_scene_request", new JSONObject { ["scene_name"] = returnSceneName });
#else
            _returnSceneName = returnSceneName;
            LoadScene(loadSceneName);
#endif
        }

        static void LoadScene(string scene_name, LoadSceneMode load_scene_mode = LoadSceneMode.Single)
        {
#if LOL_GAMEFRAME
            var loadSceneApiCall = QuestAction.Create<MakeAPICallAction>();
            loadSceneApiCall.Event = "load_scene_request";
            loadSceneApiCall.data = new JSONObject { [nameof(scene_name)] = scene_name, [nameof(load_scene_mode)] = load_scene_mode.ToString() };
            GameController.GameSwitch(loadSceneApiCall);
            return;
#else
            // Load the scene normally, must be in the build index.
            SceneManager.LoadSceneAsync(scene_name, load_scene_mode);
#endif
        }

        #endregion Scene Loading
    }

#if LOL_GAMEFRAME
    [DeserializableType("state_load")]
    public class StateLoadAction : QuestAction<JSONObject>
    {
        protected override Promise Run()
        {
            State.OnLoaded(data);
            return Promise.Resolved();
        }

        public override void Deserialize(JSONNode jSONNode)
        {
            data = jSONNode[nameof(data)].AsObject;
            base.Deserialize(jSONNode);
        }

        public override void Reset()
        {
            data = null;
            base.Reset();
        }
    }
#endif
}