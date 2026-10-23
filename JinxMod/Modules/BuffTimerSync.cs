using System.Collections.Generic;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace JinxMod
{
    public static class BuffTimerSync
    {
        public static void Send(CharacterBody body, BuffIndex index)
        {
            if (!NetworkServer.active || !body || !body.networkIdentity) return;

            var timers = new List<float>();
            for (var i = 0; i < body.timedBuffs.Count; i++)
            {
                var timedBuff = body.timedBuffs[i];
                if (timedBuff.buffIndex == index) timers.Add(timedBuff.timer);
            }

            new SyncBuffTimersMessage(body.networkIdentity.netId, index, timers).Send(NetworkDestination.Clients);
        }

        public static IList<float> GetTimers(CharacterBody body, BuffIndex index)
        {
            var store = body ? body.GetComponent<JinxBuffTimers>() : null;
            return store ? store.GetTimers(index, body.GetBuffCount(index)) : new List<float>();
        }

        public static float GetLongest(CharacterBody body, BuffIndex index)
        {
            var timers = GetTimers(body, index);
            return timers.Count > 0 ? timers[0] : 0f;
        }
    }

    public class SyncBuffTimersMessage : INetMessage
    {
        private NetworkInstanceId netId;
        private BuffIndex index;
        private List<float> timers;

        public SyncBuffTimersMessage() { }

        public SyncBuffTimersMessage(NetworkInstanceId netId, BuffIndex index, List<float> timers)
        {
            this.netId = netId;
            this.index = index;
            this.timers = timers;
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(netId);
            writer.WritePackedUInt32((uint) index);
            writer.WritePackedUInt32((uint) timers.Count);
            foreach (var t in timers) writer.Write(t);
        }

        public void Deserialize(NetworkReader reader)
        {
            netId = reader.ReadNetworkId();
            index = (BuffIndex) reader.ReadPackedUInt32();
            var count = (int) reader.ReadPackedUInt32();
            timers = new List<float>(count);
            for (var i = 0; i < count; i++) timers.Add(reader.ReadSingle());
        }

        public void OnReceived()
        {
            var netObj = Util.FindNetworkObject(netId);
            if (!netObj) return;
            var body = netObj.GetComponent<CharacterBody>();
            if (!body) return;

            var store = body.GetComponent<JinxBuffTimers>();
            if (!store) store = body.gameObject.AddComponent<JinxBuffTimers>();
            store.Set(index, timers);
        }
    }

    public class JinxBuffTimers : MonoBehaviour
    {
        private readonly Dictionary<BuffIndex, List<float>> timers = new Dictionary<BuffIndex, List<float>>();

        public void Set(BuffIndex index, List<float> values)
        {
            var list = new List<float>(values);
            list.Sort((a, b) => b.CompareTo(a));
            timers[index] = list;
        }

        public IList<float> GetTimers(BuffIndex index, int actualStacks)
        {
            List<float> list;
            if (!timers.TryGetValue(index, out list)) return new List<float>();
            if (list.Count > actualStacks) list.RemoveRange(actualStacks, list.Count - actualStacks);
            return list;
        }

        private void Update()
        {
            var dt = Time.deltaTime;
            foreach (var list in timers.Values)
            {
                for (var i = list.Count - 1; i >= 0; i--)
                {
                    list[i] -= dt;
                    if (list[i] <= 0f) list.RemoveAt(i);
                }
            }
        }
    }
}
