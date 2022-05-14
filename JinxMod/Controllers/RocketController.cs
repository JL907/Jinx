using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace JinxMod.Controller
{
    public class RocketController : MonoBehaviour
    {
        public bool isRocket;
        private CharacterBody body;
        public float stopwatch;
        public float random;
        public float attackspeed;
        public int attacks = 0;
        public void Start()
        {
            this.isRocket = false;
            this.body = base.GetComponent<CharacterBody>();
            random = UnityEngine.Random.Range(15 * this.body.attackSpeed, 30 * this.body.attackSpeed);
        }

        public void Update()
        {
            if (attacks > random)
            {
                attacks = 0;
                if (Modules.Config.voiceLines.Value) Util.PlaySound("Play_JinxAttackVO", base.gameObject);
                random = UnityEngine.Random.Range(15 * this.body.attackSpeed, 30 * this.body.attackSpeed);
            }
        }
    }
}
