﻿using Assets.Scripts.AI.Agents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Class tasked with mainly the control of the AI army.
/// this class will also control civils when they are either:
///     A: Exploring
///     B: Fighting for the motherland
/// </summary>
namespace Assets.Scripts.AI
{
    public class MicroManager
    {
		public const int AGENT_ATACK = 0;
		public const int AGENT_EXPLORER = 1;
		public const int AGENT_RETREAT = 2;
		public const int AGENT_ASSIST = 3;
        public const int AGENT_STRATEGY = 4;

        AIController ai;
        /// <summary>
        /// Commite of agents who will each vote at what to do with every squad
        /// </summary>
        public List<BaseAgent> agents;
        public List<SquadAI> squads;
        public MicroManager(AIController ai)
        {
            agents = new List<BaseAgent>();
            squads = new List<SquadAI>();
            this.ai = ai;
            AssistAgent assistAgent = new AssistAgent(ai, "Assist");
            AttackAgent aA = new AttackAgent(ai, assistAgent, "Atack");
            agents.Add(new ExplorerAgent(ai, assistAgent, "Explorer"));
            agents.Add(aA);
            agents.Add(new RetreatAgent(ai, aA, assistAgent, "Retreat"));
			agents.Add(assistAgent);
            agents.Add(new StrategyAgent(ai, assistAgent, "Strategy"));
            squads.Add(new SquadAI(1, ai));
            squads.Add(new SquadAI(2, ai));
        }
        /// <summary>
        /// Called pretty fast, it's just like Update()
        /// </summary>
        public void Micro()
        {
            //difficulty == 0 means the AI is disabled
            if (ai.DifficultyLvl > 0)
            {
                float bVal = float.MinValue;
                BaseAgent bAgent = agents[0];
                int val;
                foreach (SquadAI sq in squads)
                {
                    sq.recalculateSquadValues();
                    ((StrategyAgent)agents[AGENT_STRATEGY]).evaluateTimings();
                    foreach (BaseAgent a in agents)
                    {
                        val = a.getConfidence(sq);
                        if (AIController.AI_DEBUG_ENABLED) ai.aiDebug.setAgentConfidence(a.agentName, val);
                        if (val > bVal)
                        {
                            bVal = val;
                            bAgent = a;
                        }
                    }
                    sq.lastAgent = bAgent;
                    if (AIController.AI_DEBUG_ENABLED)
                    {
                        ai.aiDebug.setControllingAgent(bAgent.agentName, bVal);
                    }

                    bAgent.controlUnits(sq);
                    agents[AGENT_ASSIST].extraConfidence = 0;
                    ((AssistAgent)agents[AGENT_ASSIST]).clearRequests();
                }
            }
        }
        /// <summary>
        /// Outputs a different error based on the Ai difficulty level, only the easier difficulties should make errors
        /// </summary>
        /// <returns></returns>
        private float getError()
        {
            //TODO: improve when we can test it
            int error = Utils.D6.get.rollOnce();
            if (error > (ai.DifficultyLvl+3))
                return 6 / (ai.DifficultyLvl+1);
            return 0;
        }
        private void addSquad(List<Unit> units)
        {
            //Squad id 0 is used by temp squads
            SquadAI s = new SquadAI(squads.Count + 1, ai);
            s.addUnits(units);
            squads.Add(s);
        }
        public void setPersonality(List<float> rates)
        {
            if (rates.Count != agents.Count)
                Debug.LogError("setPersonality has different number of agents than personality rates");
            else
            {
                for (int i = 0; i < agents.Count; i++)
                    agents[i].modifier = rates[i];
            }
        }
        /// <summary>
        /// Boring personality, every agent has the same rate
        /// </summary>
        /// <param name="rate"></param>
        public void setPersonality(float rate)
        {
            for (int i = 0; i < agents.Count; i++)
                agents[i].modifier = rate;
        }

        public void OnUnitDead(Unit u)
        {
            foreach (SquadAI s in squads)
                if (s.units.Contains(u))
                    s.removeUnit(u);
        }
        /// <summary>
        /// Function tasked with deciding which squad should take care of the new created units
        /// </summary>
        /// <param name="u"></param>
        public void assignUnit(Unit u)
        {
            //TODO: placeholder until we know how to split the squads
            if (u.type == Storage.UnitTypes.HERO)
                squads[1].addUnit(u);
            else
                squads[0].addUnit(u);
        }
    }
}