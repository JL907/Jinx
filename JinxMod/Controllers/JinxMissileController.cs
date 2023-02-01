using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace JinxMod.Controller
{
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(ProjectileTargetComponent))]
	public class JinxMissileController : MonoBehaviour
	{
		private void Start()
		{
			this.targetComponent = base.GetComponent<ProjectileTargetComponent>();
		}

		private void Awake()
        {
			if (!NetworkServer.active)
			{
				enabled = false;
				return;
			}
		}

		private void FixedUpdate()
		{
			this.timer += Time.fixedDeltaTime;
			if (this.timer < this.giveupTimer)
			{
				if (this.targetComponent.target && this.timer >= this.delayTimer)
				{
					Vector3 vector = this.targetComponent.target.position + UnityEngine.Random.insideUnitSphere * this.turbulence - this.transform.position;
					if (vector != Vector3.zero)
					{
						this.transform.forward = Vector3.RotateTowards(this.transform.forward, vector, 720f * 0.017453292f * (Time.fixedDeltaTime * 2f), 0f);
					}
				}
			}
		}

		private ProjectileTargetComponent targetComponent;
		public float maxVelocity = 25f;
		public float rollVelocity = 0f;
		public float acceleration = 3f;
		public float delayTimer = 0.5f;
		public float giveupTimer = 8f;
		public float deathTimer = 10f;
		private float timer;
		public float turbulence = 2f;
	}
}
