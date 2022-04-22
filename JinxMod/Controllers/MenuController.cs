﻿using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace JinxMod.Controller
{
    public class MenuController : MonoBehaviour
    {
        private uint playID;
        private uint playID2;

        private void OnDestroy()
        {
            if (this.playID != 0) AkSoundEngine.StopPlayingID(this.playID);
        }

        private void OnEnable()
        {
            this.Invoke("PlayEffect", 0.05f);
        }

        private void PlayEffect()
        {
            this.playID = Util.PlaySound("Play_JinxJokeVO", base.gameObject);
        }
    }
}