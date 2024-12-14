using System;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Linq;
using System.IO;
using UnityEngine.Assertions;
using System.Diagnostics;
using DG.Tweening;

namespace InduceVomitingMod;

public class AI_InduceVomiting : AI_TargetCard
{
    static bool warning = false;
    public override TargetType TargetType
	{
		get
		{
			return TargetType.SelfAndNeighbor;
		}
	}

    public override bool IsValidTC(Card c)
	{
		return !c.isThing && c.IsPCFactionOrMinion;
	}

	public override int MaxRadius
	{
		get
		{
			return 2;
		}
	}

	public override bool CanPerform()
	{
		return Act.TC != null;
	}

    public override bool Perform()
    {
		this.target = Act.TC;
		return base.Perform();
    }
    public override IEnumerable<AIAct.Status> Run()
	{
		Chara chara = this.target.Chara;
        if (chara == null)
        {
            Msg.SetColor("ono");
            Msg.SayRaw("vomit_say_0".lang());
            yield break;
        }
        if (chara.hunger.value >= 95)
        {
            Msg.SetColor("ono");
            Msg.SayRaw("vomit_say_1".lang());
            yield break;
        }
        if (chara.hunger.value > 70)
        {
            if (!warning)
            {
                Msg.SetColor("ono");
                Msg.SayRaw("vomit_say_2".lang());
                warning = true;
                yield break;
            }
        }
        else
        {
            warning = false;
        }
        
        // Msg.SayRaw($"Hunger: {chara.hunger.value}");
        Progress_Custom progress_Custom = new Progress_Custom();
        progress_Custom.canProgress = (() => (chara == null || chara.ExistsOnMap));
        progress_Custom.onProgressBegin = delegate()
        {
        };
        progress_Custom.onProgress = delegate(Progress_Custom p)
        {
            if (chara != null && this.owner.Dist(chara) > 1)
            {
                EClass.pc.TryMoveTowards(chara.pos);
                if (this.owner == null)
                {
                    p.Cancel();
                    return;
                }
                if (chara != null && this.owner.Dist(chara) > 1)
                {
                    EClass.pc.Say("targetTooFar", null, null);
                    p.Cancel();
                    return;
                }
            }
        };
        progress_Custom.onProgressComplete = delegate()
        {
            if (chara.hunger.value > 70)
            {
                // Offal, Meat, Bone
                chara.Say("vomit", chara, null, null);
                chara.PlaySound("vomit", 1f, true);
                if (!EClass._zone.IsRegion)
                {
                    string target = "";
                    if (EClass.rnd(2) == 0)
                    {
                        target = "offal";
                    }
                    else if (EClass.rnd(2) == 0)
                    {
                        target = "meat_marble";
                    }
                    else
                    {
                        target = "bone";
                    }
                    Thing thing = ThingGen.Create(target, -1, -1);
                    thing.MakeRefFrom(chara, null);
                    EClass._zone.AddCard(thing, chara.pos);
                    
                    if (EClass.rnd(2) == 0)
                    {
                        chara.ModWeight(-1 * EClass.rnd(3), false);
                        chara.AddBlood(10);
                    }
                }
                chara.AddCondition<ConDim>(200, false);
                chara.hunger.Mod(EClass.rndHalf((100 - chara.hunger.value)) / 2);
            }
            else
            {
                // Vomit
                // Porting form Chara.Vomit() and remove the ConAnorexia part
                chara.Say("vomit", chara, null, null);
                chara.PlaySound("vomit", 1f, true);
                if (!EClass._zone.IsRegion)
                {
                    Thing thing = ThingGen.Create("731", -1, -1);
                    thing.MakeRefFrom(chara, null);
                    EClass._zone.AddCard(thing, chara.pos);
                }
                chara.AddCondition<ConDim>(100, false);
                chara.hunger.Mod(30);
            }
            EClass.pc.stamina.Mod(-EClass.rnd(5));
        };
        Progress_Custom seq = progress_Custom.SetDuration(10 + EClass.rnd(10), 4);
        yield return base.Do(seq, null);
        yield break;
    }
}

//Game.Load
[HarmonyPatch(typeof(Scene), nameof(Scene.Init))]
class GameLoadPatch
{
    static void Postfix(Scene __instance, Scene.Mode newMode)
    {
        if (newMode == Scene.Mode.StartGame && EClass.player != null && EClass.player.chara != null && !EClass.player.chara.HasElement(6515))
        {
            EClass.player.chara.GainAbility(6515);
        }
    }
}