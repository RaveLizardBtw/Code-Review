using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine;
using ThunderRoad;

namespace Vamp
{
    public class Vampiric : MonoBehaviour
    {
        private bool imbued = false;
        private bool onCooldown = false;
        private Item item;
        private AudioSource activationAudio;
        private AudioSource readyAudio;
        private SpellCastCharge vampSpell;
        public void Start()
        {
            item = GetComponent<Item>();
            vampSpell = Catalog.GetData<SpellCastCharge>("Vamp");
            item.OnDespawnEvent += Item_OnDespawnEvent;
            item.OnHeldActionEvent += Item_OnHeldActionEvent;
            activationAudio = item.GetCustomReference("activationAudio").gameObject.GetComponent<AudioSource>();
            readyAudio = item.GetCustomReference("readyAudio").gameObject.GetComponent<AudioSource>();
            EventManager.onCreatureHit += EventManager_onCreatureHit;
        }

        private void Item_OnHeldActionEvent(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
        {
            if (action == Interactable.Action.AlternateUseStart)
            {
                GameManager.local.StartCoroutine(Imbue());
            }
        }

        private void Item_OnDespawnEvent(EventTime eventTime)
        {
            if (eventTime == EventTime.OnStart)
            {
                item.OnHeldActionEvent -= Item_OnHeldActionEvent;
                item.OnDespawnEvent -= Item_OnDespawnEvent;
                EventManager.onCreatureHit -= EventManager_onCreatureHit;
            }
        }

        private void EventManager_onCreatureHit(Creature creature, CollisionInstance collisionInstance)
        {
            if (!imbued)
                return;
            Player.currentCreature.Heal(collisionInstance.damageStruct.damage * 0.3f, Player.currentCreature);
        }

        public void Update()
        {
            if (!imbued)
                return;
            foreach (Imbue imbue in item.imbues)
            {
                imbue.Transfer(vampSpell, imbue.maxEnergy);
            }
        }
        public IEnumerator Imbue()
        {
            if (!onCooldown)
            {
                onCooldown = true;
                activationAudio.Play();
                imbued = true;
                yield return new WaitForSeconds(10);
                GameManager.local.StartCoroutine(Cooldown());
                imbued = false;
            }
        }
        public IEnumerator Cooldown()
        {
            foreach (Imbue imbue in item.imbues)
            {
                imbue.energy = 0f;
            }
            yield return new WaitForSeconds(20);
            onCooldown = false;
            readyAudio.Play();
        }
    }
}
