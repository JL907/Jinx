using R2API.Networking.Interfaces;
using RoR2;
using UnityEngine.Networking;

namespace JinxMod
{
    public class RefreshStacksMessage : INetMessage
    {
        private CharacterBody body;
        private BuffIndex index;
        private float duration;
        private int maxStacks;
        bool setToMax;

        public RefreshStacksMessage(){}

        public RefreshStacksMessage(CharacterBody body, BuffIndex index, float duration, int maxStacks, bool setToMax = false)
        {
            this.body = body;
            this.index = index;
            this.duration = duration;
            this.maxStacks = maxStacks;
            this.setToMax = setToMax;
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(body.networkIdentity.netId);
            writer.WritePackedUInt32((uint) index);
            writer.Write(duration);
            writer.Write(maxStacks);
            writer.Write(setToMax);
        }

        public void Deserialize(NetworkReader reader)
        {
            var netObj = Util.FindNetworkObject(reader.ReadNetworkId());
            body = netObj.GetComponent<CharacterBody>();
            index = (BuffIndex) reader.ReadPackedUInt32();
            duration = reader.ReadSingle();
            maxStacks = reader.ReadInt32();
            setToMax = reader.ReadBoolean();
        }

        public void OnReceived()
        {
            body.RefreshStacks(index, duration, maxStacks, setToMax);
        }
    }
}